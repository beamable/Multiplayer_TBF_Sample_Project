using System;
using System.Collections.Generic;
using System.Text;
using Beamable.ConsoleCommands;
using Beamable.Service;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Console
{
   [HelpURL(ContentConstants.URL_FEATURE_ADMIN_FLOW)]
   public class DisruptorEngineConsole : MonoBehaviour
   {
        private static DisruptorEngineConsole _instance;
        private static Dictionary<string, ConsoleCommand> consoleCommandsByName = new Dictionary<string, ConsoleCommand>();

        public Canvas canvas;
        public Text txtOutput;
        public InputField txtInput;

        private bool isInitialized = false;
        private bool showNextTick = false;
        private bool isActive = false;

        private int fingerCount;
        private bool waitForRelease = false;
        private Vector2 averagePositionStart;

        async void Start()
        {
            if (_instance)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            DontDestroyOnLoad(gameObject);
            bool first = false;
            if (!ServiceManager.Exists<DBeamConsole>())
            {
                first = true;
                ServiceManager.ProvideWithDefaultContainer(new DBeamConsole());
            }

            var de = await API.Instance;

            bool enabled = false;

            if (ConsoleConfiguration.Instance.ForceEnabled)
            {
                enabled = true;
            }
            else
            {
                enabled = de.User.HasScope("cli:console");
            }

#if UNITY_EDITOR
            enabled = true;
#endif

            if (enabled)
            {
                var console = ServiceManager.Resolve<DBeamConsole>();
                console.OnLog += Log;
                if (first)
                {
                    console.OnExecute += ExecuteCommand;
                    console.OnCommandRegistered += RegisterCommand;
                    try
                    {
                        console.LoadCommands();
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                }

                txtInput.onEndEdit.AddListener((evt) =>
                {
                    if (txtInput.text.Length > 0)
                        Execute(txtInput.text);
                });

                isInitialized = true;
            }
        }

        private void Awake()
        {
            HideConsole();
        }

        void Update()
        {
            if (!isInitialized)
                return;

            if (showNextTick)
            {
                DoShow();
                showNextTick = false;
            }

            if (Input.GetKeyDown(ConsoleConfiguration.Instance.ToggleKey))
                ToggleConsole();
            else
            {
                int fingerCount = 0;
                Vector2 averagePosition = Vector2.zero;

                int touchCount = Input.touchCount;
                for (int i = 0; i < touchCount; ++i)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                    {
                        fingerCount++;
                        averagePosition += touch.position;
                    }
                }

                if ((fingerCount == 3) && !waitForRelease)
                {
                    averagePosition /= 3;
                    if (this.fingerCount != 3)
                    {
                        averagePositionStart = averagePosition;
                    }
                    else
                    {
                        if ((averagePositionStart - averagePosition).magnitude > 20.0f)
                        {
                            ToggleConsole();
                            waitForRelease = true;
                        }
                    }
                }
                else if ((fingerCount == 0) && waitForRelease)
                {
                    waitForRelease = false;
                }

                this.fingerCount = fingerCount;
            }
        }

        private void Execute(string txt)
        {
            if (!isActive)
            {
                return;
            }

            string[] parts = txt.Split(' ');
            txtInput.text = "";
            txtInput.Select();
            txtInput.ActivateInputField();
            if (parts.Length == 0)
                return;
            string command = parts[0];
            string[] args = new string[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
            {
                args[i - 1] = parts[i];
            }

            Log(ServiceManager.Resolve<DBeamConsole>().Execute(parts[0], args));
        }

        private void RegisterCommand(DBeamConsoleCommandAttribute command, ConsoleCommandCallback callback)
        {
            foreach (string name in command.Names)
            {
                ConsoleCommand cmd = new ConsoleCommand();
                cmd.command = command;
                cmd.callback = callback;
                consoleCommandsByName[name.ToLower()] = cmd;
            }
        }

        private string ExecuteCommand(string command, string[] args)
        {
            ConsoleCommand cmd;
            if (command == "help")
            {
                return OnHelp(args);
            }

            if (consoleCommandsByName.TryGetValue(command.ToLower(), out cmd))
            {
                string echoLine = "> " + command;
                foreach (string arg in args)
                {
                    echoLine += " " + arg;
                }

                Log(echoLine);
                return cmd.callback(args);
            }

            return "Unknown command";
        }

        private string OnHelp(params string[] args)
        {
            if (args.Length == 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Listing commands:");
                var uniqueCommands = new HashSet<ConsoleCommand>();
                var commands = consoleCommandsByName.Values;
                foreach (var command in commands)
                {
                    if (uniqueCommands.Contains(command))
                    {
                        continue;
                    }

                    uniqueCommands.Add(command);

                    string line = string.Format("{0} - {1}\n", command.command.Usage, command.command.Description);
                    Debug.Log(line);
                    builder.Append(line);
                }
                return builder.ToString();
            }

            string commandToGetHelpAbout = args[0].ToLower();
            ConsoleCommand found;
            if (consoleCommandsByName.TryGetValue(commandToGetHelpAbout, out found))
            {
                return string.Format("Help information about {0}\n\tDescription: {1}\n\tUsage: {2}",
                    commandToGetHelpAbout,
                    found.command.Description, found.command.Usage);
            }

            return string.Format("Cannot find help information about {0}. Are you sure it is a valid command?",
                commandToGetHelpAbout);
        }

        private void Log(string line)
        {
            Debug.Log(line);
            txtOutput.text += Environment.NewLine + line;
        }

        public void ToggleConsole()
        {
            if (isActive)
                HideConsole();
            else
                ShowConsole();
        }

        public void HideConsole()
        {
            isActive = false;
            txtInput.DeactivateInputField();
            txtInput.text = "";
            canvas.enabled = false;
        }

        public void ShowConsole()
        {
            if (!enabled)
            {
                Debug.LogWarning("Cannot open the console, because it isn't enabled");
                return;
            }

            showNextTick = true;
        }

        private void DoShow()
        {
            isActive = true;
            canvas.enabled = true;
            txtInput.text = "";
            txtInput.Select();
            txtInput.ActivateInputField();
        }

        private struct ConsoleCommand
        {
            public DBeamConsoleCommandAttribute command;
            public ConsoleCommandCallback callback;
        }
    }
}