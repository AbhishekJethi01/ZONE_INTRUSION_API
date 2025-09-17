using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ZONE.Entity.Context;
using ZONE.Repository.Interfaces;

namespace ZONE.Repository.Services
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected ZoneDbContext RepositoryContext { get; set; }

        protected RepositoryBase(ZoneDbContext context)
        {
            this.RepositoryContext = context;
        }

        #region with ZoneContext

        public IQueryable<T> FindAll(ZoneDbContext context, params Expression<Func<T, object>>[] includeExpressions)
        {

            var query = context.Set<T>().AsNoTracking();

            if (includeExpressions != null)
            {
                foreach (var includeExpression in includeExpressions)
                {
                    query = query.Include(includeExpression);
                }
            }
            return query;
        }

        public IQueryable<T> FindAllV2(ZoneDbContext context, Expression<Func<T, bool>> expression = null, params Expression<Func<T, object>>[] includeExpressions)
        {

            var query = context.Set<T>().AsNoTracking();
            if (expression != null)
                context.Set<T>().Where(expression);

            if (includeExpressions != null)
            {
                foreach (var includeExpression in includeExpressions)
                {
                    query = query.Include(includeExpression);
                }
            }
            return query;
        }

        public IQueryable<T> FindByCondition(ZoneDbContext context, Expression<Func<T, bool>> expression)
        {
            return context.Set<T>().Where(expression);
        }

        public IQueryable<T> OrderBy(IQueryable<T> query, Dictionary<string, string> orderByProperty)
        {
            if (orderByProperty != null && orderByProperty.Count() > 0)
            {
                foreach (var property in orderByProperty)
                {
                    if (query.PropertyExists<T>(property.Key))
                    {
                        switch (property.Value)
                        {
                            case "asc":
                                query = query.OrderByProperty(property.Key);
                                break;
                            case "desc":
                                query = query.OrderByPropertyDescending(property.Key);
                                break;
                        }
                    }
                }
            }
            return query;
        }

        public IEnumerable<T> Find(ZoneDbContext context, Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, params Expression<Func<T, object>>[] includeExpressions)
        {
            var query = context.Set<T>().AsNoTracking();

            if (includeExpressions != null)
            {
                foreach (var includeExpression in includeExpressions)
                {
                    query = query.Include(includeExpression);
                }
            }

            if (expression != null)
            {
                query = query.Where(expression);
            }

            return query.ToList();
        }

        public async Task<IEnumerable<T>> FindAsync(ZoneDbContext context, Expression<Func<T, bool>> expression = null, Func<IQueryable<T>,
            IOrderedQueryable<T>> orderBy = null, Dictionary<string, string> orderByProperty = null, params Expression<Func<T, object>>[] includeExpressions)
        {
            var query = context.Set<T>().AsNoTracking();

            if (includeExpressions != null)
            {
                foreach (var includeExpression in includeExpressions)
                {
                    query = query.Include(includeExpression).AsNoTracking();
                }
            }

            if (expression != null)
                query = query.Where(expression);

            if (orderBy != null)
                query = orderBy(query);

            if (orderByProperty != null && orderByProperty.Count() > 0)
            {
                foreach (var property in orderByProperty)
                {
                    if (query.PropertyExists<T>(property.Key))
                    {
                        switch (property.Value)
                        {
                            case "asc":
                                query = query.OrderByProperty(property.Key);
                                break;
                            case "desc":
                                query = query.OrderByPropertyDescending(property.Key);
                                break;
                        }
                    }
                }
            }

            return await query.ToListAsync();
        }

        public void Create(ZoneDbContext context, T entity)
        {
            context.Set<T>().Add(entity);
        }

        public async Task CreateAsync(ZoneDbContext context, T entity)
        {
            await context.Set<T>().AddAsync(entity);
        }

        public void CreateMultiple(ZoneDbContext context, IEnumerable<T> entities)
        {
            context.Set<T>().AddRange(entities);
        }

        public async Task CreateMultipleAsync(ZoneDbContext context, IEnumerable<T> entities)
        {
            await context.Set<T>().AddRangeAsync(entities);
        }

        public void Update(ZoneDbContext context, T entity)
        {
            context.Update(entity).State = EntityState.Modified;
        }

        public void Delete(ZoneDbContext context, T entity)
        {
            context.Remove(entity);
        }

        public int SaveEntity(ZoneDbContext context)
        {
            return context.SaveChanges();
        }

        public async Task<int> SaveEntityAsync(ZoneDbContext context)
        {
            return await context.SaveChangesAsync();
        }


        public async Task<int> GetCountAsync(ZoneDbContext context)
        {
            var query = context.Set<T>().AsNoTracking();
            return await query.CountAsync();
        }


        public IEnumerable<TResult> GetSpecificProperties<TResult>(ZoneDbContext context, Func<T, bool>? filter, Func<T, TResult> selector, Dictionary<string, string>? orderByProperties = null)
        {
            var query = context.Set<T>().AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter).AsQueryable();
            }

            if (orderByProperties != null && orderByProperties.Count > 0)
            {
                foreach (var property in orderByProperties)
                {
                    if (query.PropertyExists<T>(property.Key))
                    {
                        switch (property.Value.ToLower())
                        {
                            case "asc":
                                query = query.OrderByProperty(property.Key);
                                break;
                            case "desc":
                                query = query.OrderByPropertyDescending(property.Key);
                                break;
                        }
                    }
                }
            }
            return query.Select(selector).ToList();
        }

        #endregion
    }
}
