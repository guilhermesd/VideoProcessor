using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Repositories
{
    public interface IVideoRepository
    {
        Task AddAsync(VideoJob video);
        Task<IEnumerable<VideoJob>> GetPendingAsync();
        Task<VideoJob?> GetByIdAsync(Guid id);
        Task UpdateAsync(VideoJob job);
        Task<IEnumerable<VideoJob>> ListarVideosPorUsuarioAsync(Guid userId);
    }
}
