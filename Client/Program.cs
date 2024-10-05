using Client;
using Client.Models;
using System.Net.Sockets;
using System.Text;


var gameClient = new GameClient();
var user = new User();
var game = new GameWorld();


Console.WriteLine("Welcome! You need to register or login to access the game.");
Console.WriteLine("Available commands: 'login', 'register', 'connect', 'exit'");

bool isPlaying = true;
string jwtToken = null!;

await MainLoop();

async Task MainLoop()
{
    while (isPlaying)
    {
        var input = Console.ReadLine()?.ToLower();
        if (input == null) continue;

        switch (input)
        {
            case "login":
                await LoginAsync();

                if (!string.IsNullOrEmpty(jwtToken))
                {
                    await SentJWTToMainGameServer(jwtToken);
                }
                else
                {
                    Console.WriteLine("You need to register first.");
                }
                break;

            case "register":
                await RegisterAsync();
                break;

            case "connect":
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    await ConnectToGameServer();
                    game.Run();
                }
                else
                {
                    Console.WriteLine("You need to login first.");
                }
                break;

            case "exit":
                Console.WriteLine("Goodbye! Thanks for playing.");
                isPlaying = false;
                break;

            default:
                Console.WriteLine("Invalid command. Please enter 'login', 'register', 'connect', or 'exit'.");
                break;
        }
    }
}

async Task LoginAsync()
{
    Console.WriteLine("Enter your username:");
    user.Name = Console.ReadLine()!;

    Console.WriteLine("Enter your password:");
    user.Password = Console.ReadLine()!;

    if (!string.IsNullOrEmpty(user.Name) && !string.IsNullOrEmpty(user.Password))
    {
        try
        {
            jwtToken = await gameClient.Login(user);

            if (!string.IsNullOrEmpty(jwtToken))  // Check if a token is returned
            {
                Console.WriteLine("Login successful! You are now logged in.");
                await PromptForConnect(); // Ask to connect to game server
            }
            else
            {
                Console.WriteLine("Login failed. Invalid username or password.");
            }
        }
        catch (LoginException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}


async Task RegisterAsync()
{
    Console.WriteLine("Choose a username:");
    user.Name = Console.ReadLine()!;

    Console.WriteLine("Choose a password:");
    user.Password = Console.ReadLine()!;

    if (user.Name != null && user.Password != null)
    {
        try
        {
            var registrationResponse = await gameClient.HandlePostRegister(user);
            Console.WriteLine("Registration successful!");

            // Automatically log the user in after successful registration
            jwtToken = await gameClient.Login(user);

            if (!string.IsNullOrEmpty(jwtToken))
            {
                Console.WriteLine("Login successful! You are now logged in.");
                await PromptForConnect(); // Ask to connect to game server
            }
        }
        catch (RegistrationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (LoginException ex)
        {
            Console.WriteLine("Login failed after registration. Please try logging in manually.");
            Console.WriteLine(ex.Message);
        }
    }
}

async Task PromptForConnect()
{
    bool isValidInput = false;

    while (!isValidInput)
    {
        Console.WriteLine("Do you want to connect to the game server? (yes/no)");
        var connectInput = Console.ReadLine()?.ToLower();

        if (connectInput == "yes" && !string.IsNullOrEmpty(jwtToken))
        {
            //Console.WriteLine($"jwtToken from PromptForConnect: {jwtToken}");

            await ConnectToGameServer();
            isValidInput = true; // Exit loop after handling the "yes" case

        }
        else if (connectInput == "no")
        {
            Console.WriteLine("You can use the 'connect' command later if you change your mind.");
            Console.WriteLine("You need to write 'exit' to leave the game server.");
            isValidInput = true; // Exit loop after handling the "no" case
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter 'yes' or 'no'.");
        }
    }
}

async Task SentJWTToMainGameServer(string jwtToken)
{
    try
    {
        // Specify the game server's IP and Port
        string gameServerIP = "127.0.0.1"; // Replace with actual game server IP
        int gameServerPort = 12345;        // Replace with actual game server port

        // Establish TCP connection to the game server
        using (var tcpClient = new TcpClient(gameServerIP, gameServerPort))
        {
            Console.WriteLine($"Connecting to GameServer at {gameServerIP}:{gameServerPort}...");

            // Get the stream to send data
            NetworkStream stream = tcpClient.GetStream();

            // Convert the JWT to bytes
            var jwtBytes = Encoding.UTF8.GetBytes(jwtToken);

            // Optionally, you can add a header (like the length of the message)
            var message = new byte[jwtBytes.Length + 4]; // 4 bytes for length header

            // Add the length of the JWT as a header (optional)
            BitConverter.GetBytes(jwtBytes.Length).CopyTo(message, 0);

            // Copy the JWT bytes into the message array
            jwtBytes.CopyTo(message, 4);

            // Send the message (JWT with header)
            await stream.WriteAsync(message, 0, message.Length);

            Console.WriteLine("JWT sent to the GameServer. Waiting for server response...");

            // Buffer to read response from the server
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Received response from server: {serverResponse}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to send JWT to GameServer: {ex.Message}");
    }
}


async Task ConnectToGameServer()
{
    try
    {
        var serverInfo = await gameClient.GetGameServer(jwtToken);

        Console.WriteLine($"Received Server Info: IP = {serverInfo.IP}, Port = {serverInfo.Port}");
        Console.WriteLine($"Connecting to GameServer at {serverInfo.IP}:{serverInfo.Port}");

        var success = await GameClient.ConnectToGameServer(serverInfo.IP, serverInfo.Port);
        if (success)
        {
            Console.WriteLine("Connected to GameServer! You can now play the game.");
        }
        else
        {
            Console.WriteLine("Failed to connect to the GameServer.");
        }
    }
    catch (ServerConnectionException ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine("You can write 'exit' to leave the game server.");
    }
}

Console.ReadKey();
