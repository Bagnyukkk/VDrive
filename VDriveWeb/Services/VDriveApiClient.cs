using System.Net.Http.Headers;
using VDriveWeb.Dtos;

namespace VDriveWeb.Services
{
    public class VDriveApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VDriveApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7255/api/");
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/login", loginDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        }

        public async Task RegisterAsync(RegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/register", registerDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<FileDetailDto>> GetFilesAsync(
            string sortBy = "filename",
            string order = "asc",
            string filter = "all")
        {
            AddAuthorizationHeader();
            var url = $"Files?sortBy={sortBy}&order={order}&filter={filter}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<FileDetailDto>>()
                ?? Enumerable.Empty<FileDetailDto>();
        }

        public async Task UploadFileAsync(IFormFile file)
        {
            AddAuthorizationHeader();
            using var content = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);

            var response = await _httpClient.PostAsync("Files/upload", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteFileAsync(Guid fileId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"Files/{fileId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetFileContentAsync(Guid fileId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Files/content/{fileId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<Stream> DownloadFileAsync(Guid fileId)
        {
            AddAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Files/download/{fileId}", HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
