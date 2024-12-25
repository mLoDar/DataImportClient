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
            return $"[{dateTime:yyyy-MM-dd HH:mm:ss}] - [{processId} | {section}] - {error} {detail}";
        }
    }



    internal class ErrorCache
    {
        private readonly List<ErrorCacheEntry> _entries = [];

        const int maxEntries = 30;



        internal List<ErrorCacheEntry> Entries
        {
            get
            {
                return _entries;
            }
        }



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
    }
}