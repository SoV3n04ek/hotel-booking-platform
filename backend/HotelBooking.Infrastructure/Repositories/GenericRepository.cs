using HotelBooking.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly HotelsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(HotelsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> GetAll() => _dbSet.AsNoTracking();

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Delete(T entity) => _dbSet.Remove(entity);
}
