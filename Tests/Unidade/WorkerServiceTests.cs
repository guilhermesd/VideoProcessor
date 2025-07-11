using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Worker;

namespace Tests.Unidade
{
    public class WorkerServiceTests
    {
        [Fact]
        public async Task Deve_Processar_Video_Com_Sucesso_E_Enviar_Email()
        {
            // Arrange
            var videoJob = new VideoJob
            {
                Id = Guid.NewGuid(),
                FileName = "teste.mp4",
                UserId = Guid.NewGuid(),
                Status = VideoStatus.Pending
            };

            var message = JsonSerializer.Serialize(videoJob);

            var consumerMock = new Mock<IRabbitMqConsumer>();
            var videoRepoMock = new Mock<IVideoRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var notificationMock = new Mock<INotificationService>();
            var logger = new LoggerFactory().CreateLogger<WorkerService>();

            var user = new Domain.Entities.User
            {
                Id = videoJob.UserId,
                Email = "teste@email.com"
            };

            userRepoMock.Setup(r => r.GetByIdAsync(videoJob.UserId)).ReturnsAsync(user);
            videoRepoMock.Setup(r => r.UpdateAsync(It.IsAny<VideoJob>())).Returns(Task.CompletedTask);
            notificationMock.Setup(n => n.NotifyUserAsync(user.Email, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var service = new WorkerService(
                consumerMock.Object,
                videoRepoMock.Object,
                userRepoMock.Object,
                notificationMock.Object,
                logger
            );

            Func<string, Task> handler = null!;
            consumerMock.Setup(c => c.Consume("video-processing-queue", It.IsAny<Func<string, Task>>()))
                        .Callback<string, Func<string, Task>>((_, f) => handler = f);

            // Act
            await service.StartAsync(default); // Isso inicializa o Consumer
            await handler(message); // Simula recebimento da mensagem

            // Assert
            videoRepoMock.Verify(r => r.UpdateAsync(It.IsAny<VideoJob>()), Times.Once);
            notificationMock.Verify(n => n.NotifyUserAsync(user.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
