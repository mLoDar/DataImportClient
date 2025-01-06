using System.Globalization;

using DataImportClient.Scripts;
using DataImportClient.Ressources;

using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;





namespace DataImportClient.Modules
{
    internal struct ElectricityConfiguration
    {
        internal string sourceFilePath;
        internal string sourceFilePattern;
        internal string sourceFileInterval;
        internal string sqlConnectionString;
        internal string dbTableNamePower;
        internal string dbTableNamePowerfactor;


        
        internal readonly bool HoldsInvalidValues()
        {
            var stringFields = new string[] { sourceFilePath, sourceFilePattern, sourceFileInterval, sqlConnectionString, dbTableNamePower, dbTableNamePowerfactor };
            return stringFields.Any(string.IsNullOrEmpty);
        }
    }



    internal class Electricity
    {
        private const string _currentSection = "ModuleElectricity";

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

        private static Task _importWorker = new(() => { });
        private static CancellationTokenSource _cancellationTokenSource = new();



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



        internal int ErrorCount
        {
            get => _errorCount;
        }



        internal Electricity()
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
            ActivityLogger.Log(_currentSection, "Entering module 'Electricity'.");



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
                    break;

                case 2:
                    break;

                case 3:
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
            Console.WriteLine("              ┏┓┓      • •                                     ");
            Console.WriteLine("              ┣ ┃┏┓┏╋┏┓┓┏┓╋┓┏                                  ");
            Console.WriteLine("              ┗┛┗┗ ┗┗┛ ┗┗┗┗┗┫                                  ");
            Console.WriteLine("                            ┛                                  ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ───────────────────                               ");
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
                
                (ElectricityConfiguration electricityConfiguration, Exception? occuredError) = await GetConfigurationValues();

                if (occuredError != null)
                {
                    string errorMessage = "An error has occured while fetching the settings.";
                    ThrowModuleError(errorMessage, occuredError.Message);

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully fetched settings.");



                string sourceFilePath = electricityConfiguration.sourceFilePath;
                string sourceFilePattern = electricityConfiguration.sourceFilePattern;
                string sourceFileInterval = electricityConfiguration.sourceFileInterval;

                int apiSleepTimer = Convert.ToInt32(sourceFileInterval) * 1000;



            LabelRestartAsMultipleFiles:



                ImportWorkerLog("Trying to fetch data from a PLC source file.");

                (List<string> sourceFileData, bool foundMultipleFiles, occuredError) = await GetSourceFileData(sourceFilePath, sourceFilePattern);

                if (occuredError != null)
                {
                    string errorMessage = "An error has occured while fetching data form the PLC source file.";
                    ThrowModuleError(errorMessage, occuredError.Message);

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully fetched the data set from a source file.");



                ImportWorkerLog("Minimizing the fetched data.");

                (List<string> minimizedSourceData, occuredError) = MinimizeSourceFileData(sourceFileData);

                if (occuredError != null)
                {
                    string errorMessage = "An error has occured while minimizing the fetched data.";
                    ThrowModuleError(errorMessage, occuredError.Message);

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully minimized the data set.");



                ImportWorkerLog("Inserting the minimized data set into the database.");

                string dbTableNamePower = electricityConfiguration.dbTableNamePower;
                string dbTableNamePowerfactor = electricityConfiguration.dbTableNamePowerfactor;
                string sqlConnectionString = electricityConfiguration.sqlConnectionString;

                occuredError = await InsertDataIntoDatabase(sqlConnectionString, dbTableNamePower, dbTableNamePowerfactor, minimizedSourceData, cancellationToken);

                if (occuredError != null)
                {
                    string errorMessage = "An error has occured while inserting the data into the database.";
                    ThrowModuleError(errorMessage, occuredError.Message);

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully inserted the data set into the database.");



                _dateOfLastImport = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");



                if (foundMultipleFiles == true)
                {
                    ImportWorkerLog($"Restarting import process immediately, as there were multiple source files.");
                    goto LabelRestartAsMultipleFiles;
                }



                ImportWorkerLog($"Going to sleep for {apiSleepTimer / 1000} seconds.");

                await Task.Delay(apiSleepTimer, cancellationToken);
            }
        }

        private static async Task<(ElectricityConfiguration electricityConfiguration, Exception? occuredError)> GetConfigurationValues()
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
                return (new ElectricityConfiguration(), exception);
            }



            JObject modules;
            JObject electricityModule;
            JObject sqlData;

            try
            {
                modules = savedConfiguration["modules"] as JObject ?? [];

                if (modules == null || modules == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'modules' object.");
                }

                electricityModule = modules?["electricity"] as JObject ?? [];

                if (electricityModule == null || electricityModule == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'electricity' module.");
                }

                sqlData = savedConfiguration["sql"] as JObject ?? [];

                if (sqlData == null || sqlData == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'sql' object.");
                }
            }
            catch (Exception exception)
            {
                return (new ElectricityConfiguration(), exception);
            }



            try
            {
                ElectricityConfiguration electricityConfiguration = new()
                {
                    sourceFilePath = electricityModule?["sourceFilePath"]?.ToString() ?? string.Empty,
                    sourceFilePattern = electricityModule?["sourceFilePattern"]?.ToString() ?? string.Empty,
                    sourceFileInterval = electricityModule?["sourceFileInterval"]?.ToString() ?? string.Empty,
                    sqlConnectionString = sqlData?["connectionString"]?.ToString() ?? string.Empty,
                    dbTableNamePower = electricityModule?["dbTableNamePower"]?.ToString() ?? string.Empty,
                    dbTableNamePowerfactor = electricityModule?["dbTableNamePowerfactor"]?.ToString() ?? string.Empty
                };

                if (electricityConfiguration.HoldsInvalidValues() == true)
                {
                    throw new Exception("One or mulitple configuration values are null. Please check the configuration file!");
                }

                if (int.TryParse(electricityConfiguration.sourceFileInterval, out int _) == false)
                {
                    throw new Exception("Failed to parse the provided source file interval to a number.");
                }

                return (electricityConfiguration, null);
            }
            catch (Exception exception)
            {
                return (new ElectricityConfiguration(), exception);
            }
        }
        
        private static async Task<(List<string> sourceData, bool foundMultipleFiles, Exception? occuredError)> GetSourceFileData(string sourceFilePath, string sourceFilePattern)
        {
            bool multipleSourceFilesFound = false;

            if (Directory.Exists(sourceFilePath) == false)
            {
                return ([], multipleSourceFilesFound, new Exception("Failed to find the source file folder path specified in the configuration."));
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
                return ([], multipleSourceFilesFound, new Exception("Failed to find any source files which match the configuration."));
            }

            if (fileMatches.Count > 1)
            {
                multipleSourceFilesFound = true;
            }



            string[] sourceFileData;

            try
            {
                sourceFileData = await File.ReadAllLinesAsync(fileMatches[0]);
            }
            catch (Exception exception)
            {
                return ([], multipleSourceFilesFound, exception);
            }



            List<string> finalSourceFileData = [];

            for (int i = 1; i < sourceFileData.Length; i++)
            {
                if (i == sourceFileData.Length - 1)
                {
                    continue;
                }

                string currentRow = sourceFileData[i];
                currentRow = RegexPatterns.AllWhitespaces().Replace(currentRow, string.Empty);

                finalSourceFileData.Add(currentRow);
            }



            return (finalSourceFileData, multipleSourceFilesFound, null);
        }
        
        private static (List<string> minimizedSourceData, Exception? occuredError) MinimizeSourceFileData(List<string> sourceData)
        {
            int valuesPerRow = 0;
            int valuesPerColumn = 0;

            try
            {
                valuesPerRow = sourceData.Select(line => line.Split(';')[1]).Distinct().Count();
                valuesPerColumn = sourceData[0].Split(';').Length - 1;
            }
            catch (Exception exception)
            {
                return ([], new Exception("Unable to calculate count per row or column. " + exception.Message));
            }

            string[,] newDataArray = new string[valuesPerRow, valuesPerColumn];



            int currentSecondIndex = 0;
            string currentSecond = string.Empty;
            
            string[,] allCurrentSecondValues = new string[0, 0];
            int allCurrentSecondValuesCurrentIndexY = 0;


            
            while (sourceData.Count > 0)
            {
                string currentRow = sourceData[0];
                currentRow = RegexPatterns.AllWhitespaces().Replace(currentRow, string.Empty);
                currentRow = currentRow[..^1];

                List<string> splittedRowData = currentRow.Split(';').ToList();

                string currentRowDate = splittedRowData[0];
                string currentRowTime = splittedRowData[1];
                splittedRowData.RemoveRange(0, 2);



            LabelStartOver:



                if (currentSecond.Equals(string.Empty))
                {
                    currentSecond = currentRowTime;

                    int allCurrentSecondValuesHeight = sourceData.Select(row => row.Split(';')).Where(splittedRow => splittedRow[1].Equals(currentSecond)).ToList().Count;
                    int allCurrentSecondValuesWidth = splittedRowData.Count + 2;

                    allCurrentSecondValues = new string[allCurrentSecondValuesHeight, allCurrentSecondValuesWidth];
                }



                if (currentSecond.Equals(currentRowTime) == false && currentSecond.Equals(string.Empty) == false)
                {
                    for (int x = 2; x < allCurrentSecondValues.GetLength(1); x++)
                    {
                        decimal sum = 0;

                        for (int y = 0; y < allCurrentSecondValues.GetLength(0); y++)
                        {
                            sum += Convert.ToDecimal(allCurrentSecondValues[y, x].Replace(".", ","));
                        }

                        decimal average = sum / allCurrentSecondValues.GetLength(0);

                        newDataArray[currentSecondIndex, 0] = allCurrentSecondValues[0, 0];
                        newDataArray[currentSecondIndex, 1] = allCurrentSecondValues[0, 1];
                        newDataArray[currentSecondIndex, x] = Math.Round(average, 2).ToString();
                    }



                    currentSecond = string.Empty;
                    allCurrentSecondValuesCurrentIndexY = 0;
                    currentSecondIndex++;

                    goto LabelStartOver;

                }



                allCurrentSecondValues[allCurrentSecondValuesCurrentIndexY, 0] = currentRowDate;
                allCurrentSecondValues[allCurrentSecondValuesCurrentIndexY, 1] = currentRowTime;



                for (int i = 0; i < splittedRowData.Count; i++)
                {
                    allCurrentSecondValues[allCurrentSecondValuesCurrentIndexY, i + 2] = splittedRowData[i];
                }

                allCurrentSecondValuesCurrentIndexY++;

                sourceData.RemoveAt(0);



                if (sourceData.Count == 0)
                {
                    for (int x = 2; x < allCurrentSecondValues.GetLength(1); x++)
                    {
                        decimal sum = 0;

                        for (int y = 0; y < allCurrentSecondValues.GetLength(0); y++)
                        {
                            sum += Convert.ToDecimal(allCurrentSecondValues[y, x].Replace(".", ","));
                        }

                        decimal average = sum / allCurrentSecondValues.GetLength(0);

                        newDataArray[currentSecondIndex, 0] = allCurrentSecondValues[0, 0];
                        newDataArray[currentSecondIndex, 1] = allCurrentSecondValues[0, 1];
                        newDataArray[currentSecondIndex, x] = Math.Round(average, 2).ToString();
                    }



                    currentSecond = string.Empty;
                    allCurrentSecondValuesCurrentIndexY = 0;
                    currentSecondIndex++;
                }
            }


            List<string> minimizedSourceData = [];

            try
            {
                for (int y = 0; y < newDataArray.GetLength(0); y++)
                {
                    string currentRow = string.Empty;

                    for (int x = 0; x < newDataArray.GetLength(1); x++)
                    {
                        currentRow += newDataArray[y, x] + ";";
                    }

                    currentRow = currentRow[..^1];

                    minimizedSourceData.Add(currentRow);
                }
            }
            catch (Exception exception)
            {
                return ([], new Exception("Unable to fill minimized source data. " + exception.Message));
            }
            
            return (minimizedSourceData, null);
        }

        private static async Task<Exception?> InsertDataIntoDatabase(string sqlConnectionString, string dbTableNamePower, string dbTableNamePowerfactor, List<string> sourceData, CancellationToken cancellationToken)
        {
            (List<string> powerData, List<string> powerfactorData, Exception? occuredError) = SplitSourceData(sourceData);
            
            if (occuredError != null)
            {
                return new Exception("Failed to split the minimalized data set. " + occuredError.Message);
            }

            ImportWorkerLog("Splitted the minimalized data set for the database import.");



            SqlConnection databaseConnection = new(sqlConnectionString);

            try
            {
                await databaseConnection.OpenAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                return new Exception("Failed to establish a database connection. " + exception.Message);
            }

            ImportWorkerLog("Successfully established a database connection.");



            ImportWorkerLog($"Inserting a total of '{sourceData.Count}' entries into the database.");
            
            while (powerData.Count > 0)
            {
                string currentPowerDataRow = powerData[0];
                string currentPowerfactorDataRow = powerfactorData[0];

                DateTime importDate;
                TimeSpan importTime;

                try
                {
                    string powerDate = currentPowerDataRow.Split(';')[0];
                    string powerTime = currentPowerDataRow.Split(';')[1];

                    importDate = DateTime.ParseExact(powerDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    importTime = TimeSpan.ParseExact(powerTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture);

                    powerData.RemoveAt(0);
                    powerfactorData.RemoveAt(0);

                    currentPowerDataRow = currentPowerDataRow.Replace($"{powerDate};{powerTime};", string.Empty);
                    currentPowerfactorDataRow = currentPowerfactorDataRow.Replace($"{powerDate};{powerTime};", string.Empty);
                }
                catch (Exception exception)
                {
                    return new Exception("Failed to convert date and/or time. " + exception.Message);
                }



                string[] columnsOrder =
                [
                    "@einspeisung_L1",
                    "@einspeisung_L2",
                    "@einspeisung_L3",
                    "@werkstatterweiterung_L1",
                    "@werkstatterweiterung_L2",
                    "@werkstatterweiterung_L3",
                    "@flur_zimmerei_L1",
                    "@flur_zimmerei_L2",
                    "@flur_zimmerei_L3",
                    "@flur_tischlerei_L1",
                    "@flur_tischlerei_L2",
                    "@flur_tischlerei_L3",
                    "@absaugung_tischlerei_L1",
                    "@absaugung_tischlerei_L2",
                    "@absaugung_tischlerei_L3",
                    "@theorie_L1",
                    "@theorie_L2",
                    "@theorie_L3",
                    "@tischlerei_L1",
                    "@tischlerei_L2",
                    "@tischlerei_L3",
                    "@keller_L1",
                    "@keller_L2",
                    "@keller_L3",
                    "@absaugung_steinmetz_L1",
                    "@absaugung_steinmetz_L2",
                    "@absaugung_steinmetz_L3"
                ];



                using SqlTransaction transaction = databaseConnection.BeginTransaction();

                try
                {
                    string queryNamesPower = "power_date, power_time, " + string.Join(", ", columnsOrder).Replace("@", string.Empty);
                    string queryValuesPower = "@power_date, @power_time, " + string.Join(", ", columnsOrder);
                    string queryInsertPower = $"INSERT INTO {dbTableNamePower} ({queryNamesPower}) VALUES ({queryValuesPower}); SELECT SCOPE_IDENTITY();";
                    
                    using SqlCommand insertCommandPower = new(queryInsertPower, databaseConnection, transaction);

                    insertCommandPower.Parameters.AddWithValue("@power_date", importDate);
                    insertCommandPower.Parameters.AddWithValue("@power_time", importTime);

                    for (int columnIndex = 0; columnIndex < columnsOrder.Length; columnIndex++)
                    {
                        insertCommandPower.Parameters.AddWithValue(columnsOrder[columnIndex], Convert.ToDecimal(currentPowerDataRow.Split(";")[columnIndex]));
                    }

                    int lastPowerId = -1;

                    lastPowerId = Convert.ToInt32(await insertCommandPower.ExecuteScalarAsync(cancellationToken));

                    if (lastPowerId == -1)
                    {
                        throw new Exception("Failed to get last inserted PowerId.");
                    }



                    string queryNamesPowerfactor = "powerfactor_date, powerfactor_time, power_id, " + string.Join(", ", columnsOrder).Replace("@", string.Empty);
                    string queryValuesPowerfactor = "@powerfactor_date, @powerfactor_time, @power_id, " + string.Join(", ", columnsOrder);
                    string queryInsertPowerfactor = $"INSERT INTO {dbTableNamePowerfactor} ({queryNamesPowerfactor}) VALUES ({queryValuesPowerfactor}); SELECT SCOPE_IDENTITY();";

                    using SqlCommand insertCommandPowerfactor = new(queryInsertPowerfactor, databaseConnection, transaction);

                    insertCommandPowerfactor.Parameters.AddWithValue("@powerfactor_date", importDate);
                    insertCommandPowerfactor.Parameters.AddWithValue("@powerfactor_time", importTime);
                    insertCommandPowerfactor.Parameters.AddWithValue("@power_id", lastPowerId);

                    for (int columnIndex = 0; columnIndex < columnsOrder.Length; columnIndex++)
                    {
                        insertCommandPowerfactor.Parameters.AddWithValue(columnsOrder[columnIndex], Convert.ToDecimal(currentPowerfactorDataRow.Split(";")[columnIndex]));
                    }

                    await insertCommandPowerfactor.ExecuteNonQueryAsync(cancellationToken);

                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    transaction.Rollback();

                    return new Exception($"Failed to import a row of the data set. Remaining elements: '{powerData.Count}'. " + exception.Message);
                }
            }



            return null;
        }

        private static (List<string> powerData, List<string> powerfactorData, Exception? occuredError) SplitSourceData(List<string> sourceData)
        {
            List<string> powerData = [];
            List<string> powerfactorData = [];

            foreach (string dataRow in sourceData)
            {
                string[] splittedRow = dataRow.Split(';');

                string newPowerDataRow = string.Empty;
                string newPowerfactorDataRow = string.Empty;

                for (int i = 0; i < splittedRow.Length; i++)
                {
                    if (i == 0 || i == 1)
                    {
                        newPowerDataRow += splittedRow[i] + ";";
                        newPowerfactorDataRow += splittedRow[i] + ";";
                        continue;
                    }

                    if (i % 2 == 0)
                    {
                        newPowerfactorDataRow += splittedRow[i] + ";";
                        continue;
                    }

                    newPowerDataRow += splittedRow[i] + ";";
                }

                newPowerDataRow = newPowerDataRow[..^1];
                newPowerfactorDataRow = newPowerfactorDataRow[..^1];

                powerData.Add(newPowerDataRow);
                powerfactorData.Add(newPowerfactorDataRow);
            }

            return (powerData, powerfactorData, null);
        }

        private static void ImportWorkerLog(string message, bool removePrefix = false)
        {
            ImportLogger.Log(_currentSection, message, removePrefix);
            _dateOfLastLogFileEntry = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
        }

        private void ThrowModuleError(string errorMessage, string detailedError)
        {
            ImportWorkerLog($"[ERROR] - {errorMessage}");
            ImportWorkerLog(detailedError, true);

            MainMenu._sectionMiscellaneous.errorCache.AddEntry(_currentSection, errorMessage, detailedError);

            State = ModuleState.Error;
            _errorCount++;
        }
    }
}