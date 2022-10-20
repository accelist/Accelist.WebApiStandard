using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Contains methods for configuring trigram index in an Entity Framework Core model builder.
    /// </summary>
    public static class HasPostgreSqlTrigramIndexExtensions
    {
        /// <summary>
        /// GIN index operator class for pg_trgm
        /// </summary>
        private const string PgTrigramIndexOperator = "gin_trgm_ops";

        /// <summary>
        /// Adds a PostgreSQL GIN trigram index to one column with a custom index name.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityTypeBuilder"></param>
        /// <param name="indexExpression"></param>
        /// <returns></returns>
        public static IndexBuilder HasTrigramIndex<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder, 
            Expression<Func<TEntity, object?>> indexExpression, string indexName)
            where TEntity : class
        {
            return entityTypeBuilder.HasIndex(indexExpression, indexName)
                .HasMethod("GIN")
                .HasOperators(PgTrigramIndexOperator);
        }

        /// <summary>
        /// Adds a PostgreSQL GIN trigram index to one column with an auto-generated index name.
        /// Trigram indexes are not normal indexes and they must be created separately.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityTypeBuilder"></param>
        /// <param name="indexExpression"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IndexBuilder HasTrigramIndex<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            Expression<Func<TEntity, object?>> indexExpression)
            where TEntity : class
        {
            var schema = entityTypeBuilder.Metadata.GetSchema();
            var table = entityTypeBuilder.Metadata.GetTableName()
                ?? throw new InvalidOperationException("Failed to get table name");

            var soid = StoreObjectIdentifier.Table(table, schema);
            var column = entityTypeBuilder.Property(indexExpression).Metadata.GetColumnName(soid)
                ?? throw new InvalidOperationException("Failed to get column name");

            var indexName = $"IX_{table}_{column}_gin_trgrm";
            return entityTypeBuilder.HasTrigramIndex(indexExpression, indexName);
        }
    }
}
