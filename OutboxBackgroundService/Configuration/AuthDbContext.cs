using Microsoft.EntityFrameworkCore;
using OutboxBackgroundService.Models;

namespace OutboxBackgroundService.Configuration;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
}