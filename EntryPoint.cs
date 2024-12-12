namespace DataImportClient
{
    internal class EntryPoint
    {
        private const string currentSection = "EntryPoint";

        private static readonly ApplicationSettings.Runtime appRuntime = new();



        static void Main()
        {
            string appVersion = appRuntime.appVersion;
            string appRelease = appRuntime.appRelease;

            ActivityLogger.Log(currentSection, string.Empty, true);
            ActivityLogger.Log(currentSection, "Starting DataImportClient (C) Made in Austria");
            ActivityLogger.Log(currentSection, $"Version: '{appVersion}' | Release '{appRelease}'");

            // TODO: Forward to main menu

            ActivityLogger.Log(currentSection, "Shutting down DataImportClient ...");

            Environment.Exit(0);
        }
    }
}