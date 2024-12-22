using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Domain.Specifications;
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

        public async Task<List<TrainScheduleDto>> GetTrainSchedulesAsync(Guid fromStation, Guid toStation)
        {
            var includes = new List<Expression<Func<TrainSchedule, object>>>
                {
                    schedule => schedule.DepartureStation!,
                    schedule => schedule.ArrivalStation!
                };

            var specification = new AndSpecification<TrainSchedule>(
                new DepartureStationIdSpecification(fromStation),
                new ArrivalStationIdSpecification(toStation)
            );

            var schedules = await _adminUnitOfWork.TrainScheduleRepository
                .ToListAsync(
                    spec: specification,
                    includes: includes
                );

            return _mapper.Map<List<TrainScheduleDto>>(schedules);
        }
    }
}
