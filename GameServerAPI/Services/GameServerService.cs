using GameServerAPI.Models;

namespace GameServerAPI.Services
{
    public class GameServerService : IGameServerService
    {
        private static Random _random = new Random();

        public GameServerInfo GetGameServer()
        {
            // Define the IP and port to be returned
            string ipAddress = "localhost";
            int port = _random.Next(20000, 30001);

            // Start the Console App to run the game server
            //StartGameServer(ipAddress, port);

            // Return the server info
            return new GameServerInfo
            {
                IPAddress = ipAddress,
                Port = port
            };
        }
    }
}
