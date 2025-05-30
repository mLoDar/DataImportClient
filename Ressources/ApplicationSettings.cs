namespace DataImportClient.Ressources
{
    internal class ApplicationSettings
    {
        internal class Runtime
        {
            internal readonly string appVersion = "v1.0.0";
            internal readonly string appRelease = "22.05.2025";
        }

        internal class Paths
        {
            internal readonly string appDataFolder;
            internal readonly string clientFolder;
            internal readonly string logsFolder;
            internal readonly string configurationFile;

            internal readonly string errorCacheFile;
            internal readonly string modulesFolder;

            internal readonly string weatherFolder;
            internal readonly string weatherImportWorkerLogs;
            
            internal readonly string electricityFolder;
            internal readonly string electricityImportWorkerLogs;
            internal readonly string electricityFaultyFilesFolder;

            internal readonly string districtHeatFolder;
            internal readonly string districtHeatImportWorkerLogs;
            internal readonly string districtHeatFaultyFilesFolder;

            internal readonly string photovoltaicFolder;
            internal readonly string photovoltaicImportWorkerLogs;
            internal readonly string photovoltaicFaultyFilesFolder;
            


            internal Paths()
            {
                appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                clientFolder = Path.Combine(appDataFolder, "DataImportClient");
                logsFolder = Path.Combine(clientFolder, "runtimeLogs");
                configurationFile = Path.Combine(clientFolder, "appConfiguration.json");

                errorCacheFile = Path.Combine(clientFolder, "errorCache.log");
                modulesFolder = Path.Combine(clientFolder, "Modules");

                weatherFolder = Path.Combine(modulesFolder, "Weather");
                weatherImportWorkerLogs = Path.Combine(weatherFolder, "ImportWorkerLogs");

                electricityFolder = Path.Combine(modulesFolder, "Electricity");
                electricityFaultyFilesFolder = Path.Combine(electricityFolder, "FaultyDataFiles");
                electricityImportWorkerLogs = Path.Combine(electricityFolder, "ImportWorkerLogs");

                districtHeatFolder = Path.Combine(modulesFolder, "DistrictHeat");
                districtHeatFaultyFilesFolder = Path.Combine(districtHeatFolder, "FaultyDataFiles");
                districtHeatImportWorkerLogs = Path.Combine(districtHeatFolder, "ImportWorkerLogs");

                photovoltaicFolder = Path.Combine(modulesFolder, "Photovoltaic");
                photovoltaicFaultyFilesFolder = Path.Combine(photovoltaicFolder, "FaultyDataFiles");
                photovoltaicImportWorkerLogs = Path.Combine(photovoltaicFolder, "ImportWorkerLogs");
            }
        }

        internal class Urls
        {
            internal readonly string solarwebLogin = "https://www.solarweb.com/Account/ExternalLogin";
            internal readonly string photovoltaicApi = "https://www.solarweb.com/ActualData/GetCompareDataForPvSystem?pvSystemId={solarwebSystemId}&_={currentUnixMillis}";
        }
    }
}