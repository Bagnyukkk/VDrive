using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VDriveWeb.Services;

namespace VDriveWeb.Pages.File
{
    public class ViewModel : PageModel
    {
        private readonly VDriveApiClient _apiClient;

        public ViewModel(VDriveApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileContent { get; set; }
        public string Base64Image { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, string fileName, string fileType)
        {
            FileName = fileName;
            FileType = fileType.ToLowerInvariant();

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                switch (FileType)
                {
                    case ".c":
                        FileContent = await _apiClient.GetFileContentAsync(id);
                        break;
                    case ".jpg":
                        using (var stream = await _apiClient.DownloadFileAsync(id))
                        {
                            using (var ms = new MemoryStream())
                            {
                                await stream.CopyToAsync(ms);
                                Base64Image = Convert.ToBase64String(ms.ToArray());
                            }
                        }
                        break;
                    default:
                        Message = $"???????? ?????? ???? '{fileType}' ?? ?????????????.";
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                Message = $"??????? ???????????? ?????? ?????: {ex.Message}";
            }

            return Page();
        }
    }
}
