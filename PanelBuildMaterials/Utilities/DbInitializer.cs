﻿using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using PanelBuildMaterials.Models;
using BCrypt.Net;

namespace PanelBuildMaterials.Utilities
{
    public class DbInitializer
    {
        public static void Initialize(PanelDbContext context)
        {

            //Начальная инициализация пользователя-администратора
            if (!context.Users.Any(u => u.UserLogin == "admin"))
            {
                var admin = new User
                {
                    UserLogin = "admin",
                    UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                    UserLaws = "Полные"
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
