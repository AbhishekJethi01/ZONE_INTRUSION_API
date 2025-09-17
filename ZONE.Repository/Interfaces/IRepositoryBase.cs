using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ZONE.Entity.Context;

namespace ZONE.Repository.Interfaces
{
    public interface IRepositoryBase<T> where T : class
    {
        #region with thread-safe ZoneContext

        IQueryable<T> FindAll(ZoneDbContext context, params Expression<Func<T, object>>[] includeExpressions);

        IQueryable<T> FindAllV2(ZoneDbContext context, Expression<Func<T, bool>> expression = null, params Expression<Func<T, object>>[] includeExpressions);

        IQueryable<T> FindByCondition(ZoneDbContext context, Expression<Func<T, bool>> expression);

        IQueryable<T> OrderBy(IQueryable<T> query, Dictionary<string, string> orderByProperty);

        IEnumerable<T> Find(ZoneDbContext context, Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeExpressions);

        Task<IEnumerable<T>> FindAsync(ZoneDbContext context, Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Dictionary<string, string> orderByProperty = null, params Expression<Func<T, object>>[] includeExpressions);

        void Create(ZoneDbContext context, T entity);

        Task CreateAsync(ZoneDbContext context, T entity);

        void CreateMultiple(ZoneDbContext context, IEnumerable<T> entities);

        Task CreateMultipleAsync(ZoneDbContext context, IEnumerable<T> entities);

        void Update(ZoneDbContext context, T entity);

        void Delete(ZoneDbContext context, T entity);

        int SaveEntity(ZoneDbContext context);

        Task<int> SaveEntityAsync(ZoneDbContext context);

        Task<int> GetCountAsync(ZoneDbContext context);

        IEnumerable<TResult> GetSpecificProperties<TResult>(ZoneDbContext context, Func<T, bool>? filter, Func<T, TResult> selector, Dictionary<string, string>? orderByProperties = null);
        #endregion
    }
}
