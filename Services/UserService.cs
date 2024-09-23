using FastFoodAPI.Models;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;

        public UserService(IConfiguration config)
        {
            _config = config;
        }

        // Register a new user
        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                var query = "INSERT INTO users (email, password) VALUES (@Email, @Password)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Password", hashedPassword);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return true;
        }

        // Login a user and generate a JWT token
        public async Task<string> LoginAsync(LoginModel model)
        {
            try
            {
                using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    var query = "SELECT id, email, password, full_name, dark_mode FROM users WHERE email = @Email";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var storedPassword = reader["password"].ToString();

                                // Verify the password
                                if (BCrypt.Net.BCrypt.Verify(model.Password, storedPassword))
                                {
                                    var user = new User
                                    {
                                        Id = Convert.ToInt32(reader["id"]),
                                        Email = reader["email"].ToString(),
                                        FullName = reader["full_name"].ToString(),
                                        DarkMode = Convert.ToBoolean(reader["dark_mode"]) // Convert tinyint to boolean
                                    };

                                    // Generate and return JWT token or response
                                    return GenerateJwtToken(user);
                                }
                                else
                                {
                                    return "Invalid password.";
                                }
                            }
                            else
                            {
                                return "Invalid email.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return $"An error occurred during login: {ex.Message}";
            }
        }


        // Generate JWT token
        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Email),
    new Claim(ClaimTypes.Email, user.Email), // Add email claim here
    new Claim("FullName", user.FullName),
    new Claim("UserId", user.Id.ToString())
};


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Get user by email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var query = "SELECT id, email, full_name, dark_mode FROM users WHERE email = @Email";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Email = reader["email"].ToString(),
                                FullName = reader["full_name"].ToString(),
                                DarkMode = reader["dark_mode"] != DBNull.Value && Convert.ToBoolean(reader["dark_mode"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Set dark mode preference
        public async Task<bool> SetDarkModePreferenceAsync(string email, bool darkMode)
        {
            try
            {
                using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    var query = "UPDATE users SET dark_mode = @DarkMode WHERE email = @Email";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        // Convert the boolean value to tinyint (0 or 1)
                        cmd.Parameters.AddWithValue("@DarkMode", darkMode ? 1 : 0);
                        cmd.Parameters.AddWithValue("@Email", email);

                        var rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting dark mode preference: {ex.Message}");
                return false;
            }
        }


        // Update user profile
        public async Task<bool> UpdateUserProfileAsync(string email, UpdateProfileModel model)
        {
            try
            {
                using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Handle null or empty strings
                    var newFullName = string.IsNullOrEmpty(model.FullName) ? email : model.FullName;
                    var newEmail = string.IsNullOrEmpty(model.NewEmail) ? email : model.NewEmail;

                    var updateQuery = "UPDATE users SET full_name = @FullName, email = @NewEmail, dark_mode = @DarkMode WHERE email = @Email";
                    using (var cmd = new MySqlCommand(updateQuery, connection))
                    {
                        // Add parameters ensuring that null or empty strings are handled
                        cmd.Parameters.AddWithValue("@FullName", newFullName);
                        cmd.Parameters.AddWithValue("@NewEmail", newEmail);
                        cmd.Parameters.AddWithValue("@DarkMode", model.DarkMode ? 1 : 0);  // Convert boolean to tinyint
                        cmd.Parameters.AddWithValue("@Email", email);

                        var rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user profile: {ex.Message}");
                return false;
            }
        }







        // Get dark mode preference
        public async Task<bool?> GetDarkModePreferenceAsync(string email)
        {
            using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var query = "SELECT dark_mode FROM users WHERE email = @Email";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        return null;
                    }
                    return Convert.ToBoolean(result);
                }
            }
        }

        // Get order history
        public async Task<List<Order>> GetOrderHistoryAsync(string email)
        {
            var orders = new List<Order>();
            using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM orders WHERE user_id = (SELECT id FROM users WHERE email = @Email)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                OrderDate = Convert.ToDateTime(reader["order_date"]),
                                TotalAmount = Convert.ToDecimal(reader["total_amount"]),
                                Status = reader["status"].ToString()
                            });
                        }
                    }
                }
            }
            return orders;
        }

        // Validate password
        public bool ValidatePassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
