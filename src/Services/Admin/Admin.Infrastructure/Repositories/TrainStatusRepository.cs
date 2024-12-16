using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories
{
    public class TrainStatusRepository : BaseRepository<TrainStatus>, ITrainStatusRepository
    {
        public TrainStatusRepository(AdminContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
