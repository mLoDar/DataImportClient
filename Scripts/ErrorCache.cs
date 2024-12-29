using System.Diagnostics;

using DataImportClient.Ressources;





namespace DataImportClient.Scripts
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
            int startIndex = 0;



            Console.Clear();

        LabelDrawErrorCache:

            Console.SetCursorPosition(0, 4);



            if (_entries.Count <= 0)
            {
                DisplayZeroErrorSituation();
                return;
            }



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

            for (int currentErrorIndex = startIndex; currentErrorIndex < startIndex + errorsDisplayedAtOnce && currentErrorIndex < _entries.Count; currentErrorIndex++)
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
                    if (startIndex + 1 <= _entries.Count - errorsDisplayedAtOnce)
                    {
                        startIndex += 1;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (startIndex - 1 >= 0)
                    {
                        startIndex -= 1;
                    }
                    break;

                case ConsoleKey.Escape:
                    return;

                case ConsoleKey.Backspace:
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
                Console.Clear();

                Console.SetCursorPosition(0, 4);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("             Information");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("             ");
                Console.WriteLine("             There are currently no errors in the cache, yay :)");

                await Task.Delay(5000);
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

                Process.Start("notepad.exe", exportFileName);
            }
            catch
            {
                // TODO: Log exception and add error to error cache
            }
        }

        private static void DisplayZeroErrorSituation()
        {
        LabelDrawInformation:


            
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