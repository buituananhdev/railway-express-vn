using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;

namespace Admin.Application.Services.TrainService
{
    public class TrainService : ITrainService
    {
        private readonly IAdminUnitOfWork _adminUnitOfWork;
        private readonly IMapper _mapper;

        public TrainService(IAdminUnitOfWork adminUnitOfWork, IMapper mapper)
        {
            _adminUnitOfWork = adminUnitOfWork;
            _mapper = mapper;
        }
        public async Task AddTrainAsync(AddTrainDto trainDto)
        {
            try
            {
                var train = _mapper.Map<Train>(trainDto);

                await _adminUnitOfWork.TrainRepository.AddAsync(train);
                await _adminUnitOfWork.SaveChangesAsync();
            } catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<TrainDto>> GetTrainsAsync()
        {
            try
            {
                var trains = await _adminUnitOfWork.TrainRepository.ToListAsync();
                return _mapper.Map<List<TrainDto>>(trains);
            } catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddTrainCarAsync(AddTrainCarDto trainCarDto)
        {
            try
            {
                var tranCar = _mapper.Map<TrainCar>(trainCarDto);
                await _adminUnitOfWork.TrainCarRepository.AddAsync(tranCar);
                await _adminUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}