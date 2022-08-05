/* 
 *  Extensions.cs
 *  Author: RealNickk
*/

using System;

namespace Starlight.Cui
{
    public static class ConsoleExtensions
    {
        public static bool ShowDialog(string szMessage)
        {
            Console.Write(szMessage + " (Y/n) ");
            ConsoleKey resp = Console.ReadKey(false).Key;
            if (resp != ConsoleKey.Enter) Console.WriteLine();
            if (resp != ConsoleKey.Y)
                return false;
            return true;
        }
    }
}
