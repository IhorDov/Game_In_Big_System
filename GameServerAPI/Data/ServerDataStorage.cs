using GameServerAPI.Models;

namespace GameServerAPI.Data
{
    public class ServerDataStorage : IStorage
    {
        // Dictionary to store service names as keys and list of service URLs as values
        private readonly Dictionary<string, List<string>> _serviceDictionary;

        public ServerDataStorage()
        {
            _serviceDictionary = new Dictionary<string, List<string>>();
        }

        // Add a service name and its corresponding service URL
        public bool AddItem(string name, string serviceUrl)
        {
            // If the service name exists, add the URL to the list
            if (_serviceDictionary.ContainsKey(name))
            {
                if (!_serviceDictionary[name].Contains(serviceUrl))
                {
                    _serviceDictionary[name].Add(serviceUrl);
                    return true; // URL added successfully
                }
                return false; // URL already exists
            }
            else
            {
                // If the service name does not exist, create a new entry
                _serviceDictionary[name] = new List<string> { serviceUrl };
                return true;
            }
        }

        // Remove a service URL for a given service name
        public bool RemoveItem(string name, string serviceUrl)
        {
            // Check if the service exists
            if (_serviceDictionary.ContainsKey(name))
            {
                // Try to remove the service URL
                if (_serviceDictionary[name].Remove(serviceUrl))
                {
                    // If the list of URLs becomes empty, remove the service name
                    if (_serviceDictionary[name].Count == 0)
                    {
                        _serviceDictionary.Remove(name);
                    }
                    return true; // URL removed successfully
                }
            }
            return false; // Service or URL not found
        }

        // Get all service names
        public List<string> GetAllServiceNames()
        {
            return _serviceDictionary.Keys.ToList();
        }

        // Get all service URLs for a given service name
        public List<string> GetServiceUrlForService(string serviceName)
        {
            // Return the list of URLs if the service exists, otherwise return an empty list
            return _serviceDictionary.ContainsKey(serviceName) ? _serviceDictionary[serviceName] : new List<string>();
        }
    }
}
