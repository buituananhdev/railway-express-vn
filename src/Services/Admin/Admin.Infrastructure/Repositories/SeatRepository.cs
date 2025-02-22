using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories;
public class SeatRepository : BaseRepository<Seat>, ISeatRepository
{
    public SeatRepository(AdminContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
