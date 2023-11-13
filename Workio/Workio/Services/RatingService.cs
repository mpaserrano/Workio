using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Services
{
    /// <summary>
    /// Implementação da interface IRatingService
    /// </summary>
    public class RatingService : IRatingService
    {
        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private readonly ApplicationDbContext _context;
        private UserManager<User> _userManager;
        private IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        ///     Class constructor 
        ///     
        /// </summary>
        /// 
        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        /// <param name="userManager">UserManager (Identity)</param>
        /// <param name="httpContextAccessor">HttpContextAccessor</param>
        public RatingService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Adiciona o rating a um utilizador
        /// </summary>
        /// <param name="RatingModel">Rating</param>
        /// <returns>Rating</returns>

        public async Task<RatingModel> AddRating(RatingModel rating)
        {
            _context.RatingModel.Add(rating);
            await _context.SaveChangesAsync();
            return rating;
        }
        /// <summary>
        /// Obtem todos ratings
        /// </summary>
        /// <returns>Todos os ratings</returns>


        public async Task<List<RatingModel>> GetRatings()
        {
            return await _context.RatingModel.ToListAsync();
        }
        /// <summary>
        /// Remove o rating
        /// </summary>
        /// <param name="RatingModel">Rating</param>
        /// <returns>Rating</returns>

        public async Task<RatingModel> RemoveRating(RatingModel rating)
        {
            _context.RatingModel.Remove(rating);
            await _context.SaveChangesAsync();
            return rating;
        }
        /// <summary>
        /// Atualiza o rating
        /// </summary>
        /// <param name="RatingModel">Rating</param>
        /// <returns>Rating</returns>

        public async Task<bool> UpdateRating(RatingModel rating)
        {
            var success = 0;

            try
            {
                _context.Update(rating);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }

        /// <summary>
        /// Obtem o rating médio de um utilizador
        /// </summary>
        /// <param name="id">Utilizador</param>
        /// <returns>Rating médio</returns>


        public async Task<double> GetAverageRating(Guid id)
        {
            var ratings = await _context.RatingModel.Where(r => r.ReceiverId == id).Select(r => r.Rating).ToListAsync();
            double averageRatings = Math.Round(ratings.Average(), 1);
            return averageRatings;
        }

        /// <summary>
        /// Obtem o valor médio das avaliações de um utilizador com valores decimais
        /// </summary>
        /// <param name="id">Utilizador</param>
        /// <returns>Rating médio</returns>
        public async Task<double> GetTrueAverageRating(Guid id)
        {
            var ratings = await _context.RatingModel.Where(r => r.ReceiverId == id).Select(r => r.Rating).ToListAsync();

            if (ratings.Count == 0) return 0;

            double averageRatings = Math.Round(ratings.Average(), 1);
            return averageRatings;
        }

        /// <summary>
        /// Verifica se um utilizador já foi avaliado por outro
        /// </summary>
        /// <param name="id">Utilizador</param>
        /// <returns>true se ja tiver sido avaliado pelo utilizador a avaliar, false caso contrário</returns>
        public bool IsRated(Guid id)
        {
            var isRated = _context.RatingModel.Any(r => r.ReceiverId == id);
            return isRated;
        }

        /// <summary>
        /// Obtem o numero de ratings de um utilizador
        /// </summary>
        /// <param name="id">Utilizador</param>
        /// <returns>Numero de ratings de um utilizador</returns>

        public async Task<int> GetNumberOfRatings(Guid id)
        {
            var ratings = await _context.RatingModel.Where(r => r.ReceiverId == id).Select(r => r.Rating).ToListAsync();
            int number = ratings.Count;
            return number;

        }

        /// <summary>
        /// Obtem uma rating por id
        /// </summary>
        /// <param name="id">Id da rating</param>
        /// <returns>Rating Model - Rating com o Id correspondente</returns>

        public async Task<RatingModel> GetRatingById(Guid id)
        {
            return await _context.RatingModel.Where(r => r.RatingId == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Verifica se um utilizador já foi avaliado pelo o utilizador logado
        /// </summary>
        /// <param name="id">Id do utilizador avaliado</param>
        /// <returns>Verdade se já tiver sido, false caso contrário</returns>

        public async Task<bool> IsAlreadyRated(Guid id)
        {
            User user = await GetCurrentUser();
            Guid userId = Guid.Parse(user.Id);
            return _context.RatingModel.Any(r => r.RaterId == userId && r.ReceiverId == id);
        }

        /// <summary>
        /// Obtem o id da rating da qual o utilizador atual avaliou outro utilizador
        /// </summary>
        /// <param name="id">Id do utilizador avaliado</param>
        /// <returns>Guid - id da rating</returns>
        public async Task<Guid> GetRatingId(Guid id)
        {
            User user = await GetCurrentUser();
            Guid userId = Guid.Parse(user.Id);
            return await _context.RatingModel.Where(r => r.RaterId == userId && r.ReceiverId == id).Select(r => r.RatingId).FirstOrDefaultAsync();
        }




        private Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public async Task<RatingModel> GetRatingByRatingUserId(Guid ratingUserId, Guid ratedUserId)
        {
            return await _context.RatingModel.Where(r => r.RaterId == ratingUserId && r.ReceiverId == ratedUserId).FirstOrDefaultAsync();
        }
    }
}