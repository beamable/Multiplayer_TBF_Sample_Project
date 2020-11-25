using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Beamable.Service;
using UnityEngine;
using UnityEngine.Scripting;

namespace Beamable.ConsoleCommands
{
    [DBeamConsoleCommandProvider]
    public class DefaultConsoleCommands
    {
        private DBeamConsole Console => ServiceManager.Resolve<DBeamConsole>();

        [Preserve]
        public DefaultConsoleCommands()
        {
        }


        [DBeamConsoleCommand("ECHO", "Repeat message to console.", "ECHO <message>")]
        private string Echo(params string[] args)
        {
            return string.Join(" ", args);
        }

        [DBeamConsoleCommand(nameof(Where), "Find where a specific console command was registered from, if it was registered with a DBeamConsoleCommand attribute", "WHERE <command>")]
        private string Where(params string[] args)
        {
            if (args.Length == 0)
            {
                return Console.Help(nameof(Where));
            }

            return Console.Origin(args[0]);
        }

    }
}