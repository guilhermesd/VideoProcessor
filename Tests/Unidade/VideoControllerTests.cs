using Crosscutting.DTOs;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Security.Claims;
using System.Text;

namespace Tests.Unidade
{
    public class VideoControllerTests
    {
        private readonly Mock<IVideoRepository> _videoRepoMock = new();
        private readonly Mock<IRabbitMqProducer> _rabbitMqMock = new();
        private readonly Mock<IWebHostEnvironment> _envMock = new();
        private readonly Mock<IDistributedCache> _cacheMock = new();

        private VideoController CreateControllerWithUser(Guid userId)
        {
            var controller = new VideoController(_videoRepoMock.Object, _rabbitMqMock.Object, _envMock.Object, _cacheMock.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task Upload_DeveRetornarOkComJobId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var controller = CreateControllerWithUser(userId);

            var content = "fake video content";
            var fileMock = new FormFile(
                baseStream: new MemoryStream(Encoding.UTF8.GetBytes(content)),
                baseStreamOffset: 0,
                length: content.Length,
                name: "file",
                fileName: "video.mp4")
            {
                Headers = new HeaderDictionary(),
                ContentType = "video/mp4"
            };

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            _envMock.Setup(e => e.ContentRootPath).Returns(tempDir);

            // Act
            var result = await controller.Upload(fileMock);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var value = ok.Value?.ToString();
            Assert.Contains("Status", value);
            Directory.Delete(tempDir, true); // Cleanup
        }

        [Fact]
        public void Download_ArquivoExiste_DeveRetornarPhysicalFile()
        {
            // Arrange
            var fileName = "output.zip";
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var uploadsDir = Path.Combine(tempDir, "uploads");
            Directory.CreateDirectory(uploadsDir); // Corrigido

            var filePath = Path.Combine(uploadsDir, fileName);
            File.WriteAllText(filePath, "dummy zip");

            _envMock.Setup(e => e.ContentRootPath).Returns(tempDir);
            var controller = new VideoController(_videoRepoMock.Object, _rabbitMqMock.Object, _envMock.Object, _cacheMock.Object);

            // Act
            var result = controller.Download(fileName);

            // Assert
            var fileResult = Assert.IsType<PhysicalFileResult>(result);
            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.Equal(fileName, fileResult.FileDownloadName);

            Directory.Delete(tempDir, true); // Cleanup
        }

        [Fact]
        public async Task ListarVideosDoUsuario_DeveRetornarVideos()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var controller = CreateControllerWithUser(userId);

            _videoRepoMock.Setup(repo => repo.ListarVideosPorUsuarioAsync(userId)).ReturnsAsync(new List<VideoJob>
            {
                new VideoJob
                {
                    Id = Guid.NewGuid(),
                    FileName = "video1.mp4",
                    Status = "COMPLETED",
                    OutputFile = "video1.zip",
                    ProcessedAt = DateTime.UtcNow
                }
            });

            // Act
            var result = await controller.ListarVideosDoUsuario();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<VideoJobListDto>>(ok.Value);
            Assert.Single(list);
        }
    }
}
