using System.Globalization;

using DataImportClient.Scripts;

using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;





namespace DataImportClient.Modules
{
    internal class Weather
    {
        private const string _currentSection = "ModuleWeather";

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

        private static Task _importWorker = new(() => {});
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

        internal event EventHandler StateChanged;

        protected virtual void OnStateChange()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }



        internal int ErrorCount
        {
            get => _errorCount;
        }



        internal Weather()
        {
            _moduleState = ModuleState.Running;
            _errorCount = 0;
            _serviceRunning = true;

            _dateOfLastImport = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");
            _dateOfLastLogFileEntry = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");



            _cancellationTokenSource = new();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            _importWorker = Task.Run(() => ImportApiData(cancellationToken));
        }



        internal async Task Main()
        {
            ActivityLogger.Log(_currentSection, "Entering module 'Weather'.");

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

                case ConsoleKey.Backspace:
                    return;

                case ConsoleKey.Escape:
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
                    return;
            }



            Console.Clear();
            goto LabelDrawUi;
        }

        private static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("              ┓ ┏     ┓                                        ");
            Console.WriteLine("              ┃┃┃┏┓┏┓╋┣┓┏┓┏┓                                   ");
            Console.WriteLine("              ┗┻┛┗ ┗┻┗┛┗┗ ┛                                    ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ──────────────────                                ");
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

        private async Task ImportApiData(CancellationToken cancellationToken)
        {
            ImportLogger.Log(_currentSection, "Starting import worker for the current module.");
            int errorTimoutInMilliseconds = 5 * 30 * 1000;



            while (true)
            {
                ImportLogger.Log(_currentSection, "Fetching settings from configuration file.");

                (string[] apiConfiguration, string sqlConnectionString, Exception? occuredError) = await GetConfigurationValues();

                if (occuredError != null)
                {
                    ImportLogger.Log(_currentSection, "[ERROR] - An error has occured while fetching the settings");
                    ImportLogger.Log(_currentSection, occuredError.Message, true);

                    State = ModuleState.Error;
                    _errorCount++;

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportLogger.Log(_currentSection, "Successfully fetched settings.");



                string apiUrl = apiConfiguration[0];
                string apiKey = apiConfiguration[1];
                string apiCity = apiConfiguration[2];
                string apiIntervalSeconds = apiConfiguration[3];

                apiUrl += $"?q={apiCity}&appid={apiKey}&mode=json&units=metric";
                int apiSleepTimer = Convert.ToInt32(apiIntervalSeconds) * 1000;



                ImportLogger.Log(_currentSection, "Contacting the API and requesting a data set.");

                (string dataWindSpeed, string dataTemperature, occuredError) = await FetchApiData(apiUrl, cancellationToken);

                if (occuredError != null)
                {
                    ImportLogger.Log(_currentSection, "[ERROR] - An error has occured while inserting the data into the database.");
                    ImportLogger.Log(_currentSection, occuredError.Message, true);

                    State = ModuleState.Error;
                    _errorCount++;

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportLogger.Log(_currentSection, "Successfully fetched the data set from the API.");



                ImportLogger.Log(_currentSection, "Inserting the fetched data set into the database.");

                occuredError = await InsertDataIntoDatabase(sqlConnectionString, dataWindSpeed, dataTemperature, cancellationToken);

                if (occuredError != null)
                {
                    ImportLogger.Log(_currentSection, "[ERROR] - An error has occured while inserting the data into the database.");
                    ImportLogger.Log(_currentSection, occuredError.Message, true);

                    State = ModuleState.Error;
                    _errorCount++;

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportLogger.Log(_currentSection, "Successfully inserted the API data into the database.");



                ImportLogger.Log(_currentSection, $"Going to sleep for {apiSleepTimer / 1000} seconds.");
                await Task.Delay(apiSleepTimer, cancellationToken);
            }
        }

        private static async Task<(string[] apiConfiguration, string sqlConnectionString, Exception? occuredError)> GetConfigurationValues()
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
                return (Array.Empty<string>(), string.Empty, exception);
            }



            JObject modules;
            JObject weatherModule;
            JObject sqlData;

            try
            {
                modules = savedConfiguration["modules"] as JObject ?? [];

                if (modules == null || modules == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'modules' object.");
                }

                weatherModule = modules?["weather"] as JObject ?? [];

                if (weatherModule == null || weatherModule == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'weather' module.");
                }

                sqlData = savedConfiguration["sql"] as JObject ?? [];

                if (sqlData == null || sqlData == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'sql' object.");
                }
            }
            catch (Exception exception)
            {
                return (Array.Empty<string>(), string.Empty, exception);
            }



            string apiUrl;
            string apiKey;
            string apiCity;
            string apiIntervalSeconds;
            string sqlConnectionString;

            try
            {
                apiUrl = weatherModule?["apiUrl"]?.ToString() ?? string.Empty;
                apiKey = weatherModule?["apiKey"]?.ToString() ?? string.Empty;
                apiCity = weatherModule?["apiCity"]?.ToString() ?? string.Empty;
                apiIntervalSeconds = weatherModule?["apiIntervalSeconds"]?.ToString() ?? string.Empty;
                sqlConnectionString = sqlData?["connectionString"]?.ToString() ?? string.Empty;

                if (new string[] { apiUrl, apiKey, apiIntervalSeconds, sqlConnectionString }.Contains(null))
                {
                    throw new Exception("One or mulitple API or SQL values are null!");
                }

                if (new string[] { apiUrl, apiKey, apiIntervalSeconds, sqlConnectionString }.Contains(string.Empty))
                {
                    throw new Exception("One or mulitple API or SQL values are an empty string!");
                }
            }
            catch (Exception exception)
            {
                return (Array.Empty<string>(), string.Empty, exception);
            }



            if (int.TryParse(apiIntervalSeconds, out int _) == false)
            {
                return (Array.Empty<string>(), string.Empty, new Exception("Failed to parse the provided api interval to a number."));
            }



            string[] apiConfiguration =
            {
                apiUrl,
                apiKey,
                apiCity,
                apiIntervalSeconds,
            };

            return (apiConfiguration, sqlConnectionString, null);
        }

        private static async Task<(string dataWindSpeed, string dataTemperature, Exception? occuredError)> FetchApiData(string apiUrl, CancellationToken cancellationToken)
        {
            JObject apiData = [];

            try
            {
                using HttpClient client = new();

                string apiJsonData = await client.GetStringAsync(apiUrl, cancellationToken);

                apiData = JObject.Parse(apiJsonData);
            }
            catch (Exception exception)
            {
                return (string.Empty, string.Empty, exception);
            }



            if (apiData == null || apiData == new JObject())
            {
                return (string.Empty, string.Empty, new Exception("The fetched data provided by the API is 'null'."));
            }

            ImportLogger.Log(_currentSection, "Successfully received the requested data from the API.");



            string dataWindSpeed = apiData?["wind"]?["speed"]?.ToString() ?? string.Empty;
            string dataTemperature = apiData?["main"]?["temp"]?.ToString() ?? string.Empty;

            bool validApiValues = ConsoleHelper.ValidDecimalValues([dataWindSpeed, dataTemperature]);

            if (string.IsNullOrWhiteSpace(dataWindSpeed) || string.IsNullOrWhiteSpace(dataTemperature) || validApiValues == false)
            {
                return (string.Empty, string.Empty, new Exception($"The fetched data contains invalid values. (Wind speed: '{dataWindSpeed}' | Temperature: '{dataTemperature}')"));
            }

            return (dataWindSpeed, dataTemperature, null);
        }

        private static async Task<Exception?> InsertDataIntoDatabase(string sqlConnectionString, string dataWindSpeed, string dataTemperature, CancellationToken cancellationToken)
        {
            SqlConnection databaseConnection = new(sqlConnectionString);

            try
            {
                await databaseConnection.OpenAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                return exception;
            }

            ImportLogger.Log(_currentSection, "Successfully established a database connection.");



            try
            {
                string insertDataQuery = "INSERT INTO dbo.weather (windSpeed, temperature) VALUES (@windSpeed, @temperature);";
                
                using SqlCommand insertCommand = new(insertDataQuery, databaseConnection);

                insertCommand.Parameters.AddWithValue("@windSpeed", decimal.Parse(dataWindSpeed));
                insertCommand.Parameters.AddWithValue("@temperature", decimal.Parse(dataTemperature));

                await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                return exception;
            }

            return null;
        }
    }
}