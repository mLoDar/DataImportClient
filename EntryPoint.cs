using System.Text;

using DataImportClient.Scripts;
using DataImportClient.Ressources;





namespace DataImportClient
{
    internal class EntryPoint
    {
        private const string currentSection = "EntryPoint";

        private static readonly ApplicationSettings.Runtime appRuntime = new();



        static async Task Main()
        {
            string appVersion = appRuntime.appVersion;
            string appRelease = appRuntime.appRelease;

            ActivityLogger.Log(currentSection, string.Empty, true);
            ActivityLogger.Log(currentSection, "Starting DataImportClient (C) Made in Austria");
            ActivityLogger.Log(currentSection, $"Version '{appVersion}' | Release '{appRelease}'");



            ActivityLogger.Log(currentSection, "Trying to enable support for ANSI escape sequence.");
            (bool ansiSupportEnabled, Exception occuredError) = ConsoleHelper.EnableAnsiSupport();



            if (ansiSupportEnabled == false)
            {
                ActivityLogger.Log(currentSection, "[ERROR] Failed to enable ANSI support.");
                ActivityLogger.Log(currentSection, occuredError.Message, true);

                Console.SetCursorPosition(0, 4);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("             WARNING\r\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             Failed to enable support for ANSI escape sequences.");
                Console.WriteLine("             This will have side effects on the coloring within the console.\r\n");
                Console.WriteLine("             Please read the manual on how to fix this error!");

                Thread.Sleep(5000);
            }
            else
            {
                ActivityLogger.Log(currentSection, "Successfully enabled ANSI support!");
            }



            Console.CursorVisible = false;
            Console.Title = "DataImportClient";
            Console.OutputEncoding = Encoding.UTF8;



            await MainMenu.Main();
            


            ActivityLogger.Log(currentSection, "Shutting down DataImportClient ...");

            Environment.Exit(0);
        }
    }
}