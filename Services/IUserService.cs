using FastFoodAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FastFoodAPI.Services
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(RegisterModel model);
        Task<string> LoginAsync(LoginModel model);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> SetDarkModePreferenceAsync(string email, bool darkMode);
        Task<bool> UpdateUserProfileAsync(string email, UpdateProfileModel model);
        Task<bool?> GetDarkModePreferenceAsync(string email);
        Task<List<Order>> GetOrderHistoryAsync(string email);
        bool ValidatePassword(string password, string hashedPassword);
        string GenerateJwtToken(User user); // Ensure this method is in the interface
    }
}

