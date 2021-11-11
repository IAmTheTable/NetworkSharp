using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkFramework.Framework
{
    public static class Logger
    {
        public enum Loglevel
        {
            Verbose,
            Error,
            Warn,
        }
        public static bool VerboseLogging = false;
        public static bool WarningLogging = true;
        public static bool ErrorLogging = true;
        public static void Log(Loglevel _level, params object[] _data)
        {
            if (_level == Loglevel.Verbose && VerboseLogging)
            {
                Console.WriteLine($"[Verbose] {string.Join("", _data)}");
            }
            else if (_level == Loglevel.Warn && WarningLogging)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Warning] {string.Join("", _data)}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if(_level == Loglevel.Error && ErrorLogging)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] {string.Join("", _data)}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
