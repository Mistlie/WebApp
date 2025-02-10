using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private const string DEFAULT_SERVER_URL = "http://localhost:5064";
    private static CookieContainer cookies = new CookieContainer();
    private static HttpClientHandler handler = new HttpClientHandler { CookieContainer = cookies };
    private static HttpClient client = new HttpClient(handler);
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static async Task Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Регистрация");
            Console.WriteLine("2. Авторизация");
            Console.WriteLine("3. Смена пароля");
            Console.WriteLine("4. Добавить текст");
            Console.WriteLine("5. Обновить текст");
            Console.WriteLine("6. Удалить текст");
            Console.WriteLine("7. Зашифровать текст");
            Console.WriteLine("8. Расшифровать текст");
            Console.WriteLine("9. Текст по ID");
            Console.WriteLine("10. Все тексты");
            Console.WriteLine("11. История");
            Console.WriteLine("12. Очистить историю");
            Console.WriteLine("13. Выход");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await SignUp();
                    break;
                case "2":
                    await LoginOnServer();
                    break;
                case "3":
                    await ChangePassword();
                    break;
                case "4":
                    await AddText();
                    break;
                case "5":
                    await UpdateText();
                    break;    
                case "6":
                    await DeleteText();
                    break;
                case "7":
                    await EncryptText();
                    break;
                case "8":
                    await DecryptText();
                    break;
                case "9":
                    await GetText();
                    break;
                case "10":
                    await GetAllTexts();
                    break;
                case "11":
                    await GetRequestHistory();
                    break;
                case "12":
                    await DeleteRequestHistory();
                    break;
                case "13":
                    return;
                default:
                    Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    private static async Task SignUp()
    {
        if (await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы уже авторизованы. Сначала выполните выход.");
            return;
        }

        Console.Write("Введите имя пользователя: ");
        var login = Console.ReadLine();

        Console.Write("Введите пароль: ");
        var password = Console.ReadLine();

        var json_data = new { Login = login, Password = password };
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{DEFAULT_SERVER_URL}/signup", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Регистрация прошла успешно");
            foreach (Cookie cookie in cookies.GetCookies(new Uri(DEFAULT_SERVER_URL)))
            {
                Console.WriteLine(cookie.Name + ": " + cookie.Value);
            }
        }
        else
        {
            Console.WriteLine("Регистрация провалена: " + responseContent);
        }
    }

    private static async Task LoginOnServer()
    {
        if (await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы уже авторизованы. Сначала выполните выход.");
            return;
        }

        Console.Write("Введите имя пользователя: ");
        var login = Console.ReadLine();

        Console.Write("Введите пароль: ");
        var password = Console.ReadLine();

        var json_data = new { Login = login, Password = password };
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{DEFAULT_SERVER_URL}/login", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Авторизация прошла успешно");
            foreach (Cookie cookie in cookies.GetCookies(new Uri(DEFAULT_SERVER_URL)))
            {
                Console.WriteLine(cookie.Name + ": " + cookie.Value);
            }
        }
        else
        {
            Console.WriteLine("Авторизация провалена: " + responseContent);
        }
    }

    private static async Task<bool> IsUserAuthenticated()
    {
        var response = await client.GetAsync($"{DEFAULT_SERVER_URL}/check_auth");
        return response.IsSuccessStatusCode;
    }


    private static async Task ChangePassword()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите новый пароль: ");
        var newpassword = Console.ReadLine();

        var json_data = new { newPassword = newpassword };
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PatchAsync($"{DEFAULT_SERVER_URL}/changepassword", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Пароль успешно изменен");
            foreach (Cookie cookie in cookies.GetCookies(new Uri(DEFAULT_SERVER_URL)))
            {
                Console.WriteLine(cookie.Name + ": " + cookie.Value);
            }
        }
        else
        {
            Console.WriteLine("Ошибка смены пароля: " + responseContent);
        }
    }

    private static async Task AddText()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите текст (только латинские буквы): ");
        var text = Console.ReadLine();

        if (!IsLatinOnly(text))
        {
            Console.WriteLine("Ошибка: Текст должен содержать только латинские буквы.");
            return;
        }

        Console.Write("Введите ключ (только латинские буквы): ");
        var key = Console.ReadLine();

        if (!IsLatinOnly(key))
        {
            Console.WriteLine("Ошибка: Ключ должен содержать только латинские буквы.");
            return;
        }

        var json_data = new { Text = text, Key = key};
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{DEFAULT_SERVER_URL}/texts", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст успешно добавлен");
        }
        else
        {
            Console.WriteLine("Ошибка добавления текста: " + responseContent);
        }
    }

    private static async Task UpdateText()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        var id = Console.ReadLine();
        Console.Write("Введите текст (только латинские буквы): ");
        var text = Console.ReadLine();

        if (!IsLatinOnly(text))
        {
            Console.WriteLine("Ошибка: Текст должен содержать только латинские буквы.");
            return;
        }

        Console.Write("Введите ключ (только латинские буквы): ");
        var key = Console.ReadLine();

         if (!IsLatinOnly(key))
        {
            Console.WriteLine("Ошибка: Ключ должен содержать только латинские буквы.");
            return;
        }

        var json_data = new { Text = text, Key = key, Id = id };
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PatchAsync($"{DEFAULT_SERVER_URL}/texts", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст и ключ успешно изменены");
        }
        else
        {
            Console.WriteLine("Ошибка изменения текста: " + responseContent);
        }
    }

    private static async Task DeleteText()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        var id = Console.ReadLine();

        var response = await client.DeleteAsync($"{DEFAULT_SERVER_URL}/texts/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Текст и ключ успешно удалены");
        }
        else
        {
            Console.WriteLine("Ошибка удаления текста: " + responseContent);
        }
    }

    private static async Task EncryptText()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите текст для шифрования (только латинские буквы): ");
        var text = Console.ReadLine();

        if (!IsLatinOnly(text))
        {
            Console.WriteLine("Ошибка: Текст должен содержать только латинские буквы.");
            return;
        }

        Console.Write("Введите ключ (только латинские буквы): ");
        var key = Console.ReadLine();

        if (!IsLatinOnly(key))
        {
            Console.WriteLine("Ошибка: Ключ должен содержать только латинские буквы.");
            return;
        }

        var json_data = new { Text = text, Key = key};
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{DEFAULT_SERVER_URL}/texts/encrypt", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var encryptedTextResponse = JsonSerializer.Deserialize<EncryptedTextResponse>(responseContent, jsonOptions);
            Console.WriteLine($"Зашифрованный текст: {encryptedTextResponse?.encryptedText}");
        }
        else
        {
            Console.WriteLine("Ошибка шифрования текста: " + responseContent);
        }
    }

    private static async Task DecryptText()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите текст для дешифрования (только латинские буквы): ");
        var encryptedtext = Console.ReadLine();

        if (!IsLatinOnly(encryptedtext))
        {
            Console.WriteLine("Ошибка: Текст должен содержать только латинские буквы.");
            return;
        }

        Console.Write("Введите ключ (только латинские буквы): ");
        var key = Console.ReadLine();

        if (!IsLatinOnly(key))
        {
            Console.WriteLine("Ошибка: Ключ должен содержать только латинские буквы.");
            return;
        }

        var json_data = new { EncryptedText = encryptedtext, Key = key};
        var jsonBody = JsonSerializer.Serialize(json_data);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"{DEFAULT_SERVER_URL}/texts/decrypt", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var decryptedTextResponse = JsonSerializer.Deserialize<DecryptedTextResponse>(responseContent, jsonOptions);
            Console.WriteLine($"Расшифрованный текст: {decryptedTextResponse?.decryptedText}");
        }
        else
        {
            Console.WriteLine("Ошибка дешифрования текста: " + responseContent);
        }
    }

    private static async Task GetText()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        Console.Write("Введите ID текста: ");
        var id = Console.ReadLine();

        var response = await client.GetAsync($"{DEFAULT_SERVER_URL}/texts/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var text = JsonSerializer.Deserialize<TextResponse>(responseContent);
            Console.WriteLine($"Оригинальный текст: {text?.text}");
            Console.WriteLine($"Зашифрованный текст: {text?.encryptedText}");
            Console.WriteLine($"Дешифрованный текст: {text?.decryptedText}");
            Console.WriteLine($"Ключ: {text?.key}");
        }
        else
        {
            Console.WriteLine("Ошибка вывода текста: " + responseContent);
        }
    }

    private static async Task GetAllTexts()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        var response = await client.GetAsync($"{DEFAULT_SERVER_URL}/all_texts");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var texts = JsonSerializer.Deserialize<List<TextResponse>>(responseContent);
            Console.WriteLine("Все тексты:");
            foreach (var text in texts)
            {
                Console.WriteLine($"ID: {text.id}");
                Console.WriteLine($"Оригинальный текст: {text.text}");
                Console.WriteLine($"Зашифрованный текст: {text.encryptedText}");
                Console.WriteLine($"Дешифрованный текст: {text.decryptedText}");
                Console.WriteLine($"Ключ: {text.key}");
            }
        }
        else
        {
            Console.WriteLine("Ошибка вывода текстов: " + responseContent);
        }
    }

    private static async Task GetRequestHistory()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }

        var response = await client.GetAsync($"{DEFAULT_SERVER_URL}/history");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var history = JsonSerializer.Deserialize<List<string>>(responseContent);
            Console.WriteLine("История запросов:");
            foreach (var item in history)
            {
                Console.WriteLine(item);
            }
        }
        else
        {
            Console.WriteLine($"Ошибка: {responseContent}");
        }
    }
    private static async Task DeleteRequestHistory()
    {
        if (!await IsUserAuthenticated())
        {
            Console.WriteLine("Ошибка: Вы не авторизованы. Сначала выполните вход.");
            return;
        }
        
        var response = await client.DeleteAsync($"{DEFAULT_SERVER_URL}/history");
        var responseContent = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("История очищена");
        }
        else
        {
            Console.WriteLine($"Ошибка: {responseContent}");
        }
    }

    private static bool IsLatinOnly(string input)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z\p{P}\s]+$");
    }

    private class TextResponse
    {
        public int id { get; set; }
        public string text { get; set; }
        public string encryptedText { get; set; }
        public string decryptedText { get; set; }
        public string key { get; set; }
    }

    private class EncryptedTextResponse
    {
        public string encryptedText { get; set; } = string.Empty;
    }

     private class DecryptedTextResponse
    {
        public string decryptedText { get; set; } = string.Empty;
    }
}