using System.Net;
using System.Net.Sockets;
using MessagePack;

namespace Client
{
    public class TCPGameClient
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Action<byte[]> OnDataReceived;
        private const int BufferSize = 1024;

        public TCPGameClient(Action<byte[]> onDataReceived)
        {
            this.OnDataReceived = onDataReceived;

            tcpClient = new TcpClient();
            IPAddress serverIP = IPAddress.Parse("127.0.0.1");
            int serverPort = 1234;

            // Connect to the TCP server
            tcpClient.Connect(serverIP, serverPort);

            // Get the network stream for communication
            stream = tcpClient.GetStream();

            // Start a background thread to receive data from the server
            Thread receiveThread = new Thread(ReceiveDataFromServer);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        /// <summary>
        /// Sends data to the server using the TCP stream
        /// </summary>
        public async void SendDataToServer(NetworkMessage message)
        {
            await Task.Delay(GameWorld.LATENCY);

            byte[] messageBytes = new byte[BufferSize];
            byte messageTypeByte = message.GetMessageTypeAsByte;

            switch (message.MessageType)
            {
                case MessageType.MovementUpdate:
                    messageBytes = MessagePackSerializer.Serialize((MovementUpdate)message);
                    break;
                default:
                    break;
            }

            // Combine message type byte with serialized data
            byte[] combinedBytes = new byte[1 + messageBytes.Length];
            combinedBytes[0] = messageTypeByte;
            Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);

            // Send the combined data to the server via the TCP stream
            if (stream.CanWrite)
            {
                await stream.WriteAsync(combinedBytes, 0, combinedBytes.Length);
            }
        }

        /// <summary>
        /// Receives data from the server continuously in a loop
        /// </summary>
        private void ReceiveDataFromServer()
        {
            byte[] buffer = new byte[BufferSize];

            try
            {
                while (true)
                {
                    // Read data from the server
                    if (stream.CanRead)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            byte[] receivedData = new byte[bytesRead];
                            Buffer.BlockCopy(buffer, 0, receivedData, 0, bytesRead);

                            // Invoke the callback with the received data
                            OnDataReceived.Invoke(receivedData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving data from server: " + ex.Message);
            }
        }
    }
}
