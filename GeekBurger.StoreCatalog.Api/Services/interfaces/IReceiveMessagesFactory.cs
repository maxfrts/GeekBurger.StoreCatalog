namespace GeekBurger.StoreCatalog.Api.Services.interfaces
{
    public interface IReceiveMessagesFactory
    {
        ReceiveMessagesProductionAreaChangedService CreateNewProductionAreaChangedService(string topic, string subscription, string filterName = null, string filter = null);

        ReceiveMessagesProductChangedService CreateNewProductChangedService(string topic, string subscription, string filterName = null, string filter = null);
    }
}
