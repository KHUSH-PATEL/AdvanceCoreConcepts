using Azure;
using CoreAdvanceConcepts.Caching;
using CoreAdvanceConcepts.DataContext;
using CoreAdvanceConcepts.Interface;
using CoreAdvanceConcepts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Dapper.SqlMapper;

namespace CoreAdvanceConcepts.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly EmployeeDbContext _context;
        private readonly DbSet<TEntity> _entities;
        private readonly RedisCacheService _cacheService;
        private readonly string cacheKey = typeof(TEntity)+"Data";

        public Repository(EmployeeDbContext context, RedisCacheService cacheService)
        {
            _context = context;
            _entities = context.Set<TEntity>();
            _cacheService = cacheService;
        }

        public async Task<ResponceMessage<TEntity>> CreateData(TEntity entity)
        {
            ResponceMessage<TEntity> responce = new ResponceMessage<TEntity>();
            try
            {
                await _cacheService.RemoveAsync(cacheKey);
                await _entities.AddAsync(entity);
                await _context.SaveChangesAsync();
                var cachedData = await _entities.ToListAsync();
                await _cacheService.SetAsync(cacheKey, cachedData, TimeSpan.FromMinutes(10));
                responce.IsSuccess = true;
                responce.Data = entity;
            }
            catch (Exception ex)
            {
                responce.IsSuccess = false;
                responce.Message = "An error occurred while fetching entities.";
                responce.ErrorMessage = new List<string> { ex.Message };
            }
            return responce;
        }

        public async Task<ResponceMessage<TEntity>> DeleteData(TEntity entity)
        {
            ResponceMessage<TEntity> responce = new ResponceMessage<TEntity>();
            try
            {
                await _cacheService.RemoveAsync(cacheKey);
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                var cachedData = await _entities.ToListAsync();
                await _cacheService.SetAsync(cacheKey, cachedData, TimeSpan.FromMinutes(10));
                responce.IsSuccess = true;
                responce.Data = entity;
            }
            catch (Exception ex)
            {
                responce.IsSuccess = false;
                responce.Message = "An error occurred while fetching entities.";
                responce.ErrorMessage = new List<string> { ex.Message };
            }
            return responce;
        }        

        public async Task<ResponceMessage<TEntity>> EditData(TEntity entity)
        {
            ResponceMessage<TEntity> responce = new ResponceMessage<TEntity>();
            try
            {
                await _cacheService.RemoveAsync(cacheKey);
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                var cachedData = await _entities.ToListAsync();
                await _cacheService.SetAsync(cacheKey, cachedData, TimeSpan.FromMinutes(10));
                responce.IsSuccess = true;
                responce.Data = entity;
            }
            catch (Exception ex)
            {
                responce.IsSuccess = false;
                responce.Message = "An error occurred while fetching entities.";
                responce.ErrorMessage = new List<string> { ex.Message };
            }
            return responce;
        }

        public async Task<ResponceMessage<TEntity>> GetDataById(Expression<Func<TEntity, bool>> predicate)
        {
            ResponceMessage<TEntity> responce = new ResponceMessage<TEntity>();
            try
            {
                TEntity? employee = null;
                var cachedData = await _cacheService.GetAsync<IEnumerable<TEntity>>(cacheKey);
                if (cachedData == null)
                {
                    employee = _entities.FirstOrDefault(predicate.Compile());
                }
                else
                {
                    employee = cachedData.FirstOrDefault(predicate.Compile());
                }
                responce.IsSuccess = true;
                if(employee != null)
                {
                    responce.Data = employee;
                }                
            }
            catch(Exception ex)
            {
                responce.IsSuccess = false;
                responce.Message = "An error occurred while fetching entities.";
                responce.ErrorMessage = new List<string> { ex.Message };
            }
            return responce;
        }

        public async Task<ResponceMessage<IEnumerable<TEntity>>> GetDataList()
        {
            ResponceMessage<IEnumerable<TEntity>> response = new ResponceMessage<IEnumerable<TEntity>>();
            try
            {
                var cachedData = await _cacheService.GetAsync<IEnumerable<TEntity>>(cacheKey);
                if (cachedData == null)
                {
                    cachedData = await _entities.ToListAsync();
                    await _cacheService.SetAsync(cacheKey, cachedData, TimeSpan.FromMinutes(10));
                }
                response.IsSuccess = true;
                response.DataCount = cachedData.Count();
                response.Data = cachedData;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An error occurred while fetching entities.";
                response.ErrorMessage = new List<string> { ex.Message };
            }
            return response;
        }
    }
}
