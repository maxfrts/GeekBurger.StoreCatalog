using Microsoft.Extensions.Configuration;
using GeekBurger.Products.Contract;
using GeekBurger.Ingredients.Contract;
using GeekBurger.StoreCatalog.Contract;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using GeekBurger.StoreCatalog.Api.Services.interfaces;
using GeekBurger.StoreCatalog.Contract.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GeekBurger.StoreCatalog.Api.Services.ServiceBus.Topic;

namespace GeekBurger.StoreCatalog.Api.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheName = "products";
        private readonly string _baseUrlProducts;
        private readonly string _baseUrlIngredients;
        private readonly ITopicBus _topicBus;

        public ProductsService(IMemoryCache memoryCache, IConfiguration configuration, ITopicBus topicBus)
        {
            _memoryCache = memoryCache;
            _baseUrlProducts = configuration.GetValue<string>("ProductsApiBaseUrl");
            _baseUrlIngredients = configuration.GetValue<string>("IngredientsApiBaseUrl");
            _topicBus = topicBus;
        }

        public async Task<List<ProductToGet>> GetProducts(string StoreName)
        {
            List<ProductToGet> products = new List<ProductToGet>();
            using (var httpClient = new HttpClient())
            {
                string url = "";
                if (string.IsNullOrEmpty(StoreName))
                {
                    url = _baseUrlProducts + "/api/products";
                }
                else
                {
                    url = _baseUrlProducts + "/api/products?storeName=" + StoreName;
                }

                using (var response = await httpClient.GetAsync(url))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        products = JsonConvert.DeserializeObject<List<ProductToGet>>(apiResponse);
                    }
                }
            }

            return products;
        }

        public void SaveProducts(List<ProductToGet> products)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddDays(1)
            };

            _memoryCache.Set(_cacheName, products, cacheOptions);
            
        }

        public void SaveProduct(ProductChangedMessage productChanged)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddDays(1)
            };

            var products = GetProductsFromMemory();

            switch (productChanged.State)
            {
                case ProductState.Deleted:
                    RemoveProduct(products, productChanged.Product);
                    break;
                case ProductState.Modified:
                    RemoveProduct(products, productChanged.Product);
                    products.Add(productChanged.Product);
                    break;
                case ProductState.Added:
                    products.Add(productChanged.Product);
                    break;
                default:
                    break;
            }

            _memoryCache.Set(_cacheName, products, cacheOptions);

        }

        private void RemoveProduct(List<ProductToGet> products, ProductToGet product) {
            var productToRemove = products.Where(p => p.ProductId == product.ProductId).FirstOrDefault();
            if (productToRemove != null)
            {
                products.Remove(productToRemove);
            }
        }

        public List<ProductToGet> GetProductsFromMemory()
        {
            _memoryCache.TryGetValue(_cacheName, out List<ProductToGet> products);

            return products;
        }

        public async Task<IEnumerable<ProductToGet>> GetProductsAsync(ProductsRequest productRequest)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddHours(6)
            };

            //Try to get the product list from cache
            if (!_memoryCache.TryGetValue(_cacheName, out List<ProductToGet> products))
            {
                //if cache empty get from api
                products = this.GetProducts(productRequest.StoreName).Result;

                if (null != products && products.Count > 0)
                {
                    _memoryCache.Set(_cacheName, products, cacheOptions);
                }
            }

            //If user has restrictions retrieve the products considering the restrictions
            if (productRequest.Restrictions != null)
            {

                //Retrieve from ingredients the products considering the restrictions
                var productsRestriction = await this.GetProductsByRestrictions(productRequest);

                //foreach retrived product update the ingredients list
                foreach (var product in productsRestriction)
                {
                    var selectedProduct = products.Where(p => p.ProductId == product.ProductId).FirstOrDefault();
                    products.Remove(selectedProduct);

                    selectedProduct.Items = product.Ingredients.Select(i => new ItemToGet() { Name = i }).ToList();

                    products.Add(selectedProduct);
                }

                _memoryCache.Set(_cacheName, products, cacheOptions);


                UserWithLessOffer userRestriction = new UserWithLessOffer
                {
                    UserId = productRequest.UserId,
                    Restrictions = productRequest.Restrictions
                };
                try
                {
                    string serializedObj = JsonConvert.SerializeObject(userRestriction);

                    //Publish to topic UserWithLessOffer the user restrictions
                    await _topicBus.SendAsync("userwithlessoffer", serializedObj);
                }
                catch (Exception ex)
                {
                    throw;
                }
                
                var productsId = productsRestriction.Select(p => p.ProductId);

                //return the list of products considereng the restrictions
                return products.Where(p => productsId.Contains(p.ProductId));    
               
            }

            //If user has no restrictions return all products
            return products;
        }


        public async Task<List<IngredientToGet>> GetProductsByRestrictions(ProductsRequest productRequest)
        {
            List<IngredientToGet> ingredients = new List<IngredientToGet>();

            var storeName = productRequest.StoreName;

            var restrictionsParameters = string.Empty;

            foreach (var restriction in productRequest.Restrictions)
            {
                restrictionsParameters += "&Restrictions="+restriction;
            }
            
            var url = _baseUrlIngredients + "/api/products/byrestrictions/productRequest?StoreName="+storeName+restrictionsParameters;

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        ingredients = JsonConvert.DeserializeObject<List<IngredientToGet>>(apiResponse);
                    }
                }
            }

            return ingredients;
        }
    }
}
