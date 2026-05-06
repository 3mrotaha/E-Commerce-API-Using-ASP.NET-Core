using System.Linq.Expressions;
using eCommerce.Domain.Entities;
namespace eCommerce.Domain.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate, int pageNumber, int pageSize);
}
