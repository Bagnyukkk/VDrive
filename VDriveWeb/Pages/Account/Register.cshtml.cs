using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VDriveWeb.Dtos;
using VDriveWeb.Services;

namespace VDriveWeb.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly VDriveApiClient _apiClient;

        public RegisterModel(VDriveApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public RegisterDto RegisterInput { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            if (HttpContext.Session.GetString("JwtToken") != null)
            {
                Response.Redirect("/");
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
                await _apiClient.RegisterAsync(RegisterInput);

                Message = "Реєстрація успішна! Тепер ви можете увійти.";
                return RedirectToPage("./Login", new { message = Message });
            }
            catch (HttpRequestException ex)
            {
                string errorDetail = "Помилка реєстрації. Перевірте дані.";
                try
                {
                    var responseBody = ex.Message + ex.InnerException.Message;
                    if (!string.IsNullOrEmpty(responseBody) && !responseBody.Contains('{'))
                    {
                        errorDetail = responseBody;
                    }
                }
                catch
                {
                }

                Message = errorDetail;
                ModelState.AddModelError(string.Empty, Message);
                return Page();
            }
        }
    }
}