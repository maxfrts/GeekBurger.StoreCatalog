using GeekBurger.StoreCatalog.Api.Services.interfaces;
using GeekBurger.StoreCatalog.Api.Services.ServiceBus.Topic;
using Newtonsoft.Json;

namespace GeekBurger.StoreCatalog.Api.Services
{
    public class ReceiveMessagesApiService
    {
        private readonly IAreasService _areasService;
        private readonly IProductsService _productsService;
        private readonly ITopicBus _topicBus;

        public ReceiveMessagesApiService(IAreasService areasService, IProductsService productsService, ITopicBus topicBus)
        {
            _areasService = areasService;
            _productsService = productsService;
            _topicBus = topicBus;

            CallApiServices();

            var products = _productsService.GetProductsFromMemory();

            if (products.Count > 0)
            {
                var storeReady = new Contract.StoreCatalogReady
                {
                    Ready = true,
                    StoreName = "Paulista"
                };

                _topicBus.SendAsync("storecatalogready", JsonConvert.SerializeObject(storeReady));
            }
        }

        public void CallApiServices() {
            var areas = _areasService.GetProductionAreas();
            _areasService.SaveAreas(areas.Result);
            
            var products = _productsService.GetProducts("Paulista");
            _productsService.SaveProducts(products.Result);
        }
    }
}
