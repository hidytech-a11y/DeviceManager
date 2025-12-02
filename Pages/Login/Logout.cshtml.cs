public async Task<IActionResult> OnPost()

with:

public async Task<IActionResult> OnPost()
{
    await _signInManager.SignOutAsync();
    return RedirectToPage("/Account/Login");
}