using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Infrastructure.Repositories;

namespace Admin.Infrastructure.Repositories
{
    public class TrainRepository : BaseRepository<Train>, ITrainRepository
    {
        public TrainRepository(AdminContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
