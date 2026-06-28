using BettingSite.Application.DTOs;

namespace BettingSite.Application.Abstractions;

public interface IUserRepository
{
    Task<bool> SaveAllAsync();
    Task<IReadOnlyList<MemberDto>> GetMembersAsync();
    Task<MemberDto?> GetMemberAsync(string username);
}
