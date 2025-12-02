using System.Threading.Tasks;
using DeviceManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeviceManager.Controllers
{
    [AllowAnonymous] // global AuthorizeFilter is enabled; allow anonymous for login/register
    public class AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager) : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        // GET: /Login
        [HttpGet]
        public IActionResult Index(string? returnUrl = null)
        {
            var model = new LoginViewModel { ReturnUrl = returnUrl };
            return View(model);
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Find user by email (identity user names equal emails in this app)
            var user = await _userManager.FindByEmailAsync(email: model.Email ?? string.Empty);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var userName = user.UserName ?? string.Empty;
            var password = model.Password ?? string.Empty;
            var result = await _signInManager.PasswordSignInAsync(userName, password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: /Login/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Login/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var password = model.Password ?? string.Empty;
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, password: password);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }

            // sign in the newly created user
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        // POST: /Login/Logout (form POST, non-AJAX)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Account");
        }

        // POST: /Acount/LogoutAjax
        // Handles AJAX logout: expects antiforgery token header and returns JSON
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutAjax()
        {
            await _signInManager.SignOutAsync();

            // return a JSON payload so the client script can redirect or update UI
            var redirectUrl = Url.Action("Index", "Account") ?? "/";
            return Json(new { success = true, redirectUrl });
        }
    }
}
