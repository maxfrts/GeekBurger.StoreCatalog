namespace GeekBurger.StoreCatalog.Api.Services.interfaces
{
    public interface IReceiveMessagesApiFactory
    {
        ReceiveMessagesApiService CreateNewApiService();
    }
}
