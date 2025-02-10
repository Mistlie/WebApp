using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class ServerTests
{
    private HttpClient _client;
    private CookieContainer _cookieContainer;
    private HttpClientHandler _handler;
    
    [SetUp]
    public void Setup()
    {
        _cookieContainer = new CookieContainer();
        _handler = new HttpClientHandler { CookieContainer = _cookieContainer, UseCookies = true, AllowAutoRedirect = true };
        _client = new HttpClient(_handler) { BaseAddress = new Uri("http://localhost:5064") };
    }
    
    [Test]
    public async Task Signup_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/signup", content);
        response.EnsureSuccessStatusCode();
    }
    
    [Test]
    public async Task Login_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");
        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);
    }
    
    [Test]
    public async Task ChangePassword_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);
        
        var newPassword = "newpass123";
        var changepasswordContent = new StringContent(JsonSerializer.Serialize(new { newPassword }), Encoding.UTF8, "application/json");
        var response = await _client.PatchAsync("/changepassword", changepasswordContent);
        response.EnsureSuccessStatusCode();
    }
    
    [Test]
    public async Task AddText_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var text = new {Text = "test text", Key = "test key"};
        var textContent = new StringContent(JsonSerializer.Serialize(text), Encoding.UTF8, "application/json");

        var textaddResponse = await _client.PostAsync("/texts", textContent);
        textaddResponse.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task UpdateText_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var text = new {Text = "test text", Key = "test key"};
        var textContent = new StringContent(JsonSerializer.Serialize(text), Encoding.UTF8, "application/json");

        var textaddResponse = await _client.PostAsync("/texts", textContent);
        textaddResponse.EnsureSuccessStatusCode();

        var getAllTextsResponse = await _client.GetAsync("/all_texts");
        getAllTextsResponse.EnsureSuccessStatusCode();
        var getAllTextsResult = await getAllTextsResponse.Content.ReadAsStringAsync();

        var texts = JsonSerializer.Deserialize<List<TextResponseModel>>(getAllTextsResult);
        var textId = texts!.First().id;
        
        var updatedtext = new {Id = textId, Text = "test update text", Key = "test update key"};
        var updatedtextContent = new StringContent(JsonSerializer.Serialize(updatedtext), Encoding.UTF8, "application/json");

        var updateResponse = await _client.PatchAsync($"/texts", updatedtextContent);
        updateResponse.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task DeleteText_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var text = new {Text = "test text", Key = "test key"};
        var textContent = new StringContent(JsonSerializer.Serialize(text), Encoding.UTF8, "application/json");

        var textaddResponse = await _client.PostAsync("/texts", textContent);
        textaddResponse.EnsureSuccessStatusCode();

        var getAllTextsResponse = await _client.GetAsync("/all_texts");
        getAllTextsResponse.EnsureSuccessStatusCode();
        var getAllTextsResult = await getAllTextsResponse.Content.ReadAsStringAsync();

        var texts = JsonSerializer.Deserialize<List<TextResponseModel>>(getAllTextsResult);
        var textId = texts!.First().id;
        
        var deleteResponse = await _client.DeleteAsync($"/texts/{textId}");
        deleteResponse.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task EncryptText_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var encryptText = new {Text = "test encrypt text", Key = "test key"};
        var ecnryptTextContent = new StringContent(JsonSerializer.Serialize(encryptText), Encoding.UTF8, "application/json");
        var encryptResponse = await _client.PostAsync("/texts/encrypt", ecnryptTextContent);
        encryptResponse.EnsureSuccessStatusCode();
        var encryptResponseString = await encryptResponse.Content.ReadAsStringAsync();
        Assert.IsNotNull(encryptResponseString);
    }

    [Test]
    public async Task DecryptText_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var decryptText = new {EncryptedText = "test decrypt text", Key = "test key"};
        var decryptTextContent = new StringContent(JsonSerializer.Serialize(decryptText), Encoding.UTF8, "application/json");
        var decryptResponse = await _client.PostAsync("/texts/decrypt", decryptTextContent);
        decryptResponse.EnsureSuccessStatusCode();
        var decryptResponseString = await decryptResponse.Content.ReadAsStringAsync();
        Assert.IsNotNull(decryptResponseString);
    }
    
    [Test]
    public async Task GetTextById_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var text = new {Text = "test text", Key = "test key"};
        var textContent = new StringContent(JsonSerializer.Serialize(text), Encoding.UTF8, "application/json");
        var textaddResponse = await _client.PostAsync("/texts", textContent);
        textaddResponse.EnsureSuccessStatusCode();

        var getAllTextsResponse = await _client.GetAsync("/all_texts");
        getAllTextsResponse.EnsureSuccessStatusCode();
        var getAllTextsResult = await getAllTextsResponse.Content.ReadAsStringAsync();
        var texts = JsonSerializer.Deserialize<List<TextResponseModel>>(getAllTextsResult);
        var textId = texts!.First().id;

        var getTextIdResponse = await _client.GetAsync($"/texts/{textId}");
        getTextIdResponse.EnsureSuccessStatusCode();
    }
    
    [Test]
    public async Task GetAllTexts_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var text1 = new {Text = "test text1", Key = "test key1"};
        var textContent1 = new StringContent(JsonSerializer.Serialize(text1), Encoding.UTF8, "application/json");
        var textaddResponse1 = await _client.PostAsync("/texts", textContent1);
        textaddResponse1.EnsureSuccessStatusCode();

        var text2 = new {Text = "test text2", Key = "test key2"};
        var textContent2 = new StringContent(JsonSerializer.Serialize(text2), Encoding.UTF8, "application/json");
        var textaddResponse2 = await _client.PostAsync("/texts", textContent2);
        textaddResponse2.EnsureSuccessStatusCode();

        var getAllTextsResponse = await _client.GetAsync("/all_texts");
        getAllTextsResponse.EnsureSuccessStatusCode();
        var AllTextsResponseString = await getAllTextsResponse.Content.ReadAsStringAsync();
        var texts = JsonSerializer.Deserialize<List<TextResponseModel>>(AllTextsResponseString);
        Assert.AreEqual(2, texts.Count);
    }
    
    [Test]
    public async Task GetHistory_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);

        var getHistoryResponse = await _client.GetAsync("/history");
        getHistoryResponse.EnsureSuccessStatusCode();
        var getHistoryResult = await getHistoryResponse.Content.ReadAsStringAsync();
        Assert.IsNotNull(getHistoryResult);
    }
    
    [Test]
    public async Task ClearHistory_Test()
    {
        var username = $"testuser_{Guid.NewGuid()}";
        var password = "password123";
        var content = new StringContent(JsonSerializer.Serialize(new { Login = username, Password = password }), Encoding.UTF8, "application/json");

        var RegisterResponse = await _client.PostAsync("/signup", content);
        RegisterResponse.EnsureSuccessStatusCode();

        var LoginResponse = await _client.PostAsync("/login", content);
        LoginResponse.EnsureSuccessStatusCode();
        var cookies = _cookieContainer.GetCookies(new Uri("http://localhost:5064"));
        Assert.IsNotNull(cookies[".AspNetCore.Cookies"]);
        
        var deleteHistoryResponse = await _client.DeleteAsync("/history");
        deleteHistoryResponse.EnsureSuccessStatusCode();
    }

    private class TextResponseModel
    {
        public int id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }
}
