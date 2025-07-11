using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using Infrastructure.MessageBus;
using Infrastructure.Repositories;
using Infrastructure.ServicosExternos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;

namespace Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<RabbitMqConsumer>();
                    services.AddScoped<IPagamentoContext, PagamentoContext>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<INotificationService, EmailNotificationServiceFake>();
                    services.AddScoped<IVideoRepository, VideoRepository>();
                    services.AddScoped<IRabbitMqConsumer, RabbitMqConsumer>();
                    services.AddScoped<WorkerService>();
                    services.AddHostedService(sp => sp.GetRequiredService<WorkerService>());
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }

    public class WorkerService : BackgroundService
    {
        private readonly IRabbitMqConsumer _consumer;
        private readonly IVideoRepository _videoRepo;
        private readonly IUserRepository _userRepo;
        private readonly INotificationService _notification;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(
            IRabbitMqConsumer consumer,
            IVideoRepository videoRepo,
            IUserRepository userRepo,
            INotificationService notification,
            ILogger<WorkerService> logger)
        {
            _consumer = consumer;
            _videoRepo = videoRepo;
            _userRepo = userRepo;
            _notification = notification;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Consume("video-processing-queue", async message =>
            {
                VideoJob? job = null;
                try
                {
                    job = JsonSerializer.Deserialize<VideoJob>(message);
                    if (job == null) return;

                    _logger.LogInformation("[🎞️] Processando vídeo: {FileName}", job.FileName);

                    string uploadsDir = "/app/uploads";
                    string videoPath = Path.Combine(uploadsDir, job.FileName);
                    string framesDir = Path.Combine(uploadsDir, Path.GetFileNameWithoutExtension(job.FileName) + "_frames");
                    string zipPath = Path.Combine(uploadsDir, Path.GetFileNameWithoutExtension(job.FileName) + ".zip");

                    Directory.CreateDirectory(framesDir);

                    var ffmpeg = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{videoPath}\" \"{Path.Combine(framesDir, "frame_%03d.png")}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    var process = Process.Start(ffmpeg);
                    string output = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    _logger.LogInformation("[🖼️] Frames extraídos para: {FramesDir}", framesDir);

                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    ZipFile.CreateFromDirectory(framesDir, zipPath);

                    _logger.LogInformation("[📦] Zip gerado em: {ZipPath}", zipPath);

                    job.Status = VideoStatus.Completed;
                    job.ProcessedAt = DateTime.UtcNow;
                    job.OutputFile = Path.GetFileName(zipPath);

                    await _videoRepo.UpdateAsync(job);

                    CleanupTempFiles(videoPath, framesDir);

                    var user = await _userRepo.GetByIdAsync(job.UserId);
                    if (user != null)
                        await _notification.NotifyUserAsync(user.Email, $"Processamento de video {job.FileName}", $"Seu vídeo foi processado com sucesso! {job.FileName}");

                    _logger.LogInformation("[✅] Vídeo processado com sucesso: {FileName}", job.FileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[❌] Erro ao processar vídeo: {JobId}", job?.Id);

                    if (job != null)
                    {
                        job.Status = VideoStatus.Failed;
                        await _videoRepo.UpdateAsync(job);

                        var user = await _userRepo.GetByIdAsync(job.UserId);
                        if (user != null)
                            await _notification.NotifyUserAsync(user.Email, $"Erro - Processamento de video {job.FileName}", $"Houve um erro ao processar seu vídeo. Tente novamente mais tarde.{job.FileName}");
                    }
                }
            });

            return Task.CompletedTask;
        }

        private void CleanupTempFiles(string videoPath, string framesDir)
        {
            try
            {
                if (File.Exists(videoPath)) File.Delete(videoPath);
                if (Directory.Exists(framesDir)) Directory.Delete(framesDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[⚠️] Falha ao limpar arquivos temporários: {Message}", ex.Message);
            }
        }
    }
}
