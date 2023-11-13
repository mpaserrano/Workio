using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Services
{
    public class BlockService : IBlockService
    {
        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private ApplicationDbContext _context;

        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public BlockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BlockedUsersModel>> GetBlocksAsync()
        {
            return await _context.BlockedUsersModel.ToListAsync();
        }

        public async Task<BlockedUsersModel> AddBlock(BlockedUsersModel blocked)
        {
            _context.Add(blocked);
            var connection = await _context.Connections.Where(c => (c.RequestedUserId == blocked.SourceUserId || c.UserId == blocked.SourceUserId) && (c.RequestedUserId == blocked.BlockedUserId || c.UserId == blocked.BlockedUserId)).FirstOrDefaultAsync();
            if (connection != null)
            {
                _context.Connections.Remove(connection);
            }
            await _context.SaveChangesAsync();
            return blocked;
        }

        public async Task<BlockedUsersModel> RemoveBlock(BlockedUsersModel blocked)
        {
            _context.Remove(blocked);
            await _context.SaveChangesAsync();
            return blocked;
        }

    }
}
