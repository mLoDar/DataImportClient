using System.Diagnostics;
using System.Globalization;

using DataImportClient.Scripts;
using DataImportClient.Ressources;
using static DataImportClient.Ressources.ModuleConfigurations;

using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;





namespace DataImportClient.Modules
{
    internal class Photovoltaic
    {
        private const string _currentSection = "ModulePhotovoltaic";

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

        private static readonly ApplicationSettings.Urls _appUrls = new();
        private static readonly ApplicationSettings.Paths _appPaths = new();
        


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        private static IPlaywright _playwrightInstance;
        private static IBrowser _playwrightBrowser;
        private static IBrowserContext _playwrightContext;
        private static IPage _playwrightPage;

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        private static long _unixMillisNextSessionCreation = 0;



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



        internal Photovoltaic()
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
            ActivityLogger.Log(_currentSection, "Entering module 'Photovoltaic'.");



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
                        string importWorkerLogsFolder = _appPaths.photovoltaicImportWorkerLogs;
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

                    _importWorker = Task.Run(() => ImportApiData(cancellationToken));

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
            Console.WriteLine("              ┏┓┓          ┓   •                               ");
            Console.WriteLine("              ┃┃┣┓┏┓╋┏┓┓┏┏┓┃╋┏┓┓┏                              ");
            Console.WriteLine("              ┣┛┛┗┗┛┗┗┛┗┛┗┛┗┗┗┻┗┗                              ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ───────────────────────                           ");
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
            ImportWorkerLog(string.Empty, true);
            ImportWorkerLog("Starting a new import worker for the current module.");

            int errorTimoutInMilliseconds = 5 * 30 * 1000;



            while (true)
            {
                ImportWorkerLog("Fetching settings from configuration file.");

                (PhotovoltaicConfiguration photovoltaicConfiguration, Exception? occurredError) = await GetConfigurationValues();

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



                if (int.TryParse(photovoltaicConfiguration.apiIntervalSeconds, out int apiSleepTimer) == false)
                {
                    string errorMessage = "An error has occurred while assigning variables.";
                    string[] errorDetails = ["Failed to parse 'apiIntervalSeconds' to int."];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.IntegerParsing);

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }



                ImportWorkerLog("Contacting the API and requesting a data set.");

                (decimal currentPvPower, occurredError) = await FetchApiData(photovoltaicConfiguration, cancellationToken);
                
                if (occurredError != null)
                {
                    string errorMessage = "An error has occurred while fetching data from the API provider.";
                    string[] errorDetails = [occurredError.Message, occurredError.InnerException?.ToString() ?? string.Empty];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.ApiDataFetching);

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully fetched the data set from the API.");



                ImportWorkerLog("Inserting the fetched data set into the database.");

                occurredError = await InsertDataIntoDatabase(photovoltaicConfiguration.sqlConnectionString, photovoltaicConfiguration.dbTableName, currentPvPower, cancellationToken);

                if (occurredError != null)
                {
                    string errorMessage = "An error has occurred while inserting the data into the database.";
                    string[] errorDetails = [occurredError.Message, occurredError.InnerException?.ToString() ?? string.Empty];
                    ThrowModuleError(errorMessage, errorDetails, ErrorCategory.DatabaseInsertion);

                    ImportWorkerLog($"Waiting for {errorTimoutInMilliseconds} seconds before continuing with the import process.");

                    await Task.Delay(errorTimoutInMilliseconds, cancellationToken);
                    continue;
                }

                ImportWorkerLog("Successfully inserted the API data into the database.");



                _dateOfLastImport = DateTime.Now.ToString("dd.MM.yyyy - HH:mm:ss");



                ImportWorkerLog($"Going to sleep for {apiSleepTimer} seconds.");
                await Task.Delay(apiSleepTimer * 1000, cancellationToken);
            }
        }

        private static async Task<(PhotovoltaicConfiguration photovoltaicConfiguration, Exception? occurredError)> GetConfigurationValues()
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
                return (new PhotovoltaicConfiguration(), exception);
            }



            JObject modules;
            JObject photovoltaicModule;
            JObject sqlData;

            try
            {
                modules = savedConfiguration["modules"] as JObject ?? [];

                if (modules == null || modules == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'modules' object.");
                }

                photovoltaicModule = modules?["photovoltaic"] as JObject ?? [];

                if (photovoltaicModule == null || photovoltaicModule == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'photovoltaic' module.");
                }

                sqlData = savedConfiguration["sql"] as JObject ?? [];

                if (sqlData == null || sqlData == new JObject())
                {
                    throw new Exception("Configuration file does not contain a 'sql' object.");
                }
            }
            catch (Exception exception)
            {
                return (new PhotovoltaicConfiguration(), exception);
            }



            try
            {
                PhotovoltaicConfiguration photovoltaicConfiguration = new()
                {
                    solarwebEmail = photovoltaicModule?["solarwebEmail"]?.ToString() ?? string.Empty,
                    solarwebPassword = photovoltaicModule?["solarwebPassword"]?.ToString() ?? string.Empty,
                    solarwebSystemId = photovoltaicModule?["solarwebSystemId"]?.ToString() ?? string.Empty,
                    apiIntervalSeconds = photovoltaicModule?["apiIntervalSeconds"]?.ToString() ?? string.Empty,
                    sqlConnectionString = sqlData?["connectionString"]?.ToString() ?? string.Empty,
                    dbTableName = photovoltaicModule?["dbTableName"]?.ToString() ?? string.Empty
                };

                if (photovoltaicConfiguration.HoldsInvalidValues() == true)
                {
                    throw new Exception("One or mulitple configuration values are null. Please check the configuration file!");
                }

                if (int.TryParse(photovoltaicConfiguration.apiIntervalSeconds, out int _) == false)
                {
                    throw new Exception("Failed to parse the provided API interval to a number.");
                }

                return (photovoltaicConfiguration, null);
            }
            catch (Exception exception)
            {
                return (new PhotovoltaicConfiguration(), exception);
            }
        }
        
        private static async Task<(decimal currentPvPower, Exception? occurredError)> FetchApiData(PhotovoltaicConfiguration photovoltaicConfiguration, CancellationToken cancellationToken)
        {
            long currentUnixMillis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (currentUnixMillis > _unixMillisNextSessionCreation || _playwrightContext == null)
            {
                try
                {
                    await _playwrightBrowser.CloseAsync();
                }
                catch
                {

                }

                bool browserCreated = await CreatePlaywrightInstance(photovoltaicConfiguration);

                if (browserCreated == false)
                {
                    return (0, new Exception("An error has occurred while creating a new playwright instance."));
                }

                long oneDayInMillis = 24 * 60 * 60 * 1000;
                _unixMillisNextSessionCreation = currentUnixMillis + oneDayInMillis;
            }



#pragma warning disable CS8602 // Dereference of a possibly null reference.
            IReadOnlyList<BrowserContextCookiesResult> currentBrowserCookies = await _playwrightContext.CookiesAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            string cookieHeader = string.Join("; ", currentBrowserCookies.Select(cookie => $"{cookie.Name}={cookie.Value}"));

            ImportWorkerLog("Retreived all cookies from the current playwright instance.");



            string requestUrl = _appUrls.photovoltaicApi;
            requestUrl = requestUrl.Replace("{solarwebSystemId}", photovoltaicConfiguration.solarwebSystemId);
            requestUrl = requestUrl.Replace("{currentUnixMillis}", currentUnixMillis.ToString());

            HttpClient httpClient = new();
            HttpRequestMessage apiRequest;

            Dictionary<string, string> apiRequestHeaders = new()
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36" },
                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8" },
                { "Accept-Encoding", "gzip, deflate, br, zstd" },
                { "Accept-Language", "en-US,en;q=0.9" },
                { "DNT", "1" },
                { "Upgrade-Insecure-Requests", "1" },
                { "Sec-Fetch-Dest", "document" },
                { "Sec-Fetch-Mode", "navigate" },
                { "Sec-Fetch-Site", "none" },
                { "Sec-Fetch-User", "?1" },
                { "Sec-CH-UA", "\"Google Chrome\";v=\"135\", \"Not-A.Brand\";v=\"8\", \"Chromium\";v=\"135\"" },
                { "Sec-CH-UA-Mobile", "?0" },
                { "Sec-CH-UA-Platform", "\"Windows\"" },
                { "Cookie", cookieHeader }
            };



            JObject parsedApiResponse = [];
            string apiResponse = string.Empty;

            int maxRequestRetries = 5;

            for (int i = 0; i < maxRequestRetries; i++)
            {
                try
                {
                    apiRequest = new(HttpMethod.Get, requestUrl);

                    foreach (var header in apiRequestHeaders)
                    {
                        apiRequest.Headers.Add(header.Key, header.Value);
                    }

                    HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(apiRequest, cancellationToken);
                    httpResponseMessage.EnsureSuccessStatusCode();

                    apiResponse = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

                    break;
                }
                catch (HttpRequestException httpRequestException) when (i < maxRequestRetries - 1)
                {
                    ImportWorkerLog($"[WARNING] - [Iteration {i}] - Exception of type 'HttpRequestException' was thrown.");

                    if (httpRequestException.InnerException is System.Security.Authentication.AuthenticationException)
                    {
                        ImportWorkerLog("Exception is most likely related to an SSL error.", true);
                        ImportWorkerLog(httpRequestException.Message, true);
                    }
                    else
                    {
                        ImportWorkerLog("Exception is most likely related to some general network related issue.", true);
                        ImportWorkerLog(httpRequestException.Message, true);
                    }

                    int cooldownTimer = 1000 * (int)Math.Pow(2, i);
                    ImportWorkerLog($"Retrying in {cooldownTimer / 1000} seconds.", true);

                    await Task.Delay(cooldownTimer, cancellationToken);
                }
                catch (Exception exception)
                {
                    return (0, exception);
                }
            }



            decimal currentPvPower;

            try
            {
                parsedApiResponse = JObject.Parse(apiResponse);
                currentPvPower = decimal.Parse(parsedApiResponse["P_PV"]?.ToString() ?? string.Empty);
            }
            catch
            {
                return (0, new Exception("Failed to parse the API response or extract the current PV power."));
            }



            return (currentPvPower, null);
        }

        private static async Task<bool> CreatePlaywrightInstance(PhotovoltaicConfiguration photovoltaicConfiguration)
        {
            try
            {
                BrowserTypeLaunchOptions browserTypeLaunchOptions = new()
                {
                    Headless = true
                };

                _playwrightInstance = await Playwright.CreateAsync();
                _playwrightBrowser = await _playwrightInstance.Chromium.LaunchAsync(browserTypeLaunchOptions);
                _playwrightContext = await _playwrightBrowser.NewContextAsync();
                _playwrightPage = await _playwrightContext.NewPageAsync();
            }
            catch (Exception exception)
            {
                ImportWorkerLog("Encountered an unexpected error while launching a new playwright instance.");
                ImportWorkerLog(exception.Message);

                return false;
            }
            
            ImportWorkerLog("Created playwright instances.");



            try
            {
                await _playwrightPage.RouteAsync("**/*", route =>
                {
                    if (route.Request.Url.Contains("consent"))
                    {
                        route.AbortAsync();
                    }
                    else
                    {
                        route.ContinueAsync();
                    }
                }
                );

                ImportWorkerLog("Handled cookie banner.");

                string solarwebLogin = _appUrls.solarwebLogin;
                await _playwrightPage.GotoAsync(solarwebLogin);

                ImportWorkerLog("Went to the login page.");

                await _playwrightPage.WaitForSelectorAsync("input[id='usernameUserInput']", new() { Timeout = 10000, State = WaitForSelectorState.Visible });
                await _playwrightPage.FillAsync("input[id='usernameUserInput']", photovoltaicConfiguration.solarwebEmail);
                await _playwrightPage.FillAsync("input[id='password']", photovoltaicConfiguration.solarwebPassword);
                
                await _playwrightPage.WaitForSelectorAsync("button#login-button", new() { Timeout = 10000, State = WaitForSelectorState.Visible });
                await _playwrightPage.ClickAsync("button#login-button");

                ImportWorkerLog("Submitted login information.");

                await _playwrightPage.WaitForURLAsync("**/PvSystems/PvSystem?pvSystemId=*", new() { Timeout = 10000 });
            }
            catch (Exception exception)
            {
                ImportWorkerLog("Encountered an error while creating a browser instance:");
                ImportWorkerLog(exception.Message);

                return false;
            }

            ImportWorkerLog("Successfully logged in.");

            return true;
        }

        private static async Task<Exception?> InsertDataIntoDatabase(string sqlConnectionString, string dbTableName, decimal currentPvPower, CancellationToken cancellationToken)
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

            ImportWorkerLog("Successfully established a database connection.");



            try
            {
                string queryNames = "pv_leistung_watt";
                string queryValues = "@pv_leistung_watt";
                string insertDataQuery = $"INSERT INTO {dbTableName} ({queryNames}) VALUES ({queryValues});";

                using SqlCommand insertCommand = new(insertDataQuery, databaseConnection);

                insertCommand.Parameters.AddWithValue("@pv_leistung_watt", currentPvPower);

                await insertCommand.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                return exception;
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
    }
}