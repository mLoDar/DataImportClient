using System.Text;

using DataImportClient.Scripts;
using DataImportClient.Ressources;

using Newtonsoft.Json.Linq;





namespace DataImportClient
{
    internal class EntryPoint
    {
        private const string currentSection = "EntryPoint";

        private static readonly ApplicationSettings.Paths appPaths = new();
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



            ActivityLogger.Log(currentSection, "Searching for all required folders/files.");
            (bool successfullyCreated, occuredError) = await CreateDiskFolderStructure();

            if (successfullyCreated == false)
            {
                ActivityLogger.Log(currentSection, "[ERROR] Failed to create required folder/file structure.");
                ActivityLogger.Log(currentSection, occuredError.Message, true);

                Console.SetCursorPosition(0, 4);
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("             WARNING\r\n");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             Failed to create the required folder/file structure");
                Console.WriteLine("             This structure is cruicial for the application to work.\r\n");
                Console.WriteLine("             Please read the manual on how to fix this error!");

                Thread.Sleep(5000);

                Environment.Exit(0);
            }

            ActivityLogger.Log(currentSection, "All required folders/files exist at the correct path!");



            Console.CursorVisible = false;
            Console.Title = "DataImportClient";
            Console.OutputEncoding = Encoding.UTF8;
            


            await MainMenu.Main();
            


            ActivityLogger.Log(currentSection, "Shutting down DataImportClient ...");

            Environment.Exit(0);
        }

        private static async Task<(bool successfullyCreated, Exception occuredError)> CreateDiskFolderStructure()
        {
            string appDataFolder = appPaths.appDataFolder;
            string configurationFile = appPaths.configurationFile;

            List<string> foldersToCreate =
            [
                appPaths.modulesFolder,
                appPaths.weatherFolder,
                appPaths.weatherFaultyFilesFolder,
                appPaths.electricityFolder,
                appPaths.electricityFaultyFilesFolder,
                appPaths.districtHeatFolder,
                appPaths.districtHeatFaultyFilesFolder,
                appPaths.photovoltaicFolder,
                appPaths.photovoltaicFaultyFilesFolder
            ];



            foreach (string folder in foldersToCreate)
            {
                try
                {
                    if (Directory.Exists(folder) == false)
                    {
                        Directory.CreateDirectory(folder);

                        ActivityLogger.Log(currentSection, $"Created folder: {folder.Replace(appDataFolder, ".")}");
                    }

                    await Task.Delay(50);
                }
                catch (Exception exception)
                {
                    return (false, exception);
                }
                
            }



            try
            {
                if (File.Exists(configurationFile) == false)
                {
                    JObject appConfiguration = [];

                    JArray modules =
                    [
                        new JObject()["weather"] = new JObject()
                        {
                            ["sourceFilePath"] = "path/to/source/file.csv",
                            ["dbTableName"] = "tableNameforImport",
                            ["refreshTimeInSeconds"] = "intervalInWhichTheSourceDataArrives"
                        },
                        new JObject()["electricity"] = new JObject()
                        {
                            ["sourceFilePath"] = "path/to/source/file.csv",
                            ["dbTableName"] = "tableNameforImport",
                            ["refreshTimeInSeconds"] = "intervalInWhichTheSourceDataArrives"
                        },
                        new JObject()["districtHeat"] = new JObject()
                        {
                            ["sourceFilePath"] = "path/to/source/file.csv",
                            ["dbTableName"] = "tableNameforImport",
                            ["refreshTimeInSeconds"] = "intervalInWhichTheSourceDataArrives"
                        },
                        new JObject()["photovoltaic"] = new JObject()
                        {
                            ["sourceFilePath"] = "path/to/source/file.csv",
                            ["dbTableName"] = "tableNameforImport",
                            ["refreshTimeInSeconds"] = "intervalInWhichTheSourceDataArrives"
                        },
                        new JObject()["sql"] = new JObject()
                        {
                            ["connectionString"] = "theConnectionStringForTheImportDatabase"
                        }
                    ];

                    appConfiguration["modules"] = modules;
                    appConfiguration["sql"] = new JObject()
                    {
                        ["connectionString"] = "theConnectionStringForTheImportDatabase"
                    };
                    appConfiguration["emailsToAlert"] = new JArray();



                    await File.WriteAllTextAsync(configurationFile, appConfiguration.ToString());

                    ActivityLogger.Log(currentSection, $"Created file: {configurationFile.Replace(appDataFolder, ".")}");
                }
            }
            catch (Exception exception)
            {
                return (false, exception);
            }
            


            return (true, new Exception());
        }
    }
}