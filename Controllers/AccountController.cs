using System.Threading.Tasks;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers
{
    public sealed class AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (!ModelState.IsValid)
                return View();

            var result = await _signInManager.PasswordSignInAsync(email, password, true, false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid login details");
            return View();
        }

        // GET: /Account/Register

        //[Authorize(Roles = "Admin")]
        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult Register()
        //{
        //    return View(new RegisterViewModel());
        //}

        //// POST: /Account/Register
        //[Authorize(Roles = "Admin")]    
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    var user = new IdentityUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email
        //    };

        //    var createUser = await _userManager.CreateAsync(user, model.Password);

        //    if (createUser.Succeeded)
        //    {
        //        var roleExists = await _roleManager.RoleExistsAsync(model.Role);
        //        if (!roleExists)
        //        {
        //            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        //        }

        //        await _userManager.AddToRoleAsync(user, model.Role);

        //        return RedirectToAction("Login");
        //    }

        //    foreach (var error in createUser.Errors)
        //        ModelState.AddModelError("", error.Description);

        //    return View(model);
        //}

        //User Password Reset, Profile Management etc. can be added here
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", "Home");
        }


        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
