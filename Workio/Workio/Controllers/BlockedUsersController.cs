using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models;
using Workio.Services;
using Workio.Services.Interfaces;

namespace Workio.Controllers
{
    /// <summary>
    /// Controlador que dá handle dos blocks
    /// </summary>
    [Authorize]
    public class BlockedUsersController : Controller
    {
        private IBlockService _blockService { get; set; }
        private IUserService _userService { get; set; }

        /// <summary>
        /// Construtor da classe.
        /// </summary>
        /// <param name="blockService">Parametro de Serviço de Bloqueios para inicialização</param>
        /// <param name="userService">Parametro de Serviço de Users para inicialização</param>
        /// <returns>Task<IActionResult> - Reendireciona para a ação "Index" do controlador "User"</returns>
        public BlockedUsersController(IBlockService blockService, IUserService userService)
        {
            _userService= userService;
            _blockService= blockService;
        }

        /// <summary>
        /// Ação para bloquear um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador a ser bloqueado</param>
        /// <returns>Task<IActionResult> - Reendireciona para a ação "Index" do controlador "User"</returns>
        public async Task<ActionResult> BlockUser(Guid id)
        {
            var currentUserId = CurrentUserId();

            if (id == currentUserId)
            {
                return RedirectToAction("Index", "User", new { id = id });
            }

            var currentUser = await _userService.GetUser(currentUserId);
            var user = await _userService.GetUser(id);
            BlockedUsersModel blocked = new BlockedUsersModel() { BlockedUser = user, BlockedUserId = "" + user.Id, SourceUser = currentUser, SourceUserId = "" + currentUserId};
            await _blockService.AddBlock(blocked);
            return RedirectToAction("Index", "User", new { id = id });
        }

        /// <summary>
        /// Ação para listar os utilizadores bloqueados por um utilizador
        /// </summary>
        /// <returns>Task<IActionResult> - Reendireciona para a página que lista os utilizadores bloqueados</returns>
        public async Task<ActionResult> BlockedUsers()
        {
            var currentUserId = CurrentUserId();
            var allBlocked = await _blockService.GetBlocksAsync();
            var allUsers = await _userService.GetUsersAsync();
            var blocked = from b in allBlocked where Guid.Parse(b.SourceUserId) == currentUserId select new {SourceId = b.SourceUserId, BlockedId = b.BlockedUserId};
            List<User> blockedUsers = new List<User>();
            foreach (var u in allUsers)
            {
                foreach(var b in blocked)
                {
                    if(b.BlockedId == u.Id)
                    {
                        blockedUsers.Add(u);
                    }
                }
            }
            ViewBag.BlockedList = blockedUsers;
            return View();
        }

        /// <summary>
        /// Ação para desbloquear um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador que se está a remover o bloqueio</param>
        /// <returns>Task<IActionResult> - Reendireciona para a ação "BlockedList" do controlador "BlockedUsers"</returns>
        public async Task<ActionResult> Unblock(Guid id)
        {
            var currentUserId = CurrentUserId();

            if (id == currentUserId)
            {
                return RedirectToAction("Index", "User", new { id = id });
            }

            var allBlocks = await _blockService.GetBlocksAsync();
            BlockedUsersModel blocked = new BlockedUsersModel();
            foreach (var b in allBlocks)
            {
                if(Guid.Parse(b.SourceUserId) == currentUserId && Guid.Parse(b.BlockedUserId) == id)
                {
                    blocked = b; break;
                }
            }
            await _blockService.RemoveBlock(blocked);
            return RedirectToAction("Index", "User", new { id = id });
        }

        /// <summary>
        /// Ação para desbloquear um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador que se está a remover o bloqueio</param>
        /// <returns>Task<IActionResult> - Reendireciona para a ação "BlockedList" do controlador "BlockedUsers"</returns>
        public async Task<ActionResult> UnblockFromConnections(Guid id)
        {
            var currentUserId = CurrentUserId();

            if (id == currentUserId)
            {
                return RedirectToAction("Index", "User", new { id = id });
            }

            var allBlocks = await _blockService.GetBlocksAsync();
            BlockedUsersModel blocked = new BlockedUsersModel();
            foreach (var b in allBlocks)
            {
                if (Guid.Parse(b.SourceUserId) == currentUserId && Guid.Parse(b.BlockedUserId) == id)
                {
                    blocked = b; break;
                }
            }
            await _blockService.RemoveBlock(blocked);
            return RedirectToAction("Connections", "User");
        }


        private Guid CurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
