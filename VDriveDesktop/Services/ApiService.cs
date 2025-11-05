using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using VDriveDesktop.Models;

namespace VDriveDesktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7255";

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var loginDto = new { Username = username, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                return tokenResponse["token"];
            }

            return null;
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(string fullName, string username, string password)
        {
            var registerDto = new { FullName = fullName, Username = username, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(registerDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
        }

        public async Task<(byte[] FileContents, string FileName)> DownloadFileAsync(string token, Guid fileId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/Files/download/{fileId}");

            if (response.IsSuccessStatusCode)
            {
                var fileContents = await response.Content.ReadAsByteArrayAsync();
                var fileName = response.Content.Headers.ContentDisposition?.FileName.Trim('"') ?? "downloaded_file";
                return (fileContents, fileName);
            }

            return (null, null);
        }

        public async Task<(byte[] FileContents, string ContentType, string FileName)> GetFileContentAsync(string token, Guid fileId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/Files/content/{fileId}");

            if (response.IsSuccessStatusCode)
            {
                var fileContents = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.ToString();
                var fileName = response.Content.Headers.ContentDisposition?.FileName.Trim('"') ?? "file";
                return (fileContents, contentType, fileName);
            }

            return (null, null, null);
        }

        public async Task<List<FileDetailDto>> GetFilesAsync(string token, string sortBy = "filename", string order = "asc", string filter = "all")
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{BaseUrl}/api/Files?sortBy={sortBy}&order={order}&filter={filter}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FileDetailDto>>(content);
            }

            return new List<FileDetailDto>();
        }

        public async Task<bool> UploadFileAsync(string token, string filePath)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync($"{BaseUrl}/api/Files/upload", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFileAsync(string token, Guid fileId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/api/Files/{fileId}");
            return response.IsSuccessStatusCode;
        }
    }
}
