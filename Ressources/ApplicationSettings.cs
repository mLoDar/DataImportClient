namespace DataImportClient.Ressources
{
    internal class ApplicationSettings
    {
        internal class Runtime
        {
            internal readonly string appVersion = "v1.0.0";
            internal readonly string appRelease = "TBA";

            internal readonly int processId = Environment.ProcessId;
        }

        internal class Paths
        {
            private static readonly Runtime _runtime = new();

            internal readonly string appDataFolder;
            internal readonly string clientFolder;
            internal readonly string logsFolder;
            internal readonly string logFile;
            internal readonly string configurationFile;

            internal readonly string errorCacheFile;
            internal readonly string modulesFolder;

            internal readonly string weatherFolder;
            internal readonly string weatherLogFile;
            internal readonly string weatherFaultyFilesFolder;

            internal readonly string electricityFolder;
            internal readonly string electricityLogFile;
            internal readonly string electricityFaultyFilesFolder;

            internal readonly string districtHeatFolder;
            internal readonly string districtHeatLogFile;
            internal readonly string districtHeatFaultyFilesFolder;

            internal readonly string photovoltaicFolder;
            internal readonly string photovoltaicLogFile;
            internal readonly string photovoltaicFaultyFilesFolder;



            internal Paths()
            {
                DateTime now = DateTime.Now;
                string logFilename = $"runtime-{_runtime.appVersion}-{now:dd-MM-yyyy}.log";

                appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                clientFolder = Path.Combine(appDataFolder, "DataImportClient");
                logsFolder = Path.Combine(clientFolder, "runtimeLogs");
                logFile = Path.Combine(logsFolder, logFilename);
                configurationFile = Path.Combine(clientFolder, "appConfiguration.json");

                errorCacheFile = Path.Combine(clientFolder, "errorCache.log");
                modulesFolder = Path.Combine(clientFolder, "Modules");

                weatherFolder = Path.Combine(modulesFolder, "Weather");
                weatherLogFile = Path.Combine(weatherFolder, logFilename);
                weatherFaultyFilesFolder = Path.Combine(weatherFolder, "FaultyDataFiles");

                electricityFolder = Path.Combine(modulesFolder, "Electricity");
                electricityLogFile = Path.Combine(electricityFolder, logFilename);
                electricityFaultyFilesFolder = Path.Combine(electricityFolder, "FaultyDataFiles");

                districtHeatFolder = Path.Combine(modulesFolder, "DistrictHeat");
                districtHeatLogFile = Path.Combine(districtHeatFolder, logFilename);
                districtHeatFaultyFilesFolder = Path.Combine(districtHeatFolder, "FaultyDataFiles");

                photovoltaicFolder = Path.Combine(modulesFolder, "Photovoltaic");
                photovoltaicLogFile = Path.Combine(photovoltaicFolder, logFilename);
                photovoltaicFaultyFilesFolder = Path.Combine(photovoltaicFolder, "FaultyDataFiles");
            }
        }
    }
}