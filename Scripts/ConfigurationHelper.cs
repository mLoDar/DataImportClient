using DataImportClient.Ressources;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;





namespace DataImportClient.Scripts
{
    internal class ConfigurationHelper
    {
        private static readonly ApplicationSettings.Paths _appPaths = new();



        internal static async Task<Exception?> SaveConfiguration(JObject applicationConfiguration)
        {
            try
            {
                string configurationFile = _appPaths.configurationFile;
                string plainTextConfiguration = applicationConfiguration.ToString(Formatting.Indented);

                await File.WriteAllTextAsync(configurationFile, plainTextConfiguration); 
            }
            catch (Exception exception)
            {
                return exception;
            }

            return null;
        }

        internal static async Task<JObject> LoadConfiguration()
        {
            JObject loadedConfiguration = [];

            try
            {
                string configurationFile = _appPaths.configurationFile;
                string plainTextConfiguration = await File.ReadAllTextAsync(configurationFile);

                loadedConfiguration = JObject.Parse(plainTextConfiguration);
            }
            catch (Exception exception)
            {
                loadedConfiguration["error"] = exception.Message;
                return loadedConfiguration;
            }

            return loadedConfiguration;
        }
    }
}