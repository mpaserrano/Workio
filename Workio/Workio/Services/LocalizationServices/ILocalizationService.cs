using Workio.Models;
namespace Workio.Services.LocalizationServices
{
    public interface ILocalizationService
    {
        public Task<List<Localization>> GetLocalizations();
    }
}
