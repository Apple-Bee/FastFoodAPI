﻿namespace FastFoodAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool DarkMode { get; set; }
        public string Role { get; set; }  // Admin or Customer
    }
}
