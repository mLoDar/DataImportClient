using DataImportClient.Scripts;





namespace DataImportClient.Modules
{
    internal class DistrictHeat
    {
        private const string _currentSection = "ModuleDisctrictHeat";
        
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

        internal DistrictHeat()
        {
            _moduleState = ModuleState.Running;
            _errorCount = 0;
        }



        internal async Task Main()
        {
            ActivityLogger.Log(_currentSection, "Entering module 'DisctrictHeat'.");
        }
    }
}