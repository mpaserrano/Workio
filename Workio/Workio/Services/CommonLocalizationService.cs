using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Reflection;
using Workio.Resources;

namespace Workio.Services
{
    public class CommonLocalizationService
    {
        private readonly IStringLocalizer localizer;
        public CommonLocalizationService(IStringLocalizerFactory factory)
        {
            var assemblyName = new AssemblyName(typeof(CommonResources).GetTypeInfo().Assembly.FullName);
            localizer = factory.Create(nameof(CommonResources), assemblyName.Name);
        }

        public string Get(string key)
        {
            return localizer[key];
        }

        public string GetLocalizedString(string resourceKey, string languageCode)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(languageCode);
            var localizedString = localizer[resourceKey].Value;
            return localizedString;
        }
    }
}
