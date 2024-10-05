namespace GameServerAPI.Data
{
    public interface IStorage
    {
        bool AddItem(string name, string serviceUrl);
        bool RemoveItem(string name, string serviceUrl);
        List<string> GetAllServiceNames();
        List<string> GetServiceUrlForService(string serviceName);
    }
}
