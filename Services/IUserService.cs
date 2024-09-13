using FastFoodAPI.Models;

namespace FastFoodAPI.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterModel model);
        Task<string> LoginAsync(LoginModel model);
        bool ValidatePassword(string password, string hashedPassword);
        string GenerateJwtToken(string email);
    }
}
