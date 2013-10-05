using System;

namespace Giparus.Core.Logger
{
    public class ConsoleLogProvider : ILog
    {
        private const ConsoleColor SUCCESS_COLOR = ConsoleColor.Green;
        private const ConsoleColor FAILIUR_COLOR = ConsoleColor.Red;
        private const ConsoleColor WARNING_COLOR = ConsoleColor.Yellow;
        //private const ConsoleColor INFO_COLOR = ConsoleColor.Blue;

        private readonly ConsoleColor _foreColor;

        public ConsoleLogProvider()
        {
            _foreColor = Console.ForegroundColor;
        }

        public void Initialize()
        {
            //nothing to initialize
        }

        public void Log(string message, int code = 0, LogType type = LogType.Inforamtion)
        {
            switch (type)
            {
                //case LogType.Inforamtion:
                //    Console.ForegroundColor = INFO_COLOR;
                //    break;
                case LogType.Failure:
                    Console.ForegroundColor = FAILIUR_COLOR;
                    break;
                case LogType.Success:
                    Console.ForegroundColor = SUCCESS_COLOR;
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = WARNING_COLOR;
                    break;
            }
            if (code == 0) Console.WriteLine(message);
            else Console.WriteLine("code:'{0}', {1}", code, message);

            Console.WriteLine(new String('-', 79));
            Console.ForegroundColor = _foreColor;
        }
    }
}
