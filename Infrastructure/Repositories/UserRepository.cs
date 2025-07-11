using Domain.Entities;
using Domain.Interfaces.Repositories;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IPagamentoContext _context;

        public UserRepository(IPagamentoContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _context.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            return await _context.Users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.Find(_ => true).ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _context.Users.ReplaceOneAsync(filter, user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            await _context.Users.DeleteOneAsync(filter);
        }
    }
}
