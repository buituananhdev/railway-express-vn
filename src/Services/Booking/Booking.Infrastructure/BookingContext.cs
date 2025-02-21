using Booking.Domain.Entities;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure;
public partial class BookingContext : DbContext, IDataContext
{
    public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }
    DbSet<T> IDataContext.Set<T>() => Set<T>();

    public virtual DbSet<Ticket> Tickets { get; set; }
}
