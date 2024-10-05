using Client.Models;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class GameClient
    {
        private readonly HttpClient client;
        //private const string baseUrl = "https://localhost:10002/api/auth/"; //does not work nowgit status
        private const string baseUrl = "http://localhost:10001/api/auth/";

        public GameClient()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            client = new HttpClient(handler);
        }

        // Method for handling user registration
        public async Task<string> HandlePostRegister(User user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}register", content);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    Console.WriteLine($"Registration failed: {response.StatusCode}");
                    return string.Empty;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new RegistrationException($"Error during registration: {ex.Message}");
            }
        }

        // Method for handling user login
        public async Task<string> Login(User user)
        {
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{baseUrl}login", content);
                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();

                    return token;
                }
                else
                {
                    Console.WriteLine($"Login failed: {response.StatusCode}");
                    return null!;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new LoginException($"Error during login: {ex.Message}");
            }
        }

        // Retrieve game server IP and port from the GameServer
        public async Task<GameServerInfo> GetGameServer(string jwt)
        {
            // Clear the default request headers
            client.DefaultRequestHeaders.Clear();

            // Add the JWT token to the Authorization header
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

            try
            {
                // Define the URL of the endpoint
                //string serverInfoUrl = "https://localhost:10003/api/PlayerRegisterGame/server-info";
                string serverInfoUrl = "https://localhost:5000/api/api/PlayerRegisterGame/server-info";
                Console.WriteLine($"Requesting game server info from {serverInfoUrl}");

                // Send the GET request
                var response = await client.GetAsync(serverInfoUrl);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    var result = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response into a GameServerInfo object
                    var serverInfo = JsonConvert.DeserializeObject<GameServerInfo>(result);

                    // Ensure the deserialized object is valid
                    if (serverInfo != null)
                    {
                        Console.WriteLine($"Game server info retrieved: {serverInfo.IP}:{serverInfo.Port}");
                        return serverInfo;
                    }
                    else
                    {
                        throw new Exception("Failed to deserialize the game server information.");
                    }
                }
                else
                {
                    // Handle unsuccessful responses
                    throw new ServerConnectionException($"Failed to retrieve game server. Status Code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle network-related errors
                throw new ServerConnectionException($"Error retrieving game server: {ex.Message}");
            }
            catch (JsonSerializationException ex)
            {
                // Handle JSON deserialization errors
                throw new ServerConnectionException($"Error parsing game server info: {ex.Message}");
            }
        }


        // Connect to the game server
        public static async Task<bool> ConnectToGameServer(string ipAddress, int port)
        {
            try
            {
                using (var client = new TcpClient(ipAddress, port))
                {
                    Console.WriteLine("Connected to the server!");

                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.ASCII.GetBytes("Player was connected");
                    await stream.WriteAsync(data, 0, data.Length);

                    byte[] buffer = new byte[256];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Server response: {response}");

                    return true;
                }
            }
            catch (SocketException ex)
            {
                throw new ServerConnectionException($"Error connecting to game server: {ex.Message}");
            }
        }
    }
}
