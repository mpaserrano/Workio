using Workio.Models;
using Workio.Models.Events;

namespace Workio.Services.Events
{
    /// <summary>
    /// Interface de serviços para guardar dados relativamente aos eventos na base de dados.
    /// </summary>
    public interface IEventsService
    {
        /// <summary>
        /// Cria um evento e guarda na base de dados
        /// </summary>
        /// <param name="event">Objeto com os dados do evento</param>
        /// <returns>True se foi guardada com sucesso, false caso contrário</returns>
        public Task<bool> CreateEvent(Event @event);

        /// <summary>
        /// Retorn todos os eventos na base de dados
        /// </summary>
        /// <returns>Lista de eventos</returns>
        public Task<ICollection<Event>> GetEvents();

        /// <summary>
        /// Retorna um evento fornecendo um id especifico
        /// </summary>
        /// <param name="id">id do evento</param>
        /// <returns>Objeto do tipo evento</returns>
        public Task<Event> GetEvent(Guid id);
        /// <summary>
        /// Retorna uma lista de eventos com todos os eventos promovidos pela plataforma
        /// </summary>
        /// <returns>Coleção de eventos promovidos</returns>
        public Task<ICollection<Event>> GetFeaturedEvents();
        /// <summary>
        /// Obtem uma lista de eventos ordenada pelo diferença entre upvotes e downvotes de um evento
        /// </summary>
        /// <param name="maxQuantity">Número máximo de eventos a retornar</param>
        /// <returns>Lista dos eventos mais populares</returns>
        public Task<ICollection<Event>> GetTopEvents(int maxQuantity);

        /// <summary>
        /// Obtem uma lista de eventos que vão ocorrer nos próximos dias
        /// </summary>
        /// <param name="upto">Máximo de dias a procurar (até que dia devem ser retornados)</param>
        /// <param name="maxQuantity">Número máximo de eventos a serem retornados</param>
        /// <returns>Lista de eventos a acontecer nos próximos dias</returns>
        public Task<ICollection<Event>> GetSoonEvents(int upto, int maxQuantity);

        /// <summary>
        /// Atualiza um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        public Task<bool> UpdateEvent(Event @event);

        /// <summary>
        /// Apaga um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        public Task<Event> RemoveEvent(Event @event);
        /// <summary>
        /// Adiciona à base de dados que o utilizador logado está interessado no evento que se encontra
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador foi adicionado com sucesso, falso se não foi</returns>
        public Task<bool> AddInterestedUser(Guid id);
        /// <summary>
        /// Verifica se o utilizador logado de momento ja mostrou interesse no evento que se encotra
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador já está interessado, falso se não estiver</returns>
        public Task<bool> IsUserInterested(Guid id);
        /// <summary>
        /// Remove um utilizador interessado 
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se a operação tece sucesso, false se não teve</returns>
        public Task<bool> RemoveInterestedUser(Guid id);
        /// <summary>
        /// Verifica se o utilizador já deu downvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se já deu downvote, falso se não</returns>
        public Task<bool> AlreadyDownvoted(Guid id);
        /// <summary>
        /// Verifica se o utilizador já deu upvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se já deu upvote, falso se não</returns>
        public Task<bool> AlreadyUpvoted(Guid id);
        /// <summary>
        /// Verifica se o utilizador pode dar upvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se der upvote, falso se não</returns>
        public Task<bool> UpVote(Guid id);
        /// <summary>
        /// Verifica se o utilizador pode dar downvote
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se der downvote, falso se não</returns>
        public Task<bool> Downvote(Guid id);
        /// <summary>
        /// Obtem o numero de upvotes de um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>int, numero de upvotes</returns>
        public Task<int> GetNumberOfUpvotes(Guid id);
        /// <summary>
        /// Obtem o numero de downvotes de um evento
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>int, numero de upvotes</returns>
        public Task<int> GetNumberOfDownvotes(Guid id);
        /// <summary>
        /// Verifica se o utilizador atual é o criador do evento
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>true se for, falso caso contrário</returns>
        public Task<bool> isCreator(Guid id);
        /// <summary>
        /// Verifica se o evento já terminou
        /// </summary>
        /// <param name="id">Id do evento </param>
        /// <returns>true se já tiver terminado, falso caso contrário</returns>
        public Task<bool> isFinished(Guid id);
        /// <summary>
        /// Remove upvote se já existir
        /// </summary>
        /// <param name="id">Id do evento </param>
        /// <returns>true se o upvote for removido com sucesso, falso caso contrário</returns>
        public Task<bool> RemoveUpvote(Guid id);
        /// <summary>
        /// Remove o downvote se já existir
        /// </summary>
        /// <param name="id">Id do evento </param>
        /// <returns>true se o upvote for removido com sucesso, falso caso contrário</returns>
        public Task<bool> RemoveDownvote(Guid id);
        /// <summary>
        /// Remove uma equipa interessada
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se a operação tece sucesso, false se não teve</returns>
        public Task<bool> RemoveInterestedTeam(Guid id);
        /// <summary>
        /// Verifica se o utilizador logado de momento ja mostrou interesse numa equipa que lidere no evento que se encotra
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador já está interessado, falso se não estiver</returns>
        public Task<bool> IsTeamInterested(Guid id);
        /// <summary>
        /// Adiciona à base de dados a equipa interessada no evento.
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <returns>true se o utilizador foi adicionado com sucesso, falso se não foi</returns>
        public Task<bool> AddInterestedTeam(Team team, Guid id);


        /// <summary>
        /// Muda o state
        /// </summary>
        /// <param name="id">Id do evento</param>
        /// <param name="state">state do evento</param>
        public Task<bool> ChangeEventStatus(Guid id, EventState state);

        /// <summary>
        /// Retorna uma coleção de eventos em que a data de inicío dos eventos está no intrevalo recebido.
        /// </summary>
        /// <param name="minDate">Minímo do intrevalo de procura.</param>
        /// <param name="maxDate">Máximo do intrevalo de procura.</param>
        /// <returns>Coleção de eventos dentro do intrevalo.</returns>
        public Task<ICollection<Event>> GetEventsBetweenDates(DateTime minDate, DateTime maxDate);

        /// <summary>
        /// Atualiza o estado de todos os eventos de acordo com a data de início, de fim e o seu estado atual.
        /// </summary>
        /// <returns>True: Se as atualizaçãoes foram feitas com sucesso. False: Caso contrário.</returns>
        public Task<bool> RefreshAllEventsState();

        /// <summary>
        /// Obtem todos os eventos que um user está interessado, tanto como user ou equipa.
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Lista de eventos</returns>
        public Task<List<Event>> GetAllUserInterestedEvents(Guid userId);
    }
}
