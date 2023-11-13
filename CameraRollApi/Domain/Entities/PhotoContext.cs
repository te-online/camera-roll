using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public class PhotoContext : DbContext
{
    public PhotoContext(DbContextOptions<PhotoContext> options)
        : base(options)
    {
    }

    public DbSet<Photo> Photos { get; set; } = null!;
}
