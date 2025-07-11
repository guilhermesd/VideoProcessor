using Domain.Entities;
using Domain.Interfaces.Repositories;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly IPagamentoContext _context;

        public VideoRepository(IPagamentoContext context)
        {
            _context = context;
        }

        public async Task AddAsync(VideoJob video)
        {
            await _context.VideoJobs.InsertOneAsync(video);
        }

        public async Task<IEnumerable<VideoJob>> GetPendingAsync()
        {
            return await _context.VideoJobs
                .Find(v => v.Status == "PENDING")
                .ToListAsync();
        }

        public async Task<VideoJob?> GetByIdAsync(Guid id)
        {
            return await _context.VideoJobs
                .Find(v => v.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(VideoJob job)
        {
            var update = Builders<VideoJob>.Update
                .Set(v => v.Status, job.Status)
                .Set(v => v.ProcessedAt, job.ProcessedAt)
                .Set(v => v.OutputFile, job.OutputFile);

            await _context.VideoJobs.UpdateOneAsync(v => v.Id == job.Id, update);
        }

        public async Task<IEnumerable<VideoJob>> ListarVideosPorUsuarioAsync(Guid userId)
        {
            return await _context.VideoJobs
                .Find(v => v.UserId == userId)
                .SortByDescending(v => v.ProcessedAt)
                .ToListAsync();
        }
    }
}
