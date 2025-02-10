using System;
using System.Text;

public static class VigenereCipher
{
    public static string Encrypt(string text, string key)
    {
        var encryptedText = new StringBuilder();
        key = key.ToUpper();
        int keyIndex = 0;
        
        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];
            if (char.IsLetter(currentChar))
            {
                char baseChar = char.IsUpper(currentChar) ? 'A' : 'a';
                int shift = key[keyIndex % key.Length] - 'A';
                char encryptedChar = (char)((currentChar - baseChar + shift) % 26 + baseChar);
                encryptedText.Append(encryptedChar);
                keyIndex++;
            }
            else
            {
                encryptedText.Append(currentChar);
            }
        }
        return encryptedText.ToString();
    }

    public static string Decrypt(string encryptedText, string key)
    {
        var decryptedText = new StringBuilder();
        key = key.ToUpper();
        int keyIndex = 0;
        
        for (int i = 0; i < encryptedText.Length; i++)
        {
            char currentChar = encryptedText[i];
            if (char.IsLetter(currentChar))
            {
                char baseChar = char.IsUpper(currentChar) ? 'A' : 'a';
                int shift = key[keyIndex % key.Length] - 'A';
                char decryptedChar = (char)((currentChar - baseChar - shift + 26) % 26 + baseChar);
                decryptedText.Append(decryptedChar);
                keyIndex++;
            }
            else
            {
                decryptedText.Append(currentChar);
            }
        }
        return decryptedText.ToString();
    }
}