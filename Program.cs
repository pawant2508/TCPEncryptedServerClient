using System.Net;
using System.Net.Sockets;
using System.Text;
class TcpServer
{
    static Dictionary<string, Dictionary<string, int>> collections = new Dictionary<string, Dictionary<string, int>>()   //dictionary to hold subsets
    {
        { "SetA", new Dictionary<string, int> { { "One", 1 }, { "Two", 2 } } },
        { "SetB", new Dictionary<string, int> { { "Three", 3 }, { "Four", 4 } } },
        { "SetC", new Dictionary<string, int> { { "Five", 5 }, { "Six", 6 } } },
        { "SetD", new Dictionary<string, int> { { "Seven", 7 }, { "Eight", 8 } } },
        { "SetE", new Dictionary<string, int> { { "Nine", 9 }, { "Ten", 10 } } }
    };

    static AESEncryption aes;           // AES encryption instance

    public static void Main(string[] args)
    {
        Console.Write("Enter server IP (default 127.0.0.1): ");
        string ip = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

        Console.Write("Enter server port (default 5555): ");
        string portStr = Console.ReadLine();
        int port = int.TryParse(portStr, out int p) ? p : 5555;

        Console.Write("Enter AES key (min 16 chars): ");
        string key = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(key)) key = "defaultkey123456";

        Console.Write("Enter AES IV (min 8 chars): ");
        string iv = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(iv)) iv = "defaultiv123456";

        aes = new AESEncryption(key, iv);

        var server = new TcpListener(IPAddress.Parse(ip), port);
        server.Start();
        Console.WriteLine($"Server listening on {ip}:{port}");

        while (true)        // Accept incoming client connections
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected.");
            Thread thread = new Thread(() => HandleClient(client));
            thread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            string encryptedMsg = reader.ReadLine();
            string request = aes.Decrypt(encryptedMsg);

            Console.WriteLine($"Received: {request}");

            var parts = request.Split('-');
            if (parts.Length == 2 && collections.TryGetValue(parts[0], out var subset) && subset.TryGetValue(parts[1], out int n))
            {
                for (int i = 0; i < n; i++)
                {
                    string timeMsg = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                    string encryptedTime = aes.Encrypt(timeMsg);
                    writer.WriteLine(encryptedTime);
                    Thread.Sleep(1000);
                }
            }
            else
            {
                string encryptedEmpty = aes.Encrypt("EMPTY");
                writer.WriteLine(encryptedEmpty);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}