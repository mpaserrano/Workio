using Workio.Models;

namespace Workio.Services.Interfaces
{
    public interface IBlockService
    {
        /// <summary>
        /// Use este método para obter todos os registos de bloqueios.
        /// </summary>
        /// <returns>Lista de registos de bloqueios</returns>
        Task<List<BlockedUsersModel>> GetBlocksAsync();

        /// <summary>
        /// Use este método para adicionar um bloqueio à base de dados
        /// </summary>
        /// <param name="blocked">Parametro que representa o registo de bloqueio</param>
        /// <returns>Retorna o registo.</returns>
        Task<BlockedUsersModel> AddBlock(BlockedUsersModel blocked);

        /// <summary>
        /// Use este método para remover um bloqueio à base de dados
        /// </summary>
        /// <param name="blocked">Parametro que representa o registo de bloqueio</param>
        /// <returns>Retorna o registo.</returns>
        Task<BlockedUsersModel> RemoveBlock(BlockedUsersModel blocked);

    }
}
