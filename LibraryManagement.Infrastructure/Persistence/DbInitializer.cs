using LibraryManagement.Domain.Entities;
using LibraryManagement.Infrastructure.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Linq;

namespace LibraryManagement.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            try
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<AppUser>(new UserStore<AppUser>(context));

                // Seed Roles
                if (!roleManager.RoleExists(Roles.Admin))
                {
                    roleManager.Create(new IdentityRole(Roles.Admin));
                }

                if (!roleManager.RoleExists(Roles.User))
                {
                    roleManager.Create(new IdentityRole(Roles.User));
                }

                // Seed Admin User
                if (userManager.FindByName("admin@library.com") == null)
                {
                    var adminUser = new AppUser
                    {
                        UserName = "admin@library.com",
                        Email = "admin@library.com",
                        EmailConfirmed = true
                    };

                    var result = userManager.Create(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(adminUser.Id, Roles.Admin);
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine("Admin user creation error: " + error);
                        }
                    }
                }

                context.SaveChanges();

                // Seed Genres
                if (!context.Genres.Any())
                {
                    var genres = new[]
                    {
                        new Genre { Name = "Science Fiction" },
                        new Genre { Name = "Fantasy" },
                        new Genre { Name = "Mystery" },
                        new Genre { Name = "Thriller" },
                        new Genre { Name = "Historical Fiction" }
                    };
                    context.Genres.AddRange(genres);
                    context.SaveChanges();
                }

                // Seed Authors
                if (!context.Authors.Any())
                {
                    var authors = new[]
                    {
                        new Author { FullName = "Isaac Asimov", BirthDate = new DateTime(1920, 1, 2) },
                        new Author { FullName = "J.R.R. Tolkien", BirthDate = new DateTime(1892, 1, 3) },
                        new Author { FullName = "Agatha Christie", BirthDate = new DateTime(1890, 9, 15) }
                    };
                    context.Authors.AddRange(authors);
                    context.SaveChanges();
                }

                // Seed Books
                if (!context.Books.Any())
                {
                    var asimov = context.Authors.FirstOrDefault(a => a.FullName.Contains("Asimov"));
                    var tolkien = context.Authors.FirstOrDefault(a => a.FullName.Contains("Tolkien"));
                    var sciFi = context.Genres.FirstOrDefault(g => g.Name.Contains("Science Fiction"));
                    var fantasy = context.Genres.FirstOrDefault(g => g.Name.Contains("Fantasy"));

                    if (asimov != null && sciFi != null)
                    {
                        context.Books.Add(new Book
                        {
                            Title = "Foundation",
                            AuthorId = asimov.Id,
                            GenreId = sciFi.Id,
                            Pages = 255,
                            PublicationYear = 1951
                        });
                    }

                    if (tolkien != null && fantasy != null)
                    {
                        context.Books.Add(new Book
                        {
                            Title = "The Hobbit",
                            AuthorId = tolkien.Id,
                            GenreId = fantasy.Id,
                            Pages = 310,
                            PublicationYear = 1937
                        });
                    }

                    context.SaveChanges();
                }

                Console.WriteLine("Database seeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database seeding failed: " + ex.Message);
                throw;
            }
        }
    }
}
