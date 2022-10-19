using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Contains methods for configuring trigram index in an Entity Framework Core model builder.
    /// </summary>
    public static class HasTrigramIndexExtension
    {
        /// <summary>
        /// GIN index operator class for pg_trgm
        /// </summary>
        private const string PgTrigramIndexOperator = "gin_trgm_ops";

        /// <summary>
        /// Adds a PostgreSQL GIN trigram index to a column.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityTypeBuilder"></param>
        /// <param name="indexExpression"></param>
        /// <returns></returns>
        public static IndexBuilder HasTrigramIndex<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder, 
            Expression<Func<TEntity, object?>> indexExpression)
            where TEntity : class
        {
            return entityTypeBuilder.HasIndex(indexExpression).HasMethod("GIN").HasOperators(PgTrigramIndexOperator);
        }
    }
}
