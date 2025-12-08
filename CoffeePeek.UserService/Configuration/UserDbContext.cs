using CoffeePeek.UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeePeek.UserService.Configuration;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}