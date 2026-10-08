using Microsoft.AspNetCore.Mvc;
using FinalWebapp.Services;

namespace FinalWebapp.ViewComponents
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly IUserService _userService;

        public NavbarViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (_userService.IsAuthenticated())
            {
                var user = await _userService.GetCurrentUserAsync();
                return View("LoggedIn", user);
            }
            else
            {
                return View("LoggedOut");
            }
        }
    }
}