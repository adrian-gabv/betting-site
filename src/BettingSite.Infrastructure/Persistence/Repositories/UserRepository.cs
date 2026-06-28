using AutoMapper;
using AutoMapper.QueryableExtensions;
using BettingSite.Application.Abstractions;
using BettingSite.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BettingSite.Infrastructure.Persistence.Repositories
{
    public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
    {
        public async Task<MemberDto?> GetMemberAsync(string username) =>
             await context.Users
                .Where(u => u.UserName == username)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

        public async Task<IReadOnlyList<MemberDto>> GetMembersAsync() =>
            await context.Users
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .ToListAsync();

        public async Task<bool> SaveAllAsync() => await context.SaveChangesAsync() > 0;
    }
}
