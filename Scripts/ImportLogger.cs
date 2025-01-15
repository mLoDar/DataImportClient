using DataImportClient.Ressources;





namespace DataImportClient.Scripts
{
    internal class ImportLogger
    {
        private static readonly ApplicationSettings.Paths _appPaths = new();
        private static readonly ApplicationSettings.Runtime _runtime = new();



        internal static void Log(string currentSection, string message, bool removePrefix = false)
        {
            try
            {
                DateTime now = DateTime.Now;

                string clientFolder = _appPaths.clientFolder;
                string logFileName = $"runtime-{_runtime.appVersion}-{now:dd-MM-yyyy}.log";
                string logsFolder = string.Empty;
                


                switch (currentSection)
                {
                    case "ModuleWeather":
                        logsFolder = _appPaths.weatherImportWorkerLogs;
                        break;

                    case "ModuleDisctrictHeat":
                        logsFolder = _appPaths.districtHeatImportWorkerLogs;
                        break;

                    case "ModuleElectricity":
                        logsFolder = _appPaths.electricityImportWorkerLogs;
                        break;

                    case "ModulePhotovoltaic":
                        logsFolder = _appPaths.photovoltaicImportWorkerLogs;
                        break;
                }

                string logFile = Path.Combine(logsFolder, logFileName);
                string prefix = $"[{DateTime.Now}] - [ProcessId: {Environment.ProcessId}] - [Section: {currentSection}] - ";

                using FileStream fs = new(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using StreamWriter writer = new(fs);



                if (removePrefix == true)
                {
                    writer.WriteLine($"{new string(' ', prefix.Length)}{message}");
                    return;
                }

                writer.WriteLine($"{prefix}{message}");
            }
            catch
            {
                string exception = "[CRITICAL ERROR] - Failed to log message for import logger.";
                ActivityLogger.Log("ImportLogger", exception);

                exception = $"Affected section: '{currentSection}' |  Message: '{message}'";
                ActivityLogger.Log("ImportLogger", exception, true);
            }
        }
    }
}