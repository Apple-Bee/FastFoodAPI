using FastFoodAPI.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            try
            {
                Console.WriteLine("Registering user with email: " + model.Email);

                using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Connected to database.");

                    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    var query = "INSERT INTO users (email, password) VALUES (@Email, @Password)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);

                        await cmd.ExecuteNonQueryAsync();
                        Console.WriteLine("User registered successfully.");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return false;
            }
        }

        // Login a user and generate a JWT token
        public async Task<string> LoginAsync(LoginModel model)
        {
            try
            {
                using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    var query = "SELECT password FROM users WHERE email = @Email";
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
                                    // If password is correct, generate and return JWT token
                                    return GenerateJwtToken(model.Email);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
            }

            // Return null if login fails
            return null;
        }

        // Validate a password against a hashed password
        public bool ValidatePassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        // Generate JWT token
        public string GenerateJwtToken(string email)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Retrieve order history for a user
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

        // Update user profile
        public async Task<bool> UpdateUserProfileAsync(string email, UpdateProfileModel model)
        {
            try
            {
                using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    var query = "UPDATE users SET full_name = @FullName, email = @NewEmail WHERE email = @Email";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FullName", model.FullName);
                        cmd.Parameters.AddWithValue("@NewEmail", model.NewEmail);
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

        // Set dark mode preference for a user
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
                        cmd.Parameters.AddWithValue("@DarkMode", darkMode);
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
        public async Task<bool> GetDarkModePreferenceAsync(string email)
        {
            using (var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                var query = "SELECT dark_mode FROM users WHERE email = @Email";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToBoolean(result);
                }
            }
        }

    }
}
