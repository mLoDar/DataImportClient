using System.Diagnostics;

using DataImportClient.Scripts;
using DataImportClient.Ressources;

using Newtonsoft.Json.Linq;





namespace DataImportClient.Modules
{
    enum ErrorCategory
    {
        ConfigurationFetching,
        IntegerParsing,
        SourceFileDataFetching,
        DatabaseInsertion,
        FileDeletion,
        FileMoving,
        SourceFileDataMinimizing,
        ApiDataFetching,
        Miscellaneous,
    }

    struct ErrorCacheEntry
    {
        internal Guid errorId;
        internal DateTime dateTime;
        internal int processId;
        internal string section;
        internal string errorMessage;
        internal string errorDetail;
        internal ErrorCategory errorCategory;

        internal readonly string ToMinimalistic()
        {
            return $"[{section}] - {errorMessage}";
        }

        internal readonly string ToDetailed()
        {
            return $"[{dateTime:yyyy-MM-dd HH:mm:ss}] - [ProcessId: {processId} | Section: {section}] - {errorMessage} {errorDetail}";
        }
    }



    internal class ErrorCache
    {
        private const string _currentSection = "ErrorCache";

        private static readonly ApplicationSettings.Paths _appPaths = new();

        private const int _maxEntries = 50;
        private readonly object _entriesLock = new();
        private readonly List<ErrorCacheEntry> _errorCache = [];
        private readonly static List<ErrorCacheEntry> _errorAlerted = [];

        private const int _emailAlertsCooldownSeconds = 300;
        private static long _lastEmailAlertUnixSeconds = 0;



        internal void AddEntry(string errorSection, string errorMessage, string errorDetail, ErrorCategory errorCategory)
        {
            ErrorCacheEntry newEntry = new()
            {
                errorId = Guid.NewGuid(),
                dateTime = DateTime.Now,
                processId = Environment.ProcessId,
                section = errorSection,
                errorMessage = errorMessage,
                errorDetail = errorDetail,
                errorCategory = errorCategory
            };



            lock (_entriesLock)
            {
                if (_errorCache.Count + 1 > _maxEntries)
                {
                    _errorCache.RemoveAt(0);
                }

                _errorCache.Add(newEntry);

                ActivityLogger.Log(_currentSection, "Received a new error for the error cache. Checking if the application needs to send an email alert.");
                ActivityLogger.Log(_currentSection, $"ErrorCategory: {newEntry.errorCategory} | ErrorModule: {newEntry.section}", true);

                bool sendAlertEmail = ShouldSendEmailAlert(newEntry);

                if (sendAlertEmail == false)
                {
                    ActivityLogger.Log(_currentSection, "Suppressed sending of a new email alert.");
                    return;
                }

                ActivityLogger.Log(_currentSection, "Sending a new email alert for the current error.");



                string emailSubject = "Fehler beim Daten-Import";
                string emailBody = $"Ein neuer Fehler des Typs '{newEntry.errorCategory}' ist im Bereich '{newEntry.section}' aufgetreten." +
                             $"\r\nDerzeit {(_errorCache.Count > 1 ? "sind" : "ist")} {_errorCache.Count} Fehler im Fehlerspeicher des DataImportClient." +
                         $"\r\n\r\nGenauere Details werden in dem Modul-Spezifischem Logfile aufgelistet." + 
                             $"\r\nBenachrichtigungen dieser Art können im DataImportClient unter dem Menüpunkt 'Miscellaneous' deaktiviert werden.";

                Task.Run(async () =>
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
                        ActivityLogger.Log(_currentSection, "[ERROR] - Failed to check if application should send an email!");
                        ActivityLogger.Log(_currentSection, $"Exception: {exception.Message}", true);

                        return;
                    }



                    JToken? emailAlertsActive = savedConfiguration?["emailAlerts"]?["featureActive"];

                    if (emailAlertsActive == null)
                    {
                        ActivityLogger.Log(_currentSection, "[ERROR] - Failed to check if application should send an email!");
                        ActivityLogger.Log(_currentSection, $"Exception: Variable 'emailAlerts.featureActive' within the configuration file is null.", true);
                        return;
                    }

                    if (emailAlertsActive.ToString().ToLower().Equals("false"))
                    {
                        return;
                    }



                    bool emailSuccess = await EmailClient.SendEmail(errorSection, emailSubject, emailBody);

                    if (emailSuccess == false)
                    {
                        ActivityLogger.Log(_currentSection, "[ERROR] - Failed to send an email!");
                        ActivityLogger.Log(_currentSection, "Please check the error logs above to fix this error.", true);
                    }
                }
                );

                _errorAlerted.Add(newEntry);
            }
        }

        internal void RemoveSectionFromCache(string errorSection)
        {
            List<ErrorCacheEntry> entriesForRemoval = [];

            foreach (ErrorCacheEntry entry in _errorCache)
            {
                if (entry.section.Equals(errorSection))
                {
                    entriesForRemoval.Add(entry);
                }
            }

            foreach (ErrorCacheEntry entry in entriesForRemoval)
            {
                _errorCache.Remove(entry);
            }

            foreach (ErrorCacheEntry entry in entriesForRemoval)
            {
                _errorAlerted.Remove(entry);
            }

            ActivityLogger.Log(_currentSection, $"Removed all error for the section '{errorSection}' from the error cache!");
        }

        internal void DisplayMinimalistic()
        {
            int cacheViewStartIndex = 0;

            if (_errorCache.Count <= 0)
            {
                ActivityLogger.Log(_currentSection, "The error cache currently does not hold any entries.");

                DisplayZeroErrorSituation();

                ActivityLogger.Log(_currentSection, "Returning to miscellaneous selection.");
                return;
            }



            ActivityLogger.Log(_currentSection, "Displaying a minimal error cache, waiting for return signal.");
            ActivityLogger.Log(_currentSection, $"The error cache currently holds {(_errorCache.Count > 1 ? "1 entry" : $"{_errorCache.Count} entries")}.");



            Console.Clear();

        LabelDrawErrorCache:

            Console.SetCursorPosition(0, 4);



            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("              ┏┓          ┏┓   ┓   ");
            Console.WriteLine("              ┣ ┏┓┏┓┏┓┏┓  ┃ ┏┓┏┣┓┏┓");
            Console.WriteLine("              ┗┛┛ ┛ ┗┛┛   ┗┛┗┻┗┛┗┗ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ─────────────────────────                        ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌──────────────────────────────────────────────────────────────────────────────────────────┐   ┬");



            int maxErrorLength = 88;
            int scrollBarHelper = 0;
            int errorsDisplayedAtOnce = 10;

            var (scrollBarStart, scrollBarEnd) = GetScrollbarRange(_errorCache.Count, errorsDisplayedAtOnce, cacheViewStartIndex);

            for (int currentErrorIndex = cacheViewStartIndex; currentErrorIndex < cacheViewStartIndex + errorsDisplayedAtOnce && currentErrorIndex < _errorCache.Count; currentErrorIndex++)
            {
                string currentError = _errorCache[currentErrorIndex].ToMinimalistic();

                if (currentError.Length > maxErrorLength)
                {
                    currentError = currentError[..(maxErrorLength - 4)] + " ...";
                }

                currentError = currentError.PadRight(maxErrorLength);



                bool helperWithinRange = scrollBarHelper >= scrollBarStart && scrollBarHelper <= scrollBarEnd;

                Console.WriteLine($"             │ {currentError} │   {(helperWithinRange == true ? "█" : "│")}");

                scrollBarHelper++;
            }



            Console.WriteLine("             └──────────────────────────────────────────────────────────────────────────────────────────┘   ┴");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Navigate with the arrow keys                    ");
            Console.WriteLine("             └────────────────────────────────                 ");
            Console.WriteLine("             ┌ Use 'Backspace' to return                       ");
            Console.WriteLine("             └─────────────────────────────                    ");



            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.DownArrow:
                    if (cacheViewStartIndex + 1 <= _errorCache.Count - errorsDisplayedAtOnce)
                    {
                        cacheViewStartIndex += 1;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (cacheViewStartIndex - 1 >= 0)
                    {
                        cacheViewStartIndex -= 1;
                    }
                    break;

                case ConsoleKey.Escape:
                    ActivityLogger.Log(_currentSection, "Returning to miscellaneous selection via 'ESC'.");
                    return;

                case ConsoleKey.Backspace:
                    ActivityLogger.Log(_currentSection, "Returning to miscellaneous selection via 'BACKSPACE'.");
                    return;

                default:
                    break;
            }



            goto LabelDrawErrorCache;
        }

        internal async Task DisplayDetailed()
        {
            if (_errorCache.Count <= 0)
            {
                ActivityLogger.Log(_currentSection, "The error cache currently does not hold any entries.");

                string title = "Information";
                string description = "There are currently no errors in the cache, yay :)";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Green);

                ActivityLogger.Log(_currentSection, "Returning to miscellaneous selection.");

                return;
            }



            List<string> errorCacheEntries = [];

            DateTime currentTime = DateTime.UtcNow;
            long unixTimeSeconds = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            string exportFileName = Path.Combine(_appPaths.clientFolder, $"errorCacheExport-{unixTimeSeconds}.txt");


            foreach (ErrorCacheEntry entry in _errorCache)
            {
                errorCacheEntries.Add(entry.ToDetailed());
            }



            try
            {
                await File.WriteAllLinesAsync(exportFileName, [.. errorCacheEntries]);

                ActivityLogger.Log(_currentSection, "Successfully created a detailed error cache export.");

                Process.Start("notepad.exe", exportFileName);

                ActivityLogger.Log(_currentSection, "The current error cache export was opened successfully.");
            }
            catch (Exception exception)
            {
                ActivityLogger.Log(_currentSection, "[ERROR] - Failed to create or open the error cache export.");
                ActivityLogger.Log(_currentSection, exception.ToString(), true);

                string title = "Failed to perform this action.";
                string description = "Please check the error log for detailed information.";

                await ConsoleHelper.DisplayInformation(title, description, ConsoleColor.Red);
            }

            ActivityLogger.Log(_currentSection, "Returning to miscellaneous selection.");
        }

        private static void DisplayZeroErrorSituation()
        {
        LabelDrawInformation:

            Console.Clear();
            Console.SetCursorPosition(0, 4);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("              ┏┓          ┏┓   ┓   ");
            Console.WriteLine("              ┣ ┏┓┏┓┏┓┏┓  ┃ ┏┓┏┣┓┏┓");
            Console.WriteLine("              ┗┛┛ ┛ ┗┛┛   ┗┛┗┻┗┛┗┗ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("             ─────────────────────────                        ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌─────────────────────────────────────────────────────────────┐");
            Console.WriteLine("             │                                                             │");
            Console.WriteLine("             │      \u001b[92mThere are currently no errors in the cache, yay :)\u001b[97m     │");
            Console.WriteLine("             │                                                             │");
            Console.WriteLine("             └─────────────────────────────────────────────────────────────┘");
            Console.WriteLine("                                                               ");
            Console.WriteLine("                                                               ");
            Console.WriteLine("             ┌ Use 'Backspace' to return                       ");
            Console.WriteLine("             └────────────────────────────                     ");



            ConsoleKey pressedKey = Console.ReadKey(true).Key;

            switch (pressedKey)
            {
                case ConsoleKey.Escape:
                    return;

                case ConsoleKey.Backspace:
                    return;

                default:
                    break;
            }



            goto LabelDrawInformation;
        }

        private static (int scrollBarStart, int scrollBarEnd) GetScrollbarRange(int totalEntries, int errorsDisplayedAtOnce, int cacheViewStartIndex)
        {
            int scrollBarHeight = (int)Math.Round((double)errorsDisplayedAtOnce / totalEntries * errorsDisplayedAtOnce);

            if (scrollBarHeight <= 0)
            {
                scrollBarHeight = 1;
            }

            double entriesPerPart = (double)cacheViewStartIndex / (totalEntries - errorsDisplayedAtOnce);

            int scrollBarStart = (int)Math.Round(entriesPerPart * (errorsDisplayedAtOnce - scrollBarHeight));
            int scrollBarEnd = scrollBarStart + scrollBarHeight;

            return (scrollBarStart, scrollBarEnd);
        }

        private static bool ShouldSendEmailAlert(ErrorCacheEntry newEntry)
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTimeSeconds = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            if (unixTimeSeconds - _lastEmailAlertUnixSeconds <= _emailAlertsCooldownSeconds)
            {
                ActivityLogger.Log(_currentSection, $"[WARNING] - Last email was sent less than {_emailAlertsCooldownSeconds} seconds ago.", true);
                return false;
            }

            foreach (ErrorCacheEntry alertedEntry in _errorAlerted)
            {
                bool matchingCategory = alertedEntry.errorCategory == newEntry.errorCategory;
                bool matchingSection = alertedEntry.section == newEntry.section;

                if (matchingCategory && matchingSection)
                {
                    ActivityLogger.Log(_currentSection, $"[WARNING] - This specific category/module configuration was already sent as an email alert.", true);
                    return false;
                }
            }

            ActivityLogger.Log(_currentSection, $"This specific category/module configuration was not sent previously as an email alert.", true);

            _lastEmailAlertUnixSeconds = unixTimeSeconds;

            return true;
        }
    }
}