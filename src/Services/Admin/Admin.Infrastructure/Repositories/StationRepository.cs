using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories;
public class StationRepository : BaseRepository<Station>, IStationRepository
{
    public StationRepository(AdminContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
