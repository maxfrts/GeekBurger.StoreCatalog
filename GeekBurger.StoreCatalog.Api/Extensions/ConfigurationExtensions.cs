using System;
using System.Linq;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Configuration;
using GeekBurger.StoreCatalog.Api.Configuration;
using System.IO;

namespace GeekBurger.StoreCatalog.Api.Extensions
{
    public static class ConfigurationExtensions
    {
        private const string SubscriptionName = "storecatalog";

        public static IServiceBusNamespace GetServiceBusNamespace(this IConfiguration configuration)
        {
            var config = configuration.GetSection("serviceBus")
                         .Get<ServiceBusConfiguration>();

            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(config.ClientId,
                               config.ClientSecret,
                               config.TenantId,
                               AzureEnvironment.AzureGlobalCloud);

            var serviceBusManager = ServiceBusManager
                .Authenticate(credentials, config.SubscriptionId);

            var xpto = serviceBusManager.Namespaces
                   .GetByResourceGroup(config.ResourceGroup,
                   config.NamespaceName);

            return xpto;
        }

        public static void CreateTopic(this IConfiguration configuration, string TopicName)
        {
            configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

            var serviceBusNamespace = configuration.GetServiceBusNamespace();

            if (!serviceBusNamespace.Topics.List().Any(t => t.Name
            .Equals(TopicName, StringComparison.InvariantCultureIgnoreCase)))
            {
                serviceBusNamespace.Topics
                .Define(TopicName)
                .WithNewSubscription(SubscriptionName)
                .WithSizeInMB(1024)
                .Create();
            }

            var topic = serviceBusNamespace.Topics.GetByName(TopicName);

            if (topic.Subscriptions.List().Any(subscription => subscription.Name.Equals(SubscriptionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                topic.Subscriptions.DeleteByName(SubscriptionName);
            }

            topic.Subscriptions.Define(SubscriptionName).Create();
        }
    }
}
