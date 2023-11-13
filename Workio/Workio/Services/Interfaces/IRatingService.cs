using Workio.Models;

namespace Workio.Services.Interfaces
{
    public interface IRatingService
    {
        /// <summary>
        /// Obtem todos ratings
        /// </summary>
        /// <returns>Todos os ratings</returns>
        Task<List<RatingModel>> GetRatings();
        /// <summary>
        /// Adiciona o rating a um utilizador
        /// </summary>
        /// <param name="RatingModel">Rating</param>
        /// <returns>Rating</returns>

        Task<RatingModel> AddRating(RatingModel rating);
        /// <summary>
        /// Remove o rating
        /// </summary>
        /// <param name="RatingModel">Rating</param>
        /// <returns>Rating</returns>
        Task<RatingModel> RemoveRating(RatingModel rating);
        /// <summary>
        /// Atualiza o rating
        /// </summary>
        /// <param name="RatingModel">Rating</param>
        /// <returns>Rating</returns>
        Task<bool> UpdateRating(RatingModel rating);
        Task<double> GetAverageRating(Guid id);
        /// <summary>
        /// Verifica se um utilizador já foi avaliado por outro
        /// </summary>
        /// <param name="id">Utilizador</param>
        /// <returns>true se ja tiver sido avaliado pelo utilizador a avaliar, false caso contrário</returns>
        bool IsRated(Guid id);
        Task<int> GetNumberOfRatings(Guid id);

        /// <summary>
        /// Obtem o valor médio das avaliações de um utilizador com valores decimais
        /// </summary>
        /// <param name="id">Utilizador</param>
        /// <returns>Rating médio</returns>
        Task<double> GetTrueAverageRating(Guid id);
        /// <summary>
        /// Obtem uma rating por id
        /// </summary>
        /// <param name="id">Id da rating</param>
        /// <returns>Rating Model - Rating com o Id correspondente</returns>
        Task<RatingModel> GetRatingById(Guid id);
        /// <summary>
        /// Obtem o id da rating da qual o utilizador atual avaliou outro utilizador
        /// </summary>
        /// <param name="id">Id do utilizador avaliado</param>
        /// <returns>Guid - id da rating</returns>
        Task<Guid> GetRatingId(Guid id);
        /// <summary>
        /// Verifica se um utilizador já foi avaliado pelo o utilizador logado
        /// </summary>
        /// <param name="id">Id do utilizador avaliado</param>
        /// <returns>Verdade se já tiver sido, false caso contrário</returns>
        Task<bool> IsAlreadyRated(Guid id);

        /// <summary>
        /// Obtem o rating atribuído a um utilizador por outro
        /// </summary>
        /// <param name="ratingUserId">Id do user q avaliou</param>
        /// <param name="ratedUserId">Id do user avaliado</param>
        /// <returns>Valor avaliado</returns>
        Task<RatingModel> GetRatingByRatingUserId(Guid ratingUserId, Guid ratedUserId);
    }
}
