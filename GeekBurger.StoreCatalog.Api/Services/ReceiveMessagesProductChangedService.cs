
using GeekBurger.Products.Contract;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using GeekBurger.StoreCatalog.Api.Configuration;
using GeekBurger.StoreCatalog.Api.Services.interfaces;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeekBurger.StoreCatalog.Api.Services
{
    public class ReceiveMessagesProductChangedService
    {
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private static ServiceBusConfiguration _serviceBusConfiguration;

        private readonly IProductsService _productsService;

        public ReceiveMessagesProductChangedService(
                                      IProductsService productsService,
                                      string topic,
                                      string subscription,
                                      string filterName = null,
                                      string filter = null)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _productsService = productsService;

            _serviceBusConfiguration = configuration.GetSection("serviceBus").Get<ServiceBusConfiguration>();

            _topicName = topic;
            _subscriptionName = subscription;

            ReceiveMessages(filterName, filter);
        }

        public void ReceiveMessages(string filterName = null, string filter = null)
        {
            var subscriptionClient = new SubscriptionClient
                (_serviceBusConfiguration.ConnectionString, _topicName, _subscriptionName);

            var messageOptions = new MessageHandlerOptions(ExceptionHandle) { AutoComplete = true };

            if (filterName != null && filter != null)
            {
                const string defaultRule = "$default";

                if (subscriptionClient.GetRulesAsync().Result.Any(x => x.Name == defaultRule))
                    subscriptionClient.RemoveRuleAsync(defaultRule).Wait();

                if (subscriptionClient.GetRulesAsync().Result.All(x => x.Name != filterName))
                    subscriptionClient.AddRuleAsync(new RuleDescription
                    {
                        Filter = new CorrelationFilter { Label = filter },
                        Name = filterName
                    }).Wait();

            }

            subscriptionClient.RegisterMessageHandler(Handle, messageOptions);
        }

        public Task Handle(Message message, CancellationToken arg2)
        {
            var messageString = "";
            if (message.Body != null)
                messageString = Encoding.UTF8.GetString(message.Body);

            var productChanged = JsonConvert.DeserializeObject<ProductChangedMessage>(messageString);

            _productsService.SaveProduct(productChanged);

            return Task.CompletedTask;
        }

        public Task ExceptionHandle(ExceptionReceivedEventArgs arg)
        {
            var context = arg.ExceptionReceivedContext;
            return Task.CompletedTask;
        }
    }
}
