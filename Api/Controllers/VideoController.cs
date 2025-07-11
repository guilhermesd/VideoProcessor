using Crosscutting.DTOs;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.ServicosExternos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

[ApiController]
[Route("videos")]
public class VideoController : ControllerBase
{
    private readonly IVideoRepository _videoRepository;
    private readonly IRabbitMqProducer _rabbitMqProducer;
    private readonly IWebHostEnvironment _env;
    private readonly IDistributedCache _cache;

    public VideoController(IVideoRepository videoRepository, IRabbitMqProducer rabbitMqProducer, IWebHostEnvironment env, IDistributedCache cache)
    {
        _videoRepository = videoRepository;
        _rabbitMqProducer = rabbitMqProducer;
        _env = env;
        _cache = cache;
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo inválido.");

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var uploadPath = Path.Combine(_env.ContentRootPath, "uploads");

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : Guid.NewGuid();

        var job = new VideoJob
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            Status = VideoStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await _videoRepository.AddAsync(job);
        _rabbitMqProducer.Publish("video-processing-queue", JsonSerializer.Serialize(job));

        return Ok(new { job.Id, job.Status });
    }

    [HttpGet("download/{fileName}")]
    public IActionResult Download(string fileName)
    {
        string uploadsDir = "/app/uploads";
        string filePath = Path.Combine(_env.ContentRootPath, "uploads", fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new { message = $"Arquivo '{fileName}' não encontrado." });
        }

        const string contentType = "application/zip";
        return PhysicalFile(filePath, contentType, fileName);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> ListarVideosDoUsuario()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Usuário não autenticado corretamente.");

        var cacheKey = $"videos_user_{userId}";
        var cached = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cached))
        {
            var cachedResult = JsonSerializer.Deserialize<IEnumerable<VideoJobListDto>>(cached);
            return Ok(cachedResult);
        }

        var videos = await _videoRepository.ListarVideosPorUsuarioAsync(userId);

        var result = videos.Select(v => new VideoJobListDto
        {
            Id = v.Id,
            FileName = v.FileName,
            Status = v.Status,
            ProcessedAt = v.ProcessedAt,
            OutputFile = v.OutputFile
        });

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options);

        return Ok(result);
    }

}