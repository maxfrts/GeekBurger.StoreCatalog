using GeekBurger.Products.Contract;
using GeekBurger.StoreCatalog.Contract.Requests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeekBurger.StoreCatalog.Api.Services.interfaces
{
    public interface IProductsService
    {
        Task<List<ProductToGet>> GetProducts(string StoreName);

        Task<IEnumerable<ProductToGet>> GetProductsAsync(ProductsRequest productRequest);

        void SaveProducts(List<ProductToGet> products);

        void SaveProduct(ProductChangedMessage product);

        List<ProductToGet> GetProductsFromMemory();
    }
}
