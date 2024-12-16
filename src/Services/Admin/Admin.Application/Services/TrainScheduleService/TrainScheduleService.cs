using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using System.Linq.Expressions;

namespace Admin.Application.Services.TrainScheduleService
{
    public class TrainScheduleService : ITrainScheduleService
    {
        private readonly IAdminUnitOfWork _adminUnitOfWork;
        private readonly IMapper _mapper;

        public TrainScheduleService(IAdminUnitOfWork adminUnitOfWork, IMapper mapper)
        {
            _adminUnitOfWork = adminUnitOfWork;
            _mapper = mapper;
        }

        public async Task AddTrainScheduleAsync(AddTrainScheduleDto trainScheduleDto)
        {
            var schedule = _mapper.Map<TrainSchedule>(trainScheduleDto);
            await _adminUnitOfWork.TrainScheduleRepository.AddAsync(schedule);
            await _adminUnitOfWork.SaveChangesAsync();
        }

        public async Task<List<TrainScheduleDto>> GetTrainSchedulesAsync()
        {
            var schedules = await _adminUnitOfWork.TrainScheduleRepository
                .ToListAsync(includes: new List<Expression<Func<TrainSchedule, object>>> { schedule => schedule.DepartureStation!, schedule => schedule.ArrivalStation! });

            return _mapper.Map<List<TrainScheduleDto>>(schedules);
        }
    }
}
