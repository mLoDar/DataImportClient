using DataImportClient.Ressources;





namespace DataImportClient.Scripts
{
    internal class ActivityLogger
    {
        private static readonly ApplicationSettings.Paths _appPaths = new();
        private static readonly ApplicationSettings.Runtime _appRuntime = new();



        internal static void Log(string currentSection, string message, bool removePrefix = false)
        {
            try
            {
                string clientFolder = _appPaths.clientFolder;
                string logsFolder = _appPaths.logsFolder;
                string logFileName = _appPaths.logFileName;

                if (Directory.Exists(clientFolder) == false)
                {
                    Directory.CreateDirectory(clientFolder);
                }

                if (Directory.Exists(logsFolder) == false)
                {
                    Directory.CreateDirectory(logsFolder);
                }

                string logFile = Path.Combine(logsFolder, logFileName);
                string prefix = $"[{DateTime.Now}] - [ProcessId: {Environment.ProcessId}] - [Section: {currentSection}] - ";

                if (removePrefix == true)
                {
                    File.AppendAllText(logFile, $"{new string(' ', prefix.Length)}{message}\r\n");
                    return;
                }

                File.AppendAllText(logFile, $"{prefix}{message}\r\n");
            }
            catch
            {
                Console.Clear();
                Console.SetCursorPosition(0, 4);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("             WARNING\r\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             Failed to create crucial folders/files.");
                Console.WriteLine("             Please read the manual on how to fix this error!");
                Thread.Sleep(10000);

                Environment.Exit(0);
            }
        }
    }
}