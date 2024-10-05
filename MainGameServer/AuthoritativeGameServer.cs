using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MainGameServer.Models;
using Newtonsoft.Json;

namespace MainGameServer
{
    public class AuthoritativeGameServer
    {
        private readonly HttpClient httpClient;
        private System.Timers.Timer timer;
        private TcpListener? tcpListener;
        private List<TcpClient> clients = new List<TcpClient>();
        private GameWorld world;
        private float snapshotSpeed = 3;
        private CommandExecutor commandExecutor = new CommandExecutor();


        public AuthoritativeGameServer()
        {
            // Create a custom HttpClientHandler to handle proxy and certificate issues
            var httpClientHandler = new HttpClientHandler
            {
                UseProxy = false,  // Bypass proxy for localhost
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator  // Accept self-signed certificates
            };

            // Pass the custom handler to HttpClient
            httpClient = new HttpClient(httpClientHandler);

            commandExecutor.RegisterCommand("tick", SetTickRate);
            timer = new System.Timers.Timer();
            timer.Interval = 1000f / snapshotSpeed;
            timer.Elapsed += TimerElapsed!;

            // Get server info from the API and start the server
            InitializeServer().GetAwaiter().GetResult();

            // Start a thread for reading input
            Thread inputThread = new Thread(ReadInput);
            inputThread.Start();

            Start();
            world = new GameWorld();
        }

        // Method to request server info from GameServerAPI
        private async Task InitializeServer()
        {
            try
            {
                // Step 1: Retrieve JWT token from Login API
                Console.WriteLine("Retrieving JWT token...");
                string jwt = await ReceiveJWTFromClient();
                Console.WriteLine("JWT token retrieved successfully");

                // Step 2: Get the server info (IP and port)
                Console.WriteLine("Fetching server IP and port...");
                GameServerInfo serverInfo = await GetGameServer(jwt);
                Console.WriteLine($"Server info received: {serverInfo.IP}:{serverInfo.Port}");

                // Step 3: Initialize TcpListener
                Console.WriteLine("Starting TcpListener...");
                tcpListener = new TcpListener(IPAddress.Parse(serverInfo.IP), serverInfo.Port);
                tcpListener.Start();
                Console.WriteLine($"Server started, listening on {serverInfo.IP}:{serverInfo.Port}");

                // Step 4: Start the thread to accept clients
                Thread acceptThread = new Thread(AcceptClientsThread);
                acceptThread.Start();
                Console.WriteLine("AcceptClientsThread started");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while initializing server:");
                Console.WriteLine($"Exception Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private async Task<string?> ReceiveJWTFromClient()
        {
            try
            {
                // Specify the IP address and port the server will listen on
                string serverIP = "127.0.0.1"; // Replace with your server's actual IP address
                int serverPort = 12345;        // Replace with the actual port number

                // Create a TcpListener to listen for incoming connections
                var tcpListener = new TcpListener(IPAddress.Parse(serverIP), serverPort);

                // Start the listener
                tcpListener.Start();
                Console.WriteLine($"Main Game Server listening for connections on {serverIP}:{serverPort}...");

                // Accept incoming connection from a client
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");

                // Get the network stream to read data from the client
                NetworkStream stream = client.GetStream();

                // Read the length header (first 4 bytes)
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);

                if (bytesRead == 4)
                {
                    // Convert the length header to an integer
                    int jwtLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // Prepare buffer to receive the JWT
                    byte[] jwtBuffer = new byte[jwtLength];
                    bytesRead = await stream.ReadAsync(jwtBuffer, 0, jwtBuffer.Length);

                    if (bytesRead == jwtLength)
                    {
                        // Convert JWT byte array back to string
                        string receivedJwt = Encoding.UTF8.GetString(jwtBuffer);

                        // Process the received JWT (validate it, extract claims, etc.)
                        //Console.WriteLine($"Received JWT: {receivedJwt}");

                        // Send a response back to the client
                        string responseMessage = "JWT received and processed successfully.";
                        byte[] responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                        await stream.WriteAsync(responseBuffer, 0, responseBuffer.Length);

                        Console.WriteLine("Response sent to client.");

                        // Close the client connection
                        client.Close();

                        // Return the received JWT
                        return receivedJwt;
                    }
                    else
                    {
                        Console.WriteLine("Error: JWT length does not match the expected size.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Failed to read JWT length header.");
                }

                // Close the client connection if something went wrong
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while receiving the JWT: {ex.Message}");
            }

            // Return null if there was an error
            return null;
        }

        private async Task<GameServerInfo> GetGameServer(string jwt)
        {
            // Ensure headers are cleared before setting new ones
            httpClient.DefaultRequestHeaders.Clear();

            // Log the token for debugging purposes (be careful with logging sensitive data)
            Console.WriteLine($"JWT Token used: {jwt}");

            // Add the Authorization header with the Bearer token
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

            // Log the request being sent (optional)
            Console.WriteLine("Sending request to retrieve game server info...");

            // Make the GET request to the GatewayApi endpoint
            var response = await httpClient.GetAsync("https://localhost:5000/api/api/PlayerRegisterGame/server-info");

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("RESPONSE was Success!!!!!!!!!!!!");
                // Read the response content as a string
                var result = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response to a GameServerInfo object
                var serverInfo = JsonConvert.DeserializeObject<GameServerInfo>(result);

                // Return the deserialized object, ensuring it's not null
                if (serverInfo != null)
                {
                    return serverInfo;
                }
                else
                {
                    throw new Exception("Failed to deserialize GameServer info");
                }
            }
            else
            {
                // Log and throw an error if the status code is not successful
                Console.WriteLine($"Failed to retrieve GameServer info. Status Code: {response.StatusCode}");
                throw new Exception($"Error retrieving GameServer info: {response.StatusCode}");
            }
        }


        // Method to get GameServer IP and Port from GameServerAPI
        //private async Task<GameServerInfo> GetGameServer(string jwt)
        //{
        //    // Ensure headers are cleared before setting new ones
        //    httpClient.DefaultRequestHeaders.Clear();

        //    // Log the token for debugging purposes (be careful with logging sensitive data)
        //    Console.WriteLine($"JWT Token used: {jwt}");

        //    //exempel token
        //    //string token = "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3MjY2MDk0NTR9.0K2ar8gSOKdNErx9O8UaSnGwPbE-J9wHEpNSU5pVoNRfL1p69QignWaKCPstOSfeLDV71CgXBqQ8xiA1R2zeMA";
        //    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        //    // Add the Authorization header with the Bearer token
        //    //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        //    // Log the request being sent (optional)
        //    Console.WriteLine("Sending request to retrieve game server info...");



        //    //// Make the GET request to the server-info endpoint
        //    //var response = await httpClient.GetAsync("https://localhost:7217/api/PlayerRegisterGame/server-info");
        //    // Make the GET request to the Gateway API
        //    var response = await httpClient.GetAsync("https://localhost:5000/api/api/PlayerRegisterGame/server-info");

        //    // Check if the response is successful
        //    if (response.IsSuccessStatusCode)
        //    {
        //        Console.WriteLine("RESPONSE was Success!!!!!!!!!!!!");
        //        // Read the response content as a string
        //        var result = await response.Content.ReadAsStringAsync();

        //        // Deserialize the JSON response to a GameServerInfo object
        //        var serverInfo = JsonConvert.DeserializeObject<GameServerInfo>(result);

        //        // Return the deserialized object, ensuring it's not null
        //        if (serverInfo != null)
        //        {
        //            return serverInfo;
        //        }
        //        else
        //        {
        //            throw new Exception("Failed to deserialize GameServer info");
        //        }
        //    }
        //    else
        //    {
        //        // Log and throw an error if the status code is not successful
        //        Console.WriteLine($"Failed to retrieve GameServer info. Status Code: {response.StatusCode}");
        //        throw new Exception($"Error retrieving GameServer info: {response.StatusCode}");
        //    }
        //}


        private void ReadInput()
        {
            while (true)
            {
                var input = Console.ReadLine();
                commandExecutor.ExecuteCommand(input!);
            }
        }

        private void SetTickRate(string[] args)
        {
            if (args.Length == 1 && int.TryParse(args[0], out int tickRate))
            {
                Console.WriteLine($"Setting tick rate to {tickRate}");
                snapshotSpeed = tickRate;
                timer.Interval = 1000f / snapshotSpeed;
            }
            else
            {
                Console.WriteLine("Invalid arguments for tick");
            }
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        // Accept client connections
        private void AcceptClientsThread()
        {
            while (true)
            {
                try
                {
                    // Accept new client connection
                    var tcpClient = tcpListener!.AcceptTcpClient();
                    lock (clients)
                    {
                        clients.Add(tcpClient);
                    }
                    Console.WriteLine("New client connected");

                    // Start a new thread to handle the client's communication
                    Thread clientThread = new Thread(() => HandleClient(tcpClient));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        // Handle client communication
        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Client disconnected
                        break;
                    }

                    // Process received data
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);
                    HandleMessageReceived(receivedData, client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                lock (clients)
                {
                    clients.Remove(client);
                }
                client.Close();
            }
        }

        // Sending snapshots to all connected clients
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (clients)
            {
                foreach (var client in clients)
                {
                    if (client.Connected)
                    {
                        SendDataToClient(world.GetWorldStateSnapShot(), client);
                    }
                }
            }
        }

        // Send data to a specific client
        public void SendDataToClient(NetworkMessage message, TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte messageTypeByte = message.GetMessageTypeAsByte;
                byte[] messageBytes = new byte[1024];

                // Serialize the message based on its type
                switch (message.MessageType)
                {
                    case MessageType.SnapShot:
                        //messageBytes = MessagePackSerializer.Serialize((SnapShot)message);
                        break;
                    case MessageType.JoinAnswer:
                        //messageBytes = MessagePackSerializer.Serialize((JoinAnswer)message);
                        break;
                    default:
                        break;
                }

                // Combine message type and message bytes
                byte[] combinedBytes = new byte[1 + messageBytes.Length];
                combinedBytes[0] = messageTypeByte;
                Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);

                // Send the combined data to the client
                stream.Write(combinedBytes, 0, combinedBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to client: {ex.Message}");
            }
        }

        // Handle received data
        private void HandleMessageReceived(byte[] receivedData, TcpClient client)
        {
            try
            {
                // Extract the message type from the first byte
                MessageType messageType = (MessageType)receivedData[0];
                byte[] dataToDeserialize = receivedData.Skip(1).ToArray();

                switch (messageType)
                {
                    case MessageType.MovementUpdate:
                        //MovementUpdate mov = MessagePackSerializer.Deserialize<MovementUpdate>(dataToDeserialize);
                        //world.UpdatePlayerMovement(mov);
                        break;

                    case MessageType.Join:
                        // Send JoinAnswer message to the client
                        SendDataToClient(new JoinAnswer { PlayerOwner = (clients.Count == 1) }, client);
                        break;

                    default:
                        Console.WriteLine("Unknown message type received");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

        }
    }
}
