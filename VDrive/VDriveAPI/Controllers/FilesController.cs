using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using VDriveAPI.Dtos;
using VDriveAPI.Entities;
using VDriveAPI.Options;

namespace VDriveAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController(VDriveDbContext context, IOptions<StorageOptions> storageOptions) : ControllerBase
    {
        private readonly string _storagePath = storageOptions.Value.Path;

        [HttpGet]
        // GET: api/files
        public async Task<ActionResult<IEnumerable<FileDetailDto>>> GetFiles([FromQuery] string? sortBy, [FromQuery] string? order, [FromQuery] string? filter)
        {
            var userId = GetCurrentUserId();

            var query = context.Files
                              .Where(f => f.UploaderId == userId)
                              .Include(f => f.Uploader)
                              .Include(f => f.LastEditor)
                              .AsQueryable();

            // Варіант 6: .cpp, .png
            // Варіант 7: .c, .jpg
            if (!string.IsNullOrEmpty(filter) && filter.ToLower() != "all")
            {
                // Додайте потрібні розширення тут
                var allowedExtensions = new[] { ".cpp", ".png", ".c", ".jpg" };
                if (allowedExtensions.Contains("." + filter.ToLower()))
                {
                    query = query.Where(f => f.FileType == "." + filter.ToLower());
                }
            }

            if (sortBy?.ToLower() == "filename")
            {
                bool isDescending = order?.ToLower() == "desc";
                if (isDescending)
                {
                    query = query.OrderByDescending(f => f.FileName);
                }
                else
                {
                    query = query.OrderBy(f => f.FileName);
                }
            }
            else
            {
                query = query.OrderBy(f => f.FileName);
            }

            var files = await query
                .Select(f => new FileDetailDto
                {
                    FileId = f.Id,
                    FileName = f.FileName,
                    CreationDate = f.CreationDate,
                    ModificationDate = f.ModificationDate,
                    UploaderName = f.Uploader.Username,
                    LastEditorName = f.LastEditor.Username
                })
                .ToListAsync();

            return Ok(files);
        }

        // POST: api/files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not selected or empty.");

            var userId = GetCurrentUserId();
            var userStoragePath = Path.Combine(_storagePath, userId.ToString());
            if (!Directory.Exists(userStoragePath))
            {
                Directory.CreateDirectory(userStoragePath);
            }

            var filePath = Path.Combine(userStoragePath, file.FileName);

            if (System.IO.File.Exists(filePath))
                return BadRequest("File with this name already exists.");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileMetadata = new FileMetaData
            {
                FileName = file.FileName,
                FileType = Path.GetExtension(file.FileName).ToLower(),
                StoragePath = filePath,
                CreationDate = DateTime.UtcNow,
                ModificationDate = DateTime.UtcNow,
                UploaderId = userId,
                LastEditorId = userId
            };

            context.Files.Add(fileMetadata);
            await context.SaveChangesAsync();

            return Ok(new { Message = "File uploaded successfully.", fileId = fileMetadata.Id });
        }

        [HttpGet("content/{id}")]
        public async Task<IActionResult> GetFileContent(Guid id)
        {
            var userId = GetCurrentUserId();
            var fileMetadata = await context.Files
                .FirstOrDefaultAsync(f => f.Id == id && f.UploaderId == userId);

            if (fileMetadata == null)
                return NotFound("File not found or you don't have access.");

            var fileExtension = fileMetadata.FileType.ToLower();
            if (fileExtension != ".c" && fileExtension != ".jpg")
            {
                return BadRequest("This file type cannot be displayed.");
            }

            if (!System.IO.File.Exists(fileMetadata.StoragePath))
                return NotFound("File not found on the server storage.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(fileMetadata.StoragePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = fileExtension == ".jpg" ? "image/jpeg" : "text/plain";

            return File(memory, contentType, fileMetadata.FileName);
        }

        // GET: api/files/download/5
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var userId = GetCurrentUserId();
            var fileMetadata = await context.Files
                .FirstOrDefaultAsync(f => f.Id == id && f.UploaderId == userId);

            if (fileMetadata == null)
                return NotFound("File not found or you don't have access.");

            if (!System.IO.File.Exists(fileMetadata.StoragePath))
                return NotFound("File not found on the server storage.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(fileMetadata.StoragePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = "APPLICATION/octet-stream";
            return File(memory, contentType, fileMetadata.FileName);
        }

        // DELETE: api/files/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(Guid id)
        {
            var userId = GetCurrentUserId();
            var fileMetadata = await context.Files
                .FirstOrDefaultAsync(f => f.Id == id && f.UploaderId == userId);

            if (fileMetadata == null)
                return NotFound("File not found or you don't have access.");

            if (System.IO.File.Exists(fileMetadata.StoragePath))
            {
                System.IO.File.Delete(fileMetadata.StoragePath);
            }

            context.Files.Remove(fileMetadata);
            await context.SaveChangesAsync();

            return Ok(new { Message = "File deleted successfully." });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            throw new Exception("User ID not found in token.");
        }
    }
}