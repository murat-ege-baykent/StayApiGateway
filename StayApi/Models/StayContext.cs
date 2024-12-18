using Microsoft.EntityFrameworkCore;

namespace StayApi.Models;

public class StayContext : DbContext
{
    public StayContext(DbContextOptions<StayContext> options)
        : base(options)
    {
    }

    public DbSet<StayItem> StayItems { get; set; } = null!;
    public DbSet<BookingItem> BookingItems { get; set; } = null!;
}