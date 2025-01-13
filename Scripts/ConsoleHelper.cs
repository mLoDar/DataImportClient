using System.Globalization;
using System.Runtime.InteropServices;



#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time





namespace DataImportClient.Scripts
{
    internal class ConsoleHelper
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(nint hConsoleHandle, int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(nint hConsoleHandle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern nint GetStdHandle(int handle);



        internal static (bool successfullyEnabled, Exception occuredError) EnableAnsiSupport()
        {
            try
            {
                nint consoleHandle = GetStdHandle(-11);

                if (GetConsoleMode(consoleHandle, out int currentConsoleMode))
                {
                    SetConsoleMode(consoleHandle, currentConsoleMode | 0x0004);
                }
            }
            catch (Exception exception)
            {
                return (false, exception);
            }

            return (true, new Exception());
        }

        internal static bool ValidDecimalValues(string[] values)
        {
            foreach (string value in values)
            {
                if (decimal.TryParse(value, out decimal _) == false)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool ValidIntValues(string[] values)
        {
            foreach (string value in values)
            {
                if (int.TryParse(value, out int _) == false)
                {
                    return false;
                }
            }

            return true;
        }

        internal static async Task DisplayInformation(string title, string description, ConsoleColor titleColor, int durationInSeconds = 3)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 4);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             ┌───");
            Console.Write("             │ ");
            Console.ForegroundColor = titleColor;
            Console.WriteLine(title);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("             ├───────────────");
            Console.WriteLine("             │ {0}", description);
            Console.WriteLine("             └───");

            if (durationInSeconds <= 0 || durationInSeconds > 10)
            {
                await Task.Delay(5000);
                return;
            }

            await Task.Delay(durationInSeconds * 1000);
        }

        internal static bool TryToConvertDateTime(string sourceString, string format, out DateTime result)
        {
            return DateTime.TryParseExact(sourceString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
        }
    }
}