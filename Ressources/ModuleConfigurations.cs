namespace DataImportClient.Ressources
{
    internal class ModuleConfigurations
    {
        internal struct WeatherData
        {
            internal decimal longitude;
            internal decimal latitude;
            internal string weatherType;
            internal decimal humidity;
            internal decimal windSpeed;
            internal decimal temperature;
            internal int sunsetUnixSeconds;
            internal int sunriseUnixSeconds;
        }

        internal struct ElectricityConfiguration
        {
            internal string sourceFilePath;
            internal string sourceFilePattern;
            internal string sourceFileIntervalSeconds;
            internal string sqlConnectionString;
            internal string dbTableNamePower;
            internal string dbTableNamePowerfactor;



            internal readonly bool HoldsInvalidValues()
            {
                var stringFields = new string[] { sourceFilePath, sourceFilePattern, sourceFileIntervalSeconds, sqlConnectionString, dbTableNamePower, dbTableNamePowerfactor };
                return stringFields.Any(string.IsNullOrEmpty);
            }
        }

        internal struct DistrictHeatConfiguration
        {
            internal string sourceFilePath;
            internal string sourceFilePattern;
            internal string sourceFileIntervalSeconds;
            internal string sqlConnectionString;
            internal string dbTableName;



            internal readonly bool HoldsInvalidValues()
            {
                var stringFields = new string[] { sourceFilePath, sourceFilePattern, sourceFileIntervalSeconds, sqlConnectionString, dbTableName };
                return stringFields.Any(string.IsNullOrEmpty);
            }
        }
    }
}