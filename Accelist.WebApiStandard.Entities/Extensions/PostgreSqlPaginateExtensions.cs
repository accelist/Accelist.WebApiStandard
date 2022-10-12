using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Moderate performance boost by separating this static method into another static class. 
    /// Read more: https://learn.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2015/code-quality/ca1822-mark-members-as-static?view=vs-2015&redirectedfrom=MSDN
    /// </summary>
    internal static class StringQuoter
    {
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

    public class PostgreSqlDbSetPagination<TEntity> where TEntity : class
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly IEntityType _entityType;
        private readonly string _tableName;
        private readonly string? _schema;

        public PostgreSqlDbSetPagination(DbSet<TEntity> dbSet, IEntityType entityType)
        {
            _dbSet = dbSet;
            _entityType = entityType;
            _tableName = entityType.GetTableName() ??
                throw new InvalidOperationException($"Cannot get table name for {typeof(TEntity).Name}");
            _schema = entityType.GetSchema();
        }

        private string GetTableFullName()
        {
            // https://www.npgsql.org/efcore/modeling/table-column-naming.html
            if (_schema != null)
            {
                return $"{StringQuoter.Quote(_schema)}.{StringQuoter.Quote(_tableName)}";
            }

            return StringQuoter.Quote(_tableName);
        }

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

            var soid = StoreObjectIdentifier.Table(_tableName, _schema);
            var columnName = _entityType.GetProperty(propertyInfo.Name)?.GetColumnName(soid) ??
                throw new InvalidOperationException($"Cannot get column name for {propertyInfo.Name}");

            return columnName;
        }

        public IQueryable<TEntity> With<TColumn1>(
            Expression<Func<TEntity, TColumn1>> column1Selector,
            TColumn1 lastColumn1)
        {
            if (lastColumn1 is null)
            {
                return _dbSet.AsNoTracking();
            }

            var column1Name = GetColumnName(column1Selector);
            var sql = $"SELECT * FROM {GetTableFullName()} WHERE {StringQuoter.Quote(column1Name)} > {{0}}";

            return _dbSet.FromSqlRaw(sql, lastColumn1).AsNoTracking();
        }

        public IQueryable<TEntity> With<TColumn1, TColumn2>(
            Expression<Func<TEntity, TColumn1>> column1Selector,
            Expression<Func<TEntity, TColumn2>> column2Selector,
            TColumn1 lastColumn1,
            TColumn2 lastColumn2
            )
        {
            if (lastColumn1 is null || lastColumn2 is null)
            {
                return _dbSet.AsNoTracking();
            }

            var column1Name = GetColumnName(column1Selector);
            var column2Name = GetColumnName(column2Selector);
            var sql = $"SELECT * FROM {GetTableFullName()} WHERE ({StringQuoter.Quote(column1Name)}, {StringQuoter.Quote(column2Name)}) > ({{0}}, {{1}})";

            return _dbSet.FromSqlRaw(sql, lastColumn1, lastColumn2).AsNoTracking();
        }
    }

    public static class PostgreSqlPaginateExtensions
    {
        public static PostgreSqlDbSetPagination<TEntity> Paginate<TEntity>(this DbContext dbContext) where TEntity : class
        {
            var entityType = dbContext.Model.FindEntityType(typeof(TEntity)) ??
                throw new InvalidOperationException($"Cannot get entity type for {typeof(TEntity).Name}");

            return new PostgreSqlDbSetPagination<TEntity>(dbContext.Set<TEntity>(), entityType);
        }
    }
}
