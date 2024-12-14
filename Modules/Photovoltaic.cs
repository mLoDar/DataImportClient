using DataImportClient.Scripts;





namespace DataImportClient.Modules
{
    internal class Photovoltaic
    {
        private const string _currentSection = "ModulePhotovoltaic";
        
        private ModuleState _moduleState;
        private int _errorCount;



        internal ModuleState State
        {
            get => _moduleState;
        }

        internal int ErrorCount
        {
            get => _errorCount;
        }

        internal Photovoltaic()
        {
            _moduleState = ModuleState.Running;
            _errorCount = 0;
        }



        internal async Task Main()
        {
            ActivityLogger.Log(_currentSection, "Entering module 'Photovoltaic'.");
        }
    }
}