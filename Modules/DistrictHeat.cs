using System.Diagnostics;
using System.Globalization;

using DataImportClient.Scripts;
using DataImportClient.Ressources;
using static DataImportClient.Ressources.ModuleConfigurations;

using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;





namespace DataImportClient.Modules
{
    internal class DistrictHeat
    {
        private const string _currentSection = "ModuleDisctrictHeat";

        private ModuleState _moduleState;
        private static bool _serviceRunning;
        private static int _errorCount;

        private static int _navigationXPosition = 1;
        private static readonly int _countOfMenuOptions = 4;

        private static string _dateOfLastImport = string.Empty;
        private static string _dateOfLastLogFileEntry = string.Empty;

        private static string _formattedErrorCount = string.Empty;
        private static string _formattedServiceRunning = string.Empty;
        private static string _formattedLastLogFileEntry = string.Empty;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private static Task _importWorker = new(() => { });

        private static CancellationTokenSource _cancellationTokenSource = new();

        private static readonly ApplicationSettings.Paths _appPaths = new();

        private static string _currentSourceFilePath = string.Empty;



        internal ModuleState State
        {
            get => _moduleState;
            set
            {
                if (_moduleState != value)
                {
                    ActivityLogger.Log(_currentSection, $"Module state changed from '{_moduleState}' to '{value}'.");
                    _moduleState = value;

                    OnStateChange();
                }
            }
        }

        internal event EventHandler? StateChanged;

        protected virtual void OnStateChange()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        internal int ErrorCount
        {
            get => _errorCount;
        }



        internal DistrictHeat()
        {
            _moduleState = ModuleState.Running;
            _errorCount = 0;
            _serviceRunning = true;

            _dateOfLastImport = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
            _dateOfLastLogFileEntry = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");



            _cancellationTokenSource = new();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            _importWorker = Task.Run(() => ImportPlcData(cancellationToken));
        }



        internal async Task Main()
        {
            ActivityLogger.Log(_currentSection, "Entering module 'DisctrictHeat'.");



            Console.Clear();

        LabelDrawUi:

            Console.SetCursorPosition(0, 4);



            ActivityLogger.Log(_currentSection, "Formatting menu variables.");

            FormatMenuVariables();

            ActivityLogger.Log(_currentSection, "Starting to draw the main menu.");

            DisplayMenu();

            ActivityLogger.Log(_currentSection, "Displayed main menu, waiting for key input.");



            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (_navigationXPosition + 1 <= _countOfMenuOptions)
                    {
                        _navigationXPosition += 1;
                        ActivityLogger.Log(_currentSection, $"Changed menu option from '{_navigationXPosition - 1}' to '{_navigationXPosition}'.");
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (_navigationXPosition - 1 >= 1)
                    {
                        _navigationXPosition -= 1;
                        ActivityLogger.Log(_currentSection, $"Changed menu option from '{_navigationXPosition + 1}' to '{_navigationXPosition}'.");
                    }
                    break;

                case ConsoleKey.Escape:
                    ActivityLogger.Log(_currentSection, "Returning to the main menu via 'ESC'.");
                    return;

                case ConsoleKey.Backspace:
                    ActivityLogger.Log(_currentSection, "Returning to the main menu via 'BACKSPACE'.");
                    return;

                default:
                    break;
            }



            if (pressedKey != ConsoleKey.Enter)
            {
                goto LabelDrawUi;
            }



            switch (_navigationXPosition)
            {
                case 1:
                    try
                    {
                        string importWorkerLogsFolder = _appPaths.districtHeatImportWorkerLogs;
                        Process.Start("explorer.exe", importWorkerLogsFolder);

                        ActivityLogger.Log(_currentSection, "Opened the folder for the import worker logs of the current module.");
                    }
                    catch (Exception exception)
                    {
                        ActivityLogger.Log(_currentSection, "[ERROR] Failed to open the folder for import worker logs of the current module.");
                        ActivityLogger.Log(_currentSection, exception.Message, true);

                        string title = "Failed to perform this action.";
                        string description = "Please check the error log for detailed information.";

                        await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);
                    }
                    break;

                case 2:
                    if (_serviceRunning == true)
                    {
                        ActivityLogger.Log(_currentSection, "Stopping the active import worker of the current module.");
                        ImportWorkerLog("Stopping the active import worker of the current module.");

                        _cancellationTokenSource?.Cancel();
                        _moduleState = ModuleState.Stopped;
                        _serviceRunning = false;
                        break;
                    }

                    ActivityLogger.Log(_currentSection, "Starting a new import worker for the current module.");
                    ImportWorkerLog(string.Empty, true);
                    ImportWorkerLog("Starting a new import worker for the current module.");



                    if (ErrorCount <= 0)
                    {
                        _moduleState = ModuleState.Running;
                    }

                    _cancellationTokenSource = new();
                    CancellationToken cancellationToken = _cancellationTokenSource.Token;

                    _serviceRunning = true;

                    _importWorker = Task.Run(() => ImportPlcData(cancellationToken));

                    break;

                case 3:
                    ActivityLogger.Log(_currentSection, $"Clearing errors for the current module. Previous error count: '{_errorCount}'.");

                    if (State != ModuleState.Stopped)
                    {
                        _moduleState = ModuleState.Running;
                    }

                    _errorCount = 0;

                    MainMenu._sectionMiscellaneous.errorCache.RemoveSectionFromCache(_currentSection);

                    break;

                case 4:
                    ActivityLogger.Log(_currentSection, "Returning to the main menu via selection.");
                    return;
            }



            Console.Clear();
            goto LabelDrawUi;
        }

        private static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("              ┳┓•     •    ┓┏                                  ");
            Console.WriteLine("              ┃┃┓┏┏╋┏┓┓┏╋  ┣┫┏┓┏┓╋                             ");
            Console.WriteLine("              ┻┛┗┛┗┗┛ ┗┗┗  ┛┗┗ ┗┻┗                             ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ────────────────────────                          ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             │   Last import:  {0}   │                         ", _dateOfLastImport);
            Console.WriteLine("             └─────────────────────────────────────────┘       ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Options                             State       ");
            Console.WriteLine("             └──────────────────────┐              ┌───┐       ");
            Console.WriteLine("             {0} Open log file                     │ {1}       ", $"[\u001b[91m{(_navigationXPosition == 1 ? ">" : " ")}\u001b[97m]", _formattedLastLogFileEntry);
            Console.WriteLine("             {0} {1}                     │ {2}       ", $"[\u001b[91m{(_navigationXPosition == 2 ? ">" : " ")}\u001b[97m]", $"{(_serviceRunning ? "Stop service " : "Start service")}", _formattedServiceRunning);
            Console.WriteLine("             {0} Clear errors                      │ {1}       ", $"[\u001b[91m{(_navigationXPosition == 3 ? ">" : " ")}\u001b[97m]", _formattedErrorCount);
            Console.WriteLine("                                                   └───┘       ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Application                                     ");
            Console.WriteLine("             └──────────────────────┐                          ");
            Console.WriteLine("             {0} MainMenu                                      ", $"[\u001b[91m{(_navigationXPosition == 4 ? ">" : " ")}\u001b[97m]");
        }

        private static void FormatMenuVariables()
        {
            _formattedErrorCount = "\u001b[92m√\u001b[97m │ \u001b[92mCleared\u001b[97m";
            _formattedServiceRunning = "\u001b[92m√\u001b[97m │ \u001b[92mRunning\u001b[97m";

            if (_errorCount > 0)
            {
                _formattedErrorCount = $"\u001b[91mx\u001b[97m │ \u001b[91m{_errorCount} {(_errorCount > 1 ? "Errors" : "Error")}\u001b[97m";
            }

            if (_serviceRunning == false)
            {
                _formattedServiceRunning = "\u001b[93mo\u001b[97m │ \u001b[93mStopped\u001b[97m";
            }



            if (!DateTime.TryParseExact(_dateOfLastLogFileEntry, "dd.MM.yyyy - HH:mm:ss", null, DateTimeStyles.None, out DateTime providedDateTime))
            {
                _formattedLastLogFileEntry = "\u001b[96m?\u001b[97m │ \u001b[96mUnknown\u001b[97m";
            }

            if (providedDateTime > DateTime.Now)
            {
                _formattedLastLogFileEntry = "\u001b[96m?\u001b[97m │ \u001b[96mUnknown\u001b[97m";
            }



            TimeSpan difference = DateTime.Now - providedDateTime;

            if (difference.TotalMinutes < 30)
            {
                _formattedLastLogFileEntry = $"\u001b[92m√\u001b[97m │ Updated at '\u001b[92m{_dateOfLastLogFileEntry}\u001b[97m'";

            }
            else if (difference.TotalMinutes >= 30 && difference.TotalMinutes < 60)
            {
                _formattedLastLogFileEntry = $"\u001b[93mo\u001b[97m │ Updated at '\u001b[93m{_dateOfLastLogFileEntry}\u001b[97m'";
            }
            else
            {
                _formattedLastLogFileEntry = $"\u001b[91mx\u001b[97m │ Updated at '\u001b[91m{_dateOfLastLogFileEntry}\u001b[97m'";
            }
        }

        private async Task ImportPlcData(CancellationToken cancellationToken)
        {
            ImportWorkerLog(string.Empty, true);
            ImportWorkerLog("Starting a new import worker for the current module.");

            int errorTimoutInMilliseconds = 5 * 30 * 1000;



            while (true)
            {
                ImportWorkerLog("Fetching settings from configuration file.");

                (DistrictHeatConfiguration districtHeatConfiguration, Exception? occurredError) = await GetConfigurationValues();

                if (occurredError != null)
                {
                    string errorMessage = "An error has occurred while fetching the settings.";
                    string[] errorDetails = [occurredError.Message, occurredError.InnerException?.ToString() ?? string.Empty];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.ConfigurationFetching);

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds / 1000} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully fetched settings.");



                string sourceFilePath = districtHeatConfiguration.sourceFilePath;
                string sourceFilePattern = districtHeatConfiguration.sourceFilePattern;

                if (int.TryParse(districtHeatConfiguration.sourceFileIntervalSeconds, out int sourceFileIntervalSeconds) == false)
                {
                    string errorMessage = "An error has occurred while assigning variables.";
                    string[] errorDetails = ["Failed to parse 'sourceFileIntervalSeconds' to int."];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.IntegerParsing);

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }



            LabelRestartAsMultipleFiles:



                ImportWorkerLog("Trying to fetch data from a PLC source file.");

                (List<string> sourceFileData, bool foundMultipleFiles, bool noFilesFound, occurredError) = await GetSourceFileData(sourceFilePath, sourceFilePattern);

                if (occurredError != null)
                {
                    string errorMessage = "An error has occurred while fetching data from the PLC source file.";
                    string[] errorDetails = [occurredError.Message, occurredError.InnerException?.ToString() ?? string.Empty];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.SourceFileDataFetching);

                    if (noFilesFound == false)
                    {
                        MoveSourceFileToFaultyFilesFolder();
                    }

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds / 1000} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully fetched the data set from a source file.");



                ImportWorkerLog("Inserting the fetched data set into the database.");

                string dbTableName = districtHeatConfiguration.dbTableName;
                string sqlConnectionString = districtHeatConfiguration.sqlConnectionString;

                occurredError = await InsertDataIntoDatabase(sqlConnectionString, dbTableName, sourceFileData, cancellationToken);

                if (occurredError != null)
                {
                    string errorMessage = "An error has occurred while inserting the data into the database.";
                    string[] errorDetails = [occurredError.Message, occurredError.InnerException?.ToString() ?? string.Empty];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.DatabaseInsertion);

                    if (noFilesFound == false)
                    {
                        MoveSourceFileToFaultyFilesFolder();
                    }

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds / 1000} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully inserted the data set into the database.");



                ImportWorkerLog("Trying to delete the current source file.");

                try
                {
                    File.Delete(_currentSourceFilePath);
                }
                catch (Exception exception)
                {
                    string errorMessage = "Failed to delete the source file.";
                    string[] errorDetails = [exception.Message, exception.InnerException?.ToString() ?? string.Empty];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.FileDeletion);
                }

                ImportWorkerLog("Successfully deleted the source file.");



                _dateOfLastImport = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
                _currentSourceFilePath = string.Empty;



                if (foundMultipleFiles == true)
                {
                    ImportWorkerLog($"Restarting import process immediately, as there were multiple source files.");
                    goto LabelRestartAsMultipleFiles;
                }



                ImportWorkerLog($"Going to sleep for {sourceFileIntervalSeconds} seconds.");
                await Task.Delay(sourceFileIntervalSeconds * 1000, cancellationToken);
            }
        }

        private static async Task<(DistrictHeatConfiguration districtHeatConfiguration, Exception? occurredError)> GetConfigurationValues()
        {
            JObject savedConfiguration;

            try
            {
                savedConfiguration = await ConfigurationHelper.LoadConfiguration();

                if (savedConfiguration["error"] != null)
                {
                    throw new Exception($"Saved configuration file contains errors. Error: {savedConfiguration["error"]}");
                }
            }
            catch (Exception exception)
            {
                return (new DistrictHeatConfiguration(), exception);
            }



            JObject modules;
            JObject districtHeatModule;
            JObject sqlData;

            try
            {
                modules = savedConfiguration["modules"] as JObject ?? [];

                if (modules == null || modules == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'modules' object.");
                }

                districtHeatModule = modules?["districtHeat"] as JObject ?? [];

                if (districtHeatModule == null || districtHeatModule == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'districtHeat' module.");
                }

                sqlData = savedConfiguration["sql"] as JObject ?? [];

                if (sqlData == null || sqlData == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'sql' object.");
                }
            }
            catch (Exception exception)
            {
                return (new DistrictHeatConfiguration(), exception);
            }



            try
            {
                DistrictHeatConfiguration districtHeatConfiguration = new()
                {
                    sourceFilePath = districtHeatModule?["sourceFilePath"]?.ToString() ?? string.Empty,
                    sourceFilePattern = districtHeatModule?["sourceFilePattern"]?.ToString() ?? string.Empty,
                    sourceFileIntervalSeconds = districtHeatModule?["sourceFileIntervalSeconds"]?.ToString() ?? string.Empty,
                    sqlConnectionString = sqlData?["connectionString"]?.ToString() ?? string.Empty,
                    dbTableName = districtHeatModule?["dbTableName"]?.ToString() ?? string.Empty,
                };

                if (districtHeatConfiguration.HoldsInvalidValues() == true)
                {
                    throw new Exception("One or mulitple configuration values are null. Please check the configuration file!");
                }

                if (int.TryParse(districtHeatConfiguration.sourceFileIntervalSeconds, out int _) == false)
                {
                    throw new Exception("Failed to parse the provided source file interval to a number.");
                }

                return (districtHeatConfiguration, null);
            }
            catch (Exception exception)
            {
                return (new DistrictHeatConfiguration(), exception);
            }
        }

        private static async Task<(List<string> sourceData, bool foundMultipleFiles, bool noFilesFound, Exception? occurredError)> GetSourceFileData(string sourceFilePath, string sourceFilePattern)
        {
            bool multipleSourceFilesFound = false;

            if (Directory.Exists(sourceFilePath) == false)
            {
                return ([], multipleSourceFilesFound, true, new Exception("Failed to find the source file folder path specified in the configuration."));
            }



            List<string> fileMatches = [];
            string[] filesInSourcePath = Directory.GetFiles(sourceFilePath);

            foreach (string file in filesInSourcePath)
            {
                string fileName = sourceFilePattern.Split(".")[0].ToLower();
                string fileExtension = $".{sourceFilePattern.Split(".")[1]}";

                if (file.EndsWith(fileExtension) == false)
                {
                    continue;
                }

                if (file.Split(@"\").Last().Contains(fileName, StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    continue;
                }

                fileMatches.Add(file);
            }



            if (fileMatches.Count <= 0)
            {
                return ([], multipleSourceFilesFound, true, new Exception("Failed to find any source files which match the configuration."));
            }

            if (fileMatches.Count > 1)
            {
                multipleSourceFilesFound = true;
            }



            _currentSourceFilePath = fileMatches[0];

            string[] sourceFileData;

            try
            {
                sourceFileData = await File.ReadAllLinesAsync(_currentSourceFilePath);
            }
            catch (Exception exception)
            {
                return ([], multipleSourceFilesFound, false, exception);
            }



            List<string> finalSourceFileData = [];

            for (int i = 0; i < sourceFileData.Length - 1; i++)
            {
                string currentRow = sourceFileData[i];
                currentRow = RegexPatterns.AllWhitespaces().Replace(currentRow, string.Empty);

                if (currentRow.Equals(string.Empty) == false)
                {
                    finalSourceFileData.Add(currentRow);
                }
            }



            return (finalSourceFileData, multipleSourceFilesFound, false, null);
        }

        private static async Task<Exception?> InsertDataIntoDatabase(string sqlConnectionString, string dbTableName, List<string> sourceData, CancellationToken cancellationToken)
        {
            ImportWorkerLog("Trying to establish a database connection.");

            SqlConnection databaseConnection;

            try
            {
                if (sqlConnectionString.Contains("connect timeout", StringComparison.CurrentCultureIgnoreCase) == false)
                {
                    sqlConnectionString += "Connect Timeout=5;";
                }

                databaseConnection = new(sqlConnectionString);

                await databaseConnection.OpenAsync(cancellationToken);
            }
            catch (SqlException exception)
            {
                if (exception.Number == -2)
                {
                    return new Exception("Failed to establish a database connection due to a timeout.");
                }

                return new Exception("Failed to establish a database connection. " + exception.Message);
            }
            catch (Exception exception)
            {
                return new Exception("An error occurred while connection to the database. " + exception.Message);
            }

            ImportWorkerLog("Successfully established a database connection.");



            ImportWorkerLog($"Inserting a total of '{sourceData.Count}' entries into the database.");

            while (sourceData.Count > 0)
            {
                string currentDataRow = sourceData[0];
                
                if (RegexPatterns.AllWhitespaces().Replace(currentDataRow, string.Empty).Equals(string.Empty))
                {
                    sourceData.RemoveAt(0);
                    continue;
                }

                string[] importValues = currentDataRow.Split(';');



                int zaehlerId;

                decimal energie;
                decimal volumen;
                decimal leistung;
                decimal durchfluss;
                decimal vorlauf;
                decimal ruecklauf;

                DateTime heatImportDate;
                DateTime heatImportTime;



                try
                {
                    zaehlerId = Convert.ToInt32(importValues[0]);

                    volumen = Convert.ToDecimal(importValues[1]);
                    energie = Convert.ToDecimal(importValues[2]);
                    leistung = Convert.ToDecimal(importValues[3]);
                    durchfluss = Convert.ToDecimal(importValues[4]);
                    vorlauf = Convert.ToDecimal(importValues[5]);
                    ruecklauf = Convert.ToDecimal(importValues[6]);
                }
                catch (Exception exception)
                {
                    return new Exception("Failed to convert PLC data. " + exception.Message);
                }

                try
                {
                    bool parsedDate = ConsoleHelper.TryToConvertDateTime(importValues[7], "yyyy.M.d", out heatImportDate);
                    bool parsedTime = ConsoleHelper.TryToConvertDateTime(importValues[8], "H:m:s", out heatImportTime);

                    if (parsedDate == false)
                    {
                        throw new Exception($"Failed to parse the provided date. Received date: '{importValues[7]}'.");
                    }

                    if (parsedTime == false)
                    {
                        throw new Exception($"Failed to parse the provided time. Received time: '{importValues[8]}'.");
                    }
                }
                catch (Exception exception)
                {
                    return exception;
                }


                
                try
                {
                    string queryNames = "heatimport_date, heatimport_time, zaehler_id, energie, volumen, leistung, durchfluss, vorlauf, ruecklauf";
                    string queryValues = "@heatimport_date, @heatimport_time, @zaehler_id, @energie, @volumen, @leistung, @durchfluss, @vorlauf, @ruecklauf";
                    string queryInsert = $"INSERT INTO {dbTableName} ({queryNames}) VALUES ({queryValues});";

                    using SqlCommand insertCommand = new(queryInsert, databaseConnection);

                    insertCommand.Parameters.AddWithValue("@heatimport_date", heatImportDate);
                    insertCommand.Parameters.AddWithValue("@heatimport_time", heatImportTime);
                    insertCommand.Parameters.AddWithValue("@zaehler_id", zaehlerId);
                    insertCommand.Parameters.AddWithValue("@energie", energie);
                    insertCommand.Parameters.AddWithValue("@volumen", volumen);
                    insertCommand.Parameters.AddWithValue("@leistung", leistung);
                    insertCommand.Parameters.AddWithValue("@durchfluss", durchfluss);
                    insertCommand.Parameters.AddWithValue("@vorlauf", vorlauf);
                    insertCommand.Parameters.AddWithValue("@ruecklauf", ruecklauf);

                    await insertCommand.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    return new Exception($"Failed to import a row of the data set. Remaining elements: '{sourceData.Count}'. " + exception.Message);
                }

                sourceData.RemoveAt(0);
            }



            return null;
        }

        private static void ImportWorkerLog(string message, bool removePrefix = false)
        {
            ImportLogger.Log(_currentSection, message, removePrefix);
            _dateOfLastLogFileEntry = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
        }

        private void ThrowModuleError(string errorMessage, string[] errorDetails, ErrorCategory errorCategory)
        {
            ImportWorkerLog($"[ERROR] - {errorMessage}");

            foreach (string errorDetail in errorDetails)
            {
                if (string.IsNullOrWhiteSpace(errorDetail) == false)
                {
                    ImportWorkerLog(errorDetail, true);
                }
            }

            MainMenu._sectionMiscellaneous.errorCache.AddEntry(_currentSection, errorMessage, errorDetails[0], errorCategory);

            State = ModuleState.Error;
            _errorCount++;
        }

        private void MoveSourceFileToFaultyFilesFolder()
        {
            ImportWorkerLog("Trying to move the current source file to faulty files folder.");

            try
            {
                string unixTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

                string districtHeatFaultyFilesFolder = _appPaths.districtHeatFaultyFilesFolder;
                string destinationFile = Path.Combine(districtHeatFaultyFilesFolder, $"dataset_{unixTimestampSeconds}.csv");

                File.Move(_currentSourceFilePath, destinationFile);

                _currentSourceFilePath = string.Empty;

                ImportWorkerLog("Successfully moved the source file.");
            }
            catch (Exception exception)
            {
                string errorMessage = "Failed to move the current source file.";
                string[] errorDetails = [exception.Message, exception.InnerException?.ToString() ?? string.Empty, $"File path: {_currentSourceFilePath}."];
                ThrowModuleError(errorMessage, errorDetails, ErrorCategory.FileMoving);
            }
        }
    }
}