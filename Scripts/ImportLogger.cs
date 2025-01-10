using DataImportClient.Ressources;





namespace DataImportClient.Scripts
{
    internal class ImportLogger
    {
        private static readonly ApplicationSettings.Paths _appPaths = new();



        internal static void Log(string currentSection, string message, bool removePrefix = false)
        {
            try
            {
                string clientFolder = _appPaths.clientFolder;
                string logFileName = _appPaths.logFileName;
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

                if (removePrefix == true)
                {
                    File.AppendAllText(logFile, $"{new string(' ', prefix.Length)}{message}\r\n");
                    return;
                }

                File.AppendAllText(logFile, $"{prefix}{message}\r\n");
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