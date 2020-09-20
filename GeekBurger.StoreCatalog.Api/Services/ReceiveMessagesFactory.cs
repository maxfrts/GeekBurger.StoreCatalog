using GeekBurger.StoreCatalog.Api.Services.interfaces;

namespace GeekBurger.StoreCatalog.Api.Services
{
    public class ReceiveMessagesFactory : IReceiveMessagesFactory
    {
        private readonly IAreasService _areasService;
        private readonly IProductsService _productsService;

        public ReceiveMessagesFactory(IAreasService areasService, IProductsService productsService)
        {
            _areasService = areasService;
            _productsService = productsService;

            CreateNewProductionAreaChangedService("productionareachanged", "sub_storeCatalog");

            CreateNewProductChangedService("productchangedtopic", "sub_storeCatalog");
        }

        public ReceiveMessagesProductionAreaChangedService CreateNewProductionAreaChangedService(string topic, string subscription, string filterName = null, string filter = null)
        {
            return new ReceiveMessagesProductionAreaChangedService(_areasService, topic, subscription, filterName, filter);
        }

        public ReceiveMessagesProductChangedService CreateNewProductChangedService(string topic, string subscription, string filterName = null, string filter = null)
        {
            return new ReceiveMessagesProductChangedService(_productsService, topic, subscription, filterName, filter);
        }

    }
}
