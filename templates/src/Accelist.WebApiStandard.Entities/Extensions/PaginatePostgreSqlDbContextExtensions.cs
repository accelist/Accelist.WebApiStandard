using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Contains methods for querying <typeparamref name="TEntity"/> using keyset pagination logic
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class DbSetPagination<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly IEntityType _entityType;
        private readonly string _tableName;
        private readonly string _schemaQualifiedTableName;

        /// <summary>
        /// Constructs an instance of <see cref="DbSetPagination{TEntity}"/>
        /// </summary>
        /// <param name="dbSet"></param>
        /// <param name="entityType"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public DbSetPagination(DbSet<TEntity> dbSet, IEntityType entityType)
        {
            _dbSet = dbSet;
            _entityType = entityType;
            _tableName = entityType.GetTableName() ??
                throw new InvalidOperationException($"Cannot get table name for {typeof(TEntity).Name}");
            _schemaQualifiedTableName = entityType.GetSchemaQualifiedTableName() ??
                throw new InvalidOperationException($"Cannot get table name for {typeof(TEntity).Name}");
        }

        /// <summary>
        /// Get the PostgreSQL column name for property <typeparamref name="TColumn"/>
        /// </summary>
        /// <typeparam name="TColumn"></typeparam>
        /// <param name="columnSelector"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private string GetColumnName<TColumn>(Expression<Func<TEntity, TColumn>> columnSelector)
        {
            if (columnSelector.Body is not MemberExpression memex)
            {
                throw new InvalidOperationException($"Selected column is not a valid MemberExpression");
            }
            if (memex.Member is not PropertyInfo propertyInfo)
            {
                throw new InvalidOperationException($"Selected member is not a valid Property");
            }

            var soid = StoreObjectIdentifier.Table(_tableName, _entityType.GetSchema());
            var columnName = _entityType.GetProperty(propertyInfo.Name)?.GetColumnName(soid) ??
                throw new InvalidOperationException($"Cannot get column name for {propertyInfo.Name}");

            return columnName;
        }

        /// <summary>
        /// Query <typeparamref name="TEntity"/> using keyset pagination logic on a column with read-only result. <br></br>
        /// <paramref name="column1Selector"/> <b>must be the unique column of the table</b>, such as Primary Key or Unique Index.
        /// </summary>
        /// <typeparam name="TColumn1"></typeparam>
        /// <param name="column1Selector"></param>
        /// <param name="lastColumn1"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public IQueryable<TEntity> With<TColumn1>(
            Expression<Func<TEntity, TColumn1>> column1Selector,
            TColumn1 lastColumn1,
            bool descending = false)
        {
            if (lastColumn1 is null)
            {
                if (descending)
                {
                    return _dbSet.AsNoTracking().OrderByDescending(column1Selector);
                }

                return _dbSet.AsNoTracking().OrderBy(column1Selector);
            }

            var comparer = descending ? '<' : '>';
            var column1Name = GetColumnName(column1Selector);
            var sql = $"SELECT * FROM {_schemaQualifiedTableName} WHERE {PaginatePostgreSqlDbContextExtensions.Quote(column1Name)} {comparer} {{0}}";

            var query = _dbSet.FromSqlRaw(sql, lastColumn1).AsNoTracking();
            if (descending)
            {
                return query.OrderByDescending(column1Selector);
            }
            return query.OrderBy(column1Selector);
        }

        /// <summary>
        /// Query <typeparamref name="TEntity"/> using keyset pagination logic on two columns with read-only result. <br></br>
        /// <paramref name="column1Selector"/> is usually a non-unique column of the table. <br></br>
        /// <paramref name="column2Selector"/> <b>must be the unique column of the table</b>, such as Primary Key or Unique Index.
        /// </summary>
        /// <typeparam name="TColumn1"></typeparam>
        /// <typeparam name="TColumn2"></typeparam>
        /// <param name="column1Selector"></param>
        /// <param name="column2Selector"></param>
        /// <param name="lastColumn1"></param>
        /// <param name="lastColumn2"></param>
        /// <param name="descending"></param>
        /// <returns></returns>
        public IQueryable<TEntity> With<TColumn1, TColumn2>(
            Expression<Func<TEntity, TColumn1>> column1Selector,
            Expression<Func<TEntity, TColumn2>> column2Selector,
            TColumn1 lastColumn1,
            TColumn2 lastColumn2,
            bool descending = false)
        {
            if (lastColumn1 is null || lastColumn2 is null)
            {
                if (descending)
                {
                    return _dbSet.AsNoTracking()
                        .OrderByDescending(column1Selector)
                        .ThenByDescending(column2Selector);
                }

                return _dbSet.AsNoTracking()
                    .OrderBy(column1Selector)
                    .ThenBy(column2Selector);
            }

            var comparer = descending ? '<' : '>';
            var column1Name = GetColumnName(column1Selector);
            var column2Name = GetColumnName(column2Selector);

            // https://learn.microsoft.com/en-us/ef/core/querying/pagination#multiple-pagination-keys
            // https://github.com/dotnet/efcore/issues/26822
            var sql = $"SELECT * FROM {_schemaQualifiedTableName} WHERE ({PaginatePostgreSqlDbContextExtensions.Quote(column1Name)}, {PaginatePostgreSqlDbContextExtensions.Quote(column2Name)}) {comparer} ({{0}}, {{1}})";

            var query = _dbSet.FromSqlRaw(sql, lastColumn1, lastColumn2).AsNoTracking();
            if (descending)
            {
                return query.OrderByDescending(column1Selector).ThenByDescending(column2Selector);
            }
            return query.OrderBy(column1Selector).ThenBy(column2Selector);
        }
    }

    /// <summary>
    /// Contains extension methods for querying keyset pagination against <see cref="DbContext"/>
    /// </summary>
    public static class PaginatePostgreSqlDbContextExtensions
    {
        /// <summary>
        /// Begin pagination query for entity <typeparamref name="TEntity"/>
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static DbSetPagination<TEntity> Paginate<TEntity>(this DbContext dbContext) where TEntity : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity)) ??
                throw new InvalidOperationException($"Cannot get entity type for {typeof(TEntity).Name}");

            return new DbSetPagination<TEntity>(dbContext.Set<TEntity>(), entityType);
        }

        /// <summary>
        /// Put quote around the string. Used to place quotation marks around identifiers used in PostgreSQL raw SQL query. 
        /// Read more: <br></br> https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-IDENTIFIERS
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string Quote(string value)
        {
            return $"\"{value}\"";
        }
    }
}
