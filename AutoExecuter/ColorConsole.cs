using System;

namespace SilentOrbit.AutoExecuter
{
    public static class ColorConsole
    {
        public static void WriteLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}

