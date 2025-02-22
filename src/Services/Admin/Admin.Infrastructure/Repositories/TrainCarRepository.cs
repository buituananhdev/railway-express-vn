using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories;
public class TrainCarRepository : BaseRepository<TrainCar>, ITrainCarRepository
{
    public TrainCarRepository(AdminContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
