using System.Diagnostics;
using DataImportClient.Ressources;
using DataImportClient.Scripts;





namespace DataImportClient.Modules
{
    struct ErrorCacheEntry
    {
        internal DateTime dateTime;
        internal int processId;
        internal string section;
        internal string error;
        internal string detail;

        internal readonly string ToMinimalistic()
        {
            return $"[{section}] - {error}";
        }

        internal readonly string ToDetailed()
        {
            return $"[{dateTime:yyyy-MM-dd HH:mm:ss}] - [ProcessId: {processId} | Section: {section}] - {error} {detail}";
        }
    }



    internal class ErrorCache
    {
        private const string _currentSection = "ErrorCache";

        private readonly List<ErrorCacheEntry> _entries = [];

        const int maxEntries = 30;

        private static readonly ApplicationSettings.Paths _appPaths = new();



        internal void AddEntry(string errorSection, string errorMessage, string detailedError)
        {
            ErrorCacheEntry entry = new()
            {
                dateTime = DateTime.Now,
                processId = Environment.ProcessId,
                section = errorSection,
                error = errorMessage,
                detail = detailedError
            };

            if (_entries.Count + 1 > maxEntries)
            {
                _entries.RemoveAt(0);
            }

            _entries.Add(entry);
        }

        internal void RemoveSectionFromCache(string errorSection)
        {
            List<ErrorCacheEntry> entriesForRemoval = [];

            foreach (ErrorCacheEntry entry in _entries)
            {
                if (entry.section.Equals(errorSection))
                {
                    entriesForRemoval.Add(entry);
                }
            }

            foreach (ErrorCacheEntry entry in entriesForRemoval)
            {
                _entries.Remove(entry);
            }
        }

        internal void DisplayMinimalistic()
        {
            int cacheViewStartIndex = 0;

            if (_entries.Count <= 0)
            {
                ActivityLogger.Log(_currentSection, "The error cache currently does not hold any entries.");

                DisplayZeroErrorSituation();

                ActivityLogger.Log(_currentSection, "Returning to miscellaneous selection.");
                return;
            }



            ActivityLogger.Log(_currentSection, "Displaying a minimal error cache, waiting for return signal.");
            ActivityLogger.Log(_currentSection, $"The error cache currently holds {(_entries.Count > 1 ? "1 entry" : $"{_entries.Count} entries")}.");



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
            Console.WriteLine("             ┌──────────────────────────────────────────────────────────────────────────────────────────┐");



            int maxErrorLength = 88;
            int errorsDisplayedAtOnce = 10;

            for (int currentErrorIndex = cacheViewStartIndex; currentErrorIndex < cacheViewStartIndex + errorsDisplayedAtOnce && currentErrorIndex < _entries.Count; currentErrorIndex++)
            {
                string currentError = _entries[currentErrorIndex].ToMinimalistic();

                if (currentError.Length > maxErrorLength)
                {
                    currentError = currentError[..(maxErrorLength - 4)] + " ...";
                }

                currentError = currentError.PadRight(maxErrorLength);

                Console.WriteLine($"             │ {currentError} │");
            }



            Console.WriteLine("             └──────────────────────────────────────────────────────────────────────────────────────────┘");
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
                    if (cacheViewStartIndex + 1 <= _entries.Count - errorsDisplayedAtOnce)
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
            if (_entries.Count <= 0)
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
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

            string exportFileName = Path.Combine(_appPaths.clientFolder, $"errorCacheExport-{unixTime}.txt");


            foreach (ErrorCacheEntry entry in _entries)
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
    }
}