using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;

namespace Workio.Services.LocalizationServices
{
    public class LocalizationService : ILocalizationService
    {

        private ApplicationDbContext _context;

        public LocalizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Localization>> GetLocalizations()
        {
            return await _context.Localizations.ToListAsync();
        }
    }
}
