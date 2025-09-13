using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

class TcpClientApp
{
    static AESEncryption aes;

    public static void Main(string[] args)
    {
        Console.Write("Enter Server IP (default 127.0.0.1): ");
        string ip = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

        Console.Write("Enter Server Port (default 5555): ");
        string portStr = Console.ReadLine();
        int port = int.TryParse(portStr, out int p) ? p : 5555;

        Console.Write("Enter AES key (min 16 chars): ");
        string key = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(key)) key = "defaultkey123456";

        Console.Write("Enter AES IV (min 8 chars): ");
        string iv = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(iv)) iv = "defaultiv123456";

        aes = new AESEncryption(key, iv);

        using var client = new TcpClient(ip, port);
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        Console.Write("Enter request string (e.g. SetA-One): ");
        string request = Console.ReadLine();
        string encryptedMsg = aes.Encrypt(request);
        writer.WriteLine(encryptedMsg);

        while (true)
        {
            string encryptedResponse = reader.ReadLine();
            if (string.IsNullOrEmpty(encryptedResponse)) break;
            string response = aes.Decrypt(encryptedResponse);
            Console.WriteLine($"Received: {response}");
            if (response == "EMPTY") break;
        }
    }
}