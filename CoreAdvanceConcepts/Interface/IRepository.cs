using CoreAdvanceConcepts.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreAdvanceConcepts.Interface
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<ResponceMessage<IEnumerable<TEntity>>> GetDataList();
        Task<ResponceMessage<TEntity>> EditData(TEntity entity);
        Task<ResponceMessage<TEntity>> CreateData(TEntity entity);
        Task<ResponceMessage<TEntity>> DeleteData(TEntity entity);
    }
}
