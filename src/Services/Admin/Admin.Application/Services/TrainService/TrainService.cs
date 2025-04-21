using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using AutoMapper;
using Common.Application.Dtos;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Admin.Application.Services;
public class TrainService : BaseService<Train, AddTrainDto, AddTrainDto, TrainDto>, ITrainService
{
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IMapper _mapper;

    public TrainService(
        ITrainRepository repository,
        IAdminUnitOfWork adminUnitOfWork,
        IMapper mapper,
        IPaginationService paginationService
        ) : base(repository, adminUnitOfWork, mapper, paginationService)
    {
        _adminUnitOfWork = adminUnitOfWork;
        _mapper = mapper;
    }

    public async override Task<TrainDto> CreateAsync(AddTrainDto trainDto)
    {
        try
        {
            var train = _mapper.Map<Train>(trainDto);
            _adminUnitOfWork.BeginTransaction();
            await _adminUnitOfWork.TrainRepository.AddAsync(train);
            await _adminUnitOfWork.SaveChangesAsync();
            await _adminUnitOfWork.CommitAsync();
            _adminUnitOfWork.Dispose();
            return _mapper.Map<TrainDto>(train);
        }
        catch (Exception ex)
        {
            _adminUnitOfWork.Rollback();
            _adminUnitOfWork.Dispose();
            throw;
        }
    }

    public async override Task<TrainDto> UpdateAsync(Guid id, AddTrainDto trainDto)
    {
        try
        {
            var train = await _adminUnitOfWork.TrainRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Train not found");
            _mapper.Map(trainDto, train);
            await _adminUnitOfWork.SaveChangesAsync();
            return _mapper.Map<TrainDto>(train);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async override Task DeleteAsync(Guid id)
    {
        try
        {
            var train = await _adminUnitOfWork.TrainRepository.GetByIdAsync(id)
                    ?? throw new NotFoundException("Train not found");
            _adminUnitOfWork.BeginTransaction();
            _adminUnitOfWork.TrainRepository.Delete(train);
            await _adminUnitOfWork.SaveChangesAsync();
            await _adminUnitOfWork.CommitAsync();
            _adminUnitOfWork.Dispose();
        }
        catch (Exception ex)
        {
            _adminUnitOfWork.Rollback();
            _adminUnitOfWork.Dispose();
            throw;
        }
    }

    public async override Task<PaginationResult<TrainDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Train>? specification = null,
        List<Expression<Func<Train, object>>>? includes = null)
    {
        try
        {
            includes ??= new List<Expression<Func<Train, object>>>
            {
                train => train.TrainCars!
            };
            var query = _adminUnitOfWork
                .TrainRepository
                .GetQueryable(specification, includes);
            var paginatedResult = await _paginationService
                .CreatePaginatedResultAsync(query, paginationParams);
            return new PaginationResult<TrainDto>
            {
                Data = _mapper.Map<List<TrainDto>>(paginatedResult.Data),
                MetaData = paginatedResult.MetaData
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<TrainDto>> GetTrainsAsync()
    {
        var trains = await _adminUnitOfWork.TrainRepository
            .ToListAsync(includes: new List<Expression<Func<Train, object>>> { train => train.TrainCars! });
        return _mapper.Map<List<TrainDto>>(trains);
    }


    public async Task<TrainCarDto> CreateTrainCarAsync(AddTrainCarDto trainCarDto)
    {
        try
        {
            var trainCar = _mapper.Map<TrainCar>(trainCarDto);
            _adminUnitOfWork.BeginTransaction();
            await _adminUnitOfWork.TrainCarRepository.AddAsync(trainCar);
            await _adminUnitOfWork.SaveChangesAsync();
            await _adminUnitOfWork.CommitAsync();
            _adminUnitOfWork.Dispose();
            return _mapper.Map<TrainCarDto>(trainCar);
        }
        catch (Exception ex)
        {
            _adminUnitOfWork.Rollback();
            _adminUnitOfWork.Dispose();
            throw;
        }
    }

    public async Task<TrainCarDto> UpdateTrainCarAsync(Guid id, AddTrainCarDto trainCarDto)
    {
        try
        {
            var trainCar = await _adminUnitOfWork.TrainCarRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Train car not found");
            _mapper.Map(trainCarDto, trainCar);
            _adminUnitOfWork.BeginTransaction();
            _adminUnitOfWork.TrainCarRepository.Update(trainCar);
            await _adminUnitOfWork.SaveChangesAsync();
            await _adminUnitOfWork.CommitAsync();
            _adminUnitOfWork.Dispose();
            return _mapper.Map<TrainCarDto>(trainCar);
        }
        catch (Exception ex)
        {
            _adminUnitOfWork.Rollback();
            _adminUnitOfWork.Dispose();
            throw;
        }
    }

    public async Task DeleteTrainCarAsync(Guid id)
    {
        try
        {
            var trainCar = await _adminUnitOfWork.TrainCarRepository.GetByIdAsync(id)
                    ?? throw new NotFoundException("Train car not found");
            _adminUnitOfWork.BeginTransaction();
            _adminUnitOfWork.TrainCarRepository.Delete(trainCar);
            await _adminUnitOfWork.SaveChangesAsync();
            await _adminUnitOfWork.CommitAsync();
            _adminUnitOfWork.Dispose();
        }
        catch (Exception ex)
        {
            _adminUnitOfWork.Rollback();
            _adminUnitOfWork.Dispose();
            throw;
        }
    }
}
