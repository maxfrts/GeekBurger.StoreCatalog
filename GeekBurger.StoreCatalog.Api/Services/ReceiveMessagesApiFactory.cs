using GeekBurger.StoreCatalog.Api.Services.interfaces;
using GeekBurger.StoreCatalog.Api.Services.ServiceBus.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeekBurger.StoreCatalog.Api.Services
{
    public class ReceiveMessagesApiFactory : IReceiveMessagesApiFactory
    {
        private readonly IAreasService _areasService;
        private readonly IProductsService _productsService;
        private readonly ITopicBus _topicBus;

        public ReceiveMessagesApiFactory(IAreasService areasService, IProductsService productsService, ITopicBus topicBus)
        {
            _areasService = areasService;
            _productsService = productsService;
            _topicBus = topicBus;

            CreateNewApiService();
        }

        public ReceiveMessagesApiService CreateNewApiService() {
            return new ReceiveMessagesApiService(_areasService, _productsService, _topicBus);
        }
    }
}
