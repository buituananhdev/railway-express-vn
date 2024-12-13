using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Application.Repositories;
using Common.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Common.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly IDataContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly IMapper _mapper;

        protected BaseRepository(IDataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = _context.Set<T>();
        }

        public virtual Task<T?> FirstOrDefaultAsync(Specification<T>? spec = null, List<Expression<Func<T, object>>>? includes = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            return _dbSet.AsNoTracking()
                .ApplySpecification(spec)
                .ApplyOrder(orderBy)
                .FirstOrDefaultAsync();
        }

        public virtual Task<TDto?> FirstOrDefaultAsync<TDto>(Specification<T>? spec = null, List<Expression<Func<T, object>>>? includes = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
        {
            return _dbSet.AsNoTracking()
                .ApplySpecification(spec)
                .ApplyOrder(orderBy)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual Task<int> CountAsync(Specification<T>? spec)
        {
            return _dbSet.AsNoTracking().ApplySpecification(spec).CountAsync();
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual Task<bool> ExistsAsync(Specification<T>? spec = null)
        {
            return _dbSet.AsNoTracking().ApplySpecification(spec).AnyAsync();
        }

        public Task<List<T>> ToListAsync(Specification<T>? spec = null, List<Expression<Func<T, object>>>? includes = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, int? page = null,
            int? size = null)
        {
            return _dbSet.AsNoTracking()
                .ApplySpecification(spec)
                .ApplyOrder(orderBy)
                .ApplyPaging(page, size)
                .ToListAsync();
        }

        public Task<List<TDto>> ToListAsync<TDto>(Specification<T>? spec = null, List<Expression<Func<T, object>>>? includes = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, int? page = null,
            int? size = null)
        {
            return _dbSet.AsNoTracking()
                .ApplySpecification(spec)
                .ApplyOrder(orderBy)
                .ApplyPaging(page, size)
                .ProjectTo<TDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public Task<bool> AnyAsync(Specification<T>? spec = null)
        {
            return _dbSet.AsNoTracking().ApplySpecification(spec).AnyAsync();
        }
    }

    internal static class RepositoryExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int? page, int? size)
        {
            if (page != null && size != null)
            {
                return query.Skip((page.Value - 1) * size.Value).Take(size.Value);
            }
            return query;
        }

        public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, Specification<T>? spec)
        {
            if (spec != null)
            {
                query = query.ApplyFilter(spec.ToExpression());
            }
            return query;
        }

        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, Expression<Func<T, bool>>? predicate)
        {
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            return query;
        }

        public static IQueryable<T> ApplyOrder<T>(this IQueryable<T> query, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy)
        {
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            return query;
        }

        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> query, List<Expression<Func<T, object>>>? includes) where T : class
        {
            return includes == null ? query : includes.Aggregate(query, (current, include) => current.Include(include));
        }
    }
}
