using Microsoft.Extensions.Configuration;

namespace ISPL.NetCoreFramework.Helpers
{
    public static class AppSettingsHelper
    {
        private static IConfiguration? _configuration;

        public static void Init(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string? Get(string key)
        {
            return _configuration?[key];
        }

        public static T? GetSection<T>(string sectionName) where T : new()
        {
            var section = new T();
            if(_configuration == null)
            {
                return section;
            }

            _configuration.GetSection(sectionName).Bind(section);
            return section;
        }
    }
}
