using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


//Helper class for AES encryption and decryption for both client and server
public class AESEncryption
{
    private readonly byte[] Key;
    private readonly byte[] IV;

    public AESEncryption(string keyStr, string ivStr)  //constructor to initialize Key and IV
    {
        Key = Encoding.UTF8.GetBytes(keyStr.PadRight(32).Substring(0, 32));
        IV = Encoding.UTF8.GetBytes(ivStr.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)   //getting plaintext and returning cipher text
    {
        using var aes = Aes.Create();
        aes.Key = Key; aes.IV = IV;
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(plainText);
        sw.Close(); // Ensure all data is flushed to the CryptoStream
        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText) //getting cipher text and returning plaintext
    {
        using var aes = Aes.Create();
        aes.Key = Key; aes.IV = IV;
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}