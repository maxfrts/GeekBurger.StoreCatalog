using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using GeekBurger.StoreCatalog.Api.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekBurger.StoreCatalog.Api.Services.ServiceBus.Topic
{
    public class TopicBus : ITopicBus
    {
        private readonly ServiceBusConfiguration _busConfig;

        public TopicBus(IOptions<ServiceBusConfiguration> option)
        {
            _busConfig = option.Value;
        }

        public async Task SendAsync(string topic, string message)
        {
            var topicClient = new TopicClient(_busConfig.ConnectionString, topic);
            await topicClient.SendAsync(new Message(Encoding.UTF8.GetBytes(message)));
            await topicClient.CloseAsync();
        }

        public async Task SendAsync(string topic, IList<string> messages)
        {
            var topicClient = new TopicClient(_busConfig.ConnectionString, topic);
            var json = JsonConvert.SerializeObject(messages);
            await topicClient.SendAsync(messages.Select(m => new Message(Encoding.UTF8.GetBytes(m))).ToList());
            await topicClient.CloseAsync();
        }

    }
}
