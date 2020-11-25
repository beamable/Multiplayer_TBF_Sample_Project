using System;
using UnityEngine.Scripting;

namespace Beamable.ConsoleCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DBeamConsoleCommandAttribute : PreserveAttribute
    {
        public string[] Names { get; }
        public string Description { get; }
        public string Usage { get; }

        public DBeamConsoleCommandAttribute(string name, string description, string usage)
        {
            Names = new[] {name.ToUpperInvariant()};
            Description = description;
            Usage = usage;
        }

        public DBeamConsoleCommandAttribute(string[] names, string description, string usage)
        {
            Names = Array.ConvertAll(names, x => x.ToUpperInvariant());
            Description = description;
            Usage = usage;
        }
    }
}
