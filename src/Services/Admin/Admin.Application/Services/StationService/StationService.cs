using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Application.Interfaces;
using Common.Application.Pagination;
using Common.Application.Services;
using System.Linq.Expressions;

namespace Admin.Application.Services;
public class StationService : BaseService<Station, AddStationDto, AddStationDto, StationDto>, IStationService
{
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public StationService(
        IStationRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        ICacheService cacheService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _adminUnitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<StationDto> GetStationByNameAsync(string stationName)
    {
        var specification = new StationNameSpecification(stationName);
        return await _adminUnitOfWork.StationRepository.FirstOrDefaultAsync<StationDto>(spec: specification);
    }

    public async Task<List<StationDto>> GetStations()
    {
        const string cacheKey = "station:all";
        var cached = await _cacheService.GetCacheAsync<List<StationDto>>(cacheKey);
        if (cached is not null)
            return cached;

        Func<IQueryable<Station>, IOrderedQueryable<Station>> orderBy = query =>
            query.OrderBy(station => station.StationOrder);

        var includes = new List<Expression<Func<Station, object>>> { station => station.TrainAtStation! };
        var stations = await _adminUnitOfWork.StationRepository
            .ToListAsync(includes: includes, orderBy: orderBy);

        var mapped = _mapper.Map<List<StationDto>>(stations);
        await _cacheService.SetCacheAsync(cacheKey, mapped, TimeSpan.FromDays(30));

        return mapped;
    }
}
