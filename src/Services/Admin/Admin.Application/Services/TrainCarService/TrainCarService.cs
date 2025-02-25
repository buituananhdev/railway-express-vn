using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Application.Interfaces;
using Common.Application.Services;

namespace Admin.Application.Services;
public class TrainCarService : BaseService<TrainCar, AddTrainCarDto, AddTrainCarDto, TrainCarDto>, ITrainCarService
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public TrainCarService(
        ITrainCarRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<TrainCarDto>> GetTrainCarsByTrainIdAsync(Guid trainId)
    {
        try
        {
            var specification = new TrainIdSpecification(trainId);
            var trainCars = await _unitOfWork.TrainCarRepository.ToListAsync(specification);

            return _mapper.Map<List<TrainCarDto>>(trainCars);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
