using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Events;

namespace Workio.ViewComponents
{
    public class EventPreviewCardViewComponent : ViewComponent
    {
        private readonly IEventsService _eventsService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public EventPreviewCardViewComponent(IEventsService eventsService, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _eventsService = eventsService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IViewComponentResult Invoke(Event @event)
        {
            //var eventItem = _eventsService.GetEvent(id);
            if (!@event.IsInPerson)
            {
                @event.LatitudeText = "null";
                @event.LongitudeText = "null";
            }
            else
            {
                @event.LatitudeText = @event.Latitude.ToString().Replace(",", ".");
                @event.LongitudeText = @event.Longitude.ToString().Replace(",", ".");
            }

            if(@event != null)
            {
                if (_signInManager.IsSignedIn(UserClaimsPrincipal))
                {
                    ViewBag.hasUpvote = false;
                    ViewBag.hasDownvote = false;
                    ViewBag.isCreator = false;

                    var userId = _userManager.GetUserId(UserClaimsPrincipal);

                    if (@event.UserId == userId) ViewBag.isCreator = true;
                    else
                    {
                        var reaction = @event.EventReactions.Where(r => r.UserId.ToString() == userId).FirstOrDefault();
                        if (reaction != null)
                        {
                            if (reaction.ReactionType == EventReactionType.UpVote) ViewBag.hasUpvote = true;
                            else if (reaction.ReactionType == EventReactionType.DownVote) ViewBag.hasDownvote = true;
                        }
                    }

                    
                }
            }

            return View(@event);
        }
    }
}
