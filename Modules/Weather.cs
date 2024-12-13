using DataImportClient.Scripts;





namespace DataImportClient.Modules
{
    internal class Weather
    {
        private const string currentSection = "ModuleWeather";
        
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

        internal Weather()
        {
            _moduleState = ModuleState.Running;
            _errorCount = 0;
        }



        internal async Task Main()
        {
            ActivityLogger.Log(currentSection, "Entering module 'Weather'.");
        }
    }
}