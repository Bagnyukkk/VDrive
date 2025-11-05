using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VDriveWeb.Dtos;
using VDriveWeb.Services;

namespace VDriveWeb.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly VDriveApiClient _apiClient;

        public LoginModel(VDriveApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public LoginDto LoginInput { get; set; }

        public string Message { get; set; }

        public void OnGet(string message = null)
        {
            if (HttpContext.Session.GetString("JwtToken") != null)
            {
                Response.Redirect("/");
            }

            if (!string.IsNullOrEmpty(message))
            {
                Message = message;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var response = await _apiClient.LoginAsync(LoginInput);

                HttpContext.Session.SetString("JwtToken", response.Token);

                return RedirectToPage("/Index");
            }
            catch (HttpRequestException ex)
            {
                Message = "Помилка входу: " + ex.Message;
                return Page();
            }
        }
    }
}
