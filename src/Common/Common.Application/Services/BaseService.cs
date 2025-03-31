using System.Linq.Expressions;
using AutoMapper;
using Common.Application.Dtos;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Application.Repositories;
using Common.Domain.Specifications;

namespace Common.Application.Services;

public abstract class BaseService<TEntity, TCreateDto, TUpdateDto, TReadDto>
    where TEntity : class
    where TCreateDto : class
    where TUpdateDto : class
    where TReadDto : class
{
    protected readonly IBaseRepository<TEntity> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;
    protected readonly IPaginationService _paginationService;

    protected BaseService(
        IBaseRepository<TEntity> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paginationService = paginationService;
    }

    public virtual async Task<TReadDto> CreateAsync(TCreateDto createDto)
    {
        try
        {
            var entity = _mapper.Map<TEntity>(createDto);
            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<TReadDto>(entity);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public virtual async Task<TReadDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Entity with id {id} not found");

        return _mapper.Map<TReadDto>(entity);
    }

    public virtual async Task<PaginationResult<TReadDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<TEntity>? specification = null,
        List<Expression<Func<TEntity, object>>>? includes = null)
    {
        var query = _repository.GetQueryable(specification, includes);
        var paginatedResult = await _paginationService
            .CreatePaginatedResultAsync(query, paginationParams);

        return new PaginationResult<TReadDto>
        {
            Data = _mapper.Map<List<TReadDto>>(paginatedResult.Data),
            MetaData = paginatedResult.MetaData
        };
    }

    public virtual async Task<TReadDto> UpdateAsync(Guid id, TUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Entity with id {id} not found");

        _mapper.Map(updateDto, entity);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<TReadDto>(entity);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Entity with id {id} not found");

        _repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}

