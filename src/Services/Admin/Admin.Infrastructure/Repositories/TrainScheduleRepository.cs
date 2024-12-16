using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories
{
    public class TrainScheduleRepository : BaseRepository<TrainSchedule>, ITrainScheduleRepository
    {
        public TrainScheduleRepository(AdminContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
