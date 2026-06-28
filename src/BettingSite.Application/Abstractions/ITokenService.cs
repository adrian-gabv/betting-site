namespace BettingSite.Application.Abstractions;

public interface ITokenService
{
    Task<string> CreateToken(int userId, string username, IList<string> roles);
}
