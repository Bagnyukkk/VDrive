using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VDriveAPI.Controllers;
using VDriveAPI.Dtos;
using VDriveAPI.Entities;
using VDriveAPI.Options;

namespace VDriveAPI.Tests
{
    public class FilesControllerTests
    {
        private readonly VDriveDbContext _context;
        private readonly FilesController _controller;
        private readonly User _testUser;
        private readonly Guid _testUserId;

        public FilesControllerTests()
        {
            var options = new DbContextOptionsBuilder<VDriveDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new VDriveDbContext(options);

            var storageOptions = Microsoft.Extensions.Options.Options.Create(new StorageOptions { Path = "test_storage" });

            _testUserId = Guid.NewGuid();
            _testUser = new User { Id = _testUserId, Username = "testuser", FullName = "Test User", PasswordHash = "test_password_hash" };

            _controller = new FilesController(_context, storageOptions);

            // auth user imitation
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaims }
            };
        }

        // seed data
        private async Task SeedDatabase()
        {
            _context.Users.Add(_testUser);

            _context.Files.AddRange(
                new FileMetaData { Id = Guid.NewGuid(), FileName = "c_program.c", FileType = ".c", StoragePath = "test/c_program.c", UploaderId = _testUserId, LastEditorId = _testUserId, Uploader = _testUser, LastEditor = _testUser },
                new FileMetaData { Id = Guid.NewGuid(), FileName = "b_image.jpg", FileType = ".jpg", StoragePath = "test/b_image.jpg", UploaderId = _testUserId, LastEditorId = _testUserId, Uploader = _testUser, LastEditor = _testUser },
                new FileMetaData { Id = Guid.NewGuid(), FileName = "a_script.cpp", FileType = ".cpp", StoragePath = "test/a_script.cpp", UploaderId = _testUserId, LastEditorId = _testUserId, Uploader = _testUser, LastEditor = _testUser },
                new FileMetaData { Id = Guid.NewGuid(), FileName = "d_logo.png", FileType = ".png", StoragePath = "test/d_logo.png", UploaderId = _testUserId, LastEditorId = _testUserId, Uploader = _testUser, LastEditor = _testUser }
            );
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetFiles_SortsByFileNameDescending_ReturnsCorrectlyOrderedList()
        {
            // Arrange
            await SeedDatabase();

            // Act
            var result = await _controller.GetFiles(sortBy: "filename", order: "desc", filter: "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var files = Assert.IsAssignableFrom<IEnumerable<FileDetailDto>>(okResult.Value);
            var fileList = files.ToList();

            Assert.Equal(4, fileList.Count);
            Assert.Equal("d_logo.png", fileList[0].FileName);
            Assert.Equal("c_program.c", fileList[1].FileName);
            Assert.Equal("b_image.jpg", fileList[2].FileName);
            Assert.Equal("a_script.cpp", fileList[3].FileName);
        }

        [Fact]
        public async Task GetFiles_FiltersByFileTypeCpp_ReturnsOnlyCppFiles()
        {
            // Arrange
            await SeedDatabase();

            // Act
            var result = await _controller.GetFiles(sortBy: "filename", order: "asc", filter: "cpp");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var files = Assert.IsAssignableFrom<IEnumerable<FileDetailDto>>(okResult.Value);
            var fileList = files.ToList();

            Assert.Single(fileList);
            Assert.Equal("a_script.cpp", fileList[0].FileName);
        }

        [Fact]
        public async Task GetFileContent_ForAllowedFileType_ReturnsFileContentResult()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var fileName = "main.c";
            var tempFilePath = Path.Combine(Path.GetTempPath(), fileName); 
            var fileContent = "#include <stdio.h>";
            await File.WriteAllTextAsync(tempFilePath, fileContent);

            _context.Users.Add(_testUser);
            _context.Files.Add(new FileMetaData
            {
                Id = fileId,
                FileName = fileName,
                FileType = ".c",
                StoragePath = tempFilePath, 
                UploaderId = _testUserId,
                LastEditorId = _testUserId
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetFileContent(fileId);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);

            // Cleanup
            File.Delete(tempFilePath); 
        }
    }
}
