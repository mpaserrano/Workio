using Microsoft.AspNetCore.Mvc;
using Workio.Models;
using Workio.ViewModels;

namespace Workio.Views.Teams.Components
{
    public class NewChatModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<ChatViewModel> chats)
        {
            return View(chats);
        }
    }
}
