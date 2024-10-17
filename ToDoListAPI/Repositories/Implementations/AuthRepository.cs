using ToDoListAPI.Data;
using ToDoListAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ToDoListAPI.Migrations;

namespace ToDoListAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ToDoListContext _context;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(ToDoListContext context, ILogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsEmailUniqueAsync(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Attempted to check email uniqueness with a null or empty email.");
                throw new ArgumentException("Email cannot be null or empty.", nameof(userEmail));
            }

            try
            {
                return !await _context.Users.AnyAsync(u => u.Email == userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email {userEmail} is unique.", userEmail);
                throw new Exception("Error checking email uniqueness.", ex);
            }
        }

        public async void SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            try
            {
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while adding refresh token {refreshToken}.", refreshToken);
                throw new Exception("Database error occurred while adding refresh token.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding refresh token {refreshToken}.", refreshToken);
                throw new Exception("An unexpected error occurred while adding refresh token.", ex);
            }
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            try
            {
                return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token && !rt.Revoked);
            }
            catch (Exception ex){
                _logger.LogError(ex, $"Error fetching refresh token {token}.", token);
                throw new Exception("Error fetching refresh token.", ex);
            }
        }

        public async Task RevokeRefreshTokenAsync(string token){
            try
            {
                var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
                if (refreshToken != null)
                {
                    refreshToken.Revoked = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while revoking refresh token: {Token}", token);
                throw new Exception("Database error occurred while updating refresh token.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while revoking refresh token: {Token}", token);
                throw new Exception("An unexpected error occurred while revoking refresh token.", ex);
            }
        }


    }
}



    
