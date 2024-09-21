using FastFoodAPI.Models;

namespace FastFoodAPI.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterModel model);
        Task<string> LoginAsync(LoginModel model);
        bool ValidatePassword(string password, string hashedPassword);
        string GenerateJwtToken(string email);
        Task<List<Order>> GetOrderHistoryAsync(string email);
        Task<bool> SetDarkModePreferenceAsync(string email, bool darkMode);
        Task<bool> UpdateUserProfileAsync(string email, UpdateProfileModel model);
        Task<bool> GetDarkModePreferenceAsync(string email);
    }
}
