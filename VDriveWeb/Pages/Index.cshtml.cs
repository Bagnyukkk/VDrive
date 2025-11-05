using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using VDriveWeb.Dtos;
using VDriveWeb.Services;

namespace VDriveWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly VDriveApiClient _apiClient;

        public IndexModel(VDriveApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [BindProperty]
        public FileActionDto FileAction { get; set; }
        public IEnumerable<FileDetailDto> Files { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentOrder { get; set; }
        public string CurrentFilter { get; set; }
        public string StatusMessage { get; set; }

        // Для завантаження файлів (підтримка IFormFile для Razor Pages)
        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        public async Task<IActionResult> OnGetAsync(string sortBy, string order, string filter)
        {
            // Примусова авторизація: якщо немає токену, перенаправити на логін
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentSort = sortBy ?? "filename";
            CurrentOrder = order ?? "asc";
            CurrentFilter = filter ?? "all";

            try
            {
                Files = await _apiClient.GetFilesAsync(CurrentSort, CurrentOrder, CurrentFilter);
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Помилка завантаження файлів: {ex.Message}";
                Files = Enumerable.Empty<FileDetailDto>();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (UploadedFile == null)
            {
                StatusMessage = "Помилка: Не вибрано файл для завантаження.";
                return RedirectToPage();
            }

            try
            {
                await _apiClient.UploadFileAsync(UploadedFile);
                StatusMessage = $"Файл '{UploadedFile.FileName}' успішно завантажено.";
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Помилка завантаження: {ex.Message}";
            }

            return RedirectToPage(); // Перезавантаження сторінки для оновлення списку
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid deleteFileId, string deleteFileName)
        {
            // Тепер перевіряємо Guid.Empty
            if (deleteFileId == Guid.Empty)
            {
                StatusMessage = "Помилка: Недійсний ідентифікатор файлу.";
                return RedirectToPage();
            }

            try
            {
                await _apiClient.DeleteFileAsync(deleteFileId);
                StatusMessage = $"Файл '{deleteFileName}' успішно видалено.";
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Помилка видалення: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDownloadAsync(Guid downloadFileId, string downloadFileName)
        {
            if (downloadFileId == Guid.Empty)
            {
                StatusMessage = "Помилка: Недійсний ідентифікатор файлу для завантаження.";
                return RedirectToPage();
            }

            try
            {
                var fileStream = await _apiClient.DownloadFileAsync(downloadFileId);
                var contentType = MimeTypes.GetMimeType(downloadFileName);

                return File(fileStream, contentType, downloadFileName);
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Помилка завантаження файлу: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
