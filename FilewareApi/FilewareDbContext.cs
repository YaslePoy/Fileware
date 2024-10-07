using FilewareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FilewareApi;

public class FilewareDbContext : DbContext
{
    public FilewareDbContext(DbContextOptions<FilewareDbContext> options) : base(options)
    {
    }

    public DbSet<FileData> FileData { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<HistoryPoint> HistoryPoints { get; set; }
}