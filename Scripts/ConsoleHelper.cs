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
    }
}