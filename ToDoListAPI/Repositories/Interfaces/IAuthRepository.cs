using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public interface IAuthRepository {
    Task<bool> IsEmailUniqueAsync(string userEmail);
    void SaveRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}