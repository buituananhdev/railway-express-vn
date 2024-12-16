using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using System.Linq.Expressions;

namespace Admin.Application.Services.StationService
{
    public class StationService : IStationService
    {
        private readonly IAdminUnitOfWork _adminUnitOfWork;
        private readonly IMapper _mapper;

        public StationService(IAdminUnitOfWork adminUnitOfWork, IMapper mapper)
        {
            _adminUnitOfWork = adminUnitOfWork;
            _mapper = mapper;
        }

        public async Task AddStationAsync(AddStationDto addStationDto)
        {
            var station = _mapper.Map<Station>(addStationDto);
            await _adminUnitOfWork.StationRepository.AddAsync(station);
            await _adminUnitOfWork.SaveChangesAsync();
        }

        public async Task<List<StationDto>> GetStations()
        {
            var stations = await _adminUnitOfWork.StationRepository
                .ToListAsync(includes: new List<Expression<Func<Station, object>>> { station => station.TrainAtStation! });

            return _mapper.Map<List<StationDto>>(stations);
        }
    }
}
