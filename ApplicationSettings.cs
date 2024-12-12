namespace DataImportClient
{
    internal class ApplicationSettings
    {
        internal class Runtime
        {
            internal readonly string appVersion = "v1.0.0";
            internal readonly string appRelease = "TBA";

            internal readonly int processId;
            internal readonly string pathAppDataFolder;
            internal readonly string pathClientFolder;
            internal readonly string pathLogFile;



            internal Runtime()
            {
                DateTime now = DateTime.Now;
                string logFilename = $"runtime-{appVersion}-{now:dd-MM-yyyy}.log";

                processId = Environment.ProcessId;
                pathAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                pathClientFolder = Path.Combine(pathAppDataFolder, "DataImportClient");
                pathLogFile = Path.Combine(pathClientFolder, logFilename);
            }
        }
    }
}