using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

builder.Services.AddSingleton<DBManager>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

DBManager db = app.Services.GetRequiredService<DBManager>();

app.MapPost("/signup", ([FromBody] UserModel user, HttpContext context, DBManager db) => {
    if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
    {
        return Results.Problem("Пароль и логин должны быть заполнены");
    }

    if (db.AddUser(user.Login, user.Password))
    {
        db.AddRequestHistory(user.Login, " Успешно зарегистрирован");
        return Results.Ok($"Пользователь {user.Login} успешно зарегистрирован");
    }
    else
    {
        return Results.Problem($"Ошибка регистрации пользователя: {user.Login}");
    }
});

app.MapPost("/login", async ([FromBody] UserModel user, HttpContext context, DBManager db) => {
    if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
    {
        return Results.Problem("Логин и пароль должны быть заполнены");
    }

    if (!db.CheckUser(user.Login, user.Password))
    {
        return Results.Unauthorized();
    }
    
    var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Login) };
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    
    db.AddRequestHistory(user.Login, " Успешно авторизовался");
    return Results.Ok();
});

app.MapGet("/check_auth", [Authorize] (HttpContext context) =>
{
    return Results.Ok();
});

app.MapPatch("/changepassword", [Authorize] async ([FromBody] UserModel user, HttpContext context, DBManager db) => {
    if (string.IsNullOrEmpty(user.newPassword))
    {
        return Results.Problem("Пароль должен быть заполнен");
    }

    if (db.ChangePassword(context.User.Identity.Name, user.newPassword))
    {
        db.AddRequestHistory(context.User.Identity.Name, "Изменение пароля");
        return Results.Ok("Пользователь " + context.User.Identity.Name + " успешно сменил пароль");
    }

    else
    {
        return Results.Problem("Ошибка смены пароля пользователя: " + context.User.Identity.Name);
    }
});

app.MapPost("/texts", [Authorize] ([FromBody] UserModel user, HttpContext context, DBManager db) => {
    if (string.IsNullOrEmpty(user.Text) || string.IsNullOrEmpty(user.Key))
    {
        return Results.Problem("Текст и ключ должны быть заполнены");
    }

    if (db.AddText(context.User.Identity.Name, user.Text, user.Key))
    {
        db.AddRequestHistory(context.User.Identity.Name, "Добавление текста");
        return Results.Ok("Пользователь " + context.User.Identity.Name + " добавил текст");
    }

    else
    {
        return Results.Problem("Ошибка добавления текста пользователем: " + context.User.Identity.Name);
    }
});

app.MapPatch("/texts", [Authorize] ([FromBody] UserModel user, HttpContext context, DBManager db) => {
   if (string.IsNullOrEmpty(user.Text) || string.IsNullOrEmpty(user.Key))
    {
        return Results.Problem("Текст и ключ должны быть заполнены");
    }

    if (db.UpdateText(user.Id, context.User.Identity.Name, user.Text, user.Key))
    {
        db.AddRequestHistory(context.User.Identity.Name, "Изменение текста номер " + user.Id);
        return Results.Ok("Текст под номером " + user.Id + " успешно изменён");
    }

    else
    {
        return Results.Problem("Ошибка изменения текста под номером " + user.Id);
    }
});

app.MapDelete("/texts/{id}", [Authorize] (int id, HttpContext context, DBManager db) => {
    if (db.DeleteText(id, context.User.Identity.Name))
    {
        db.AddRequestHistory(context.User.Identity.Name, "Удаление текста номер " + id);
        return Results.Ok("Текст под номером " + id + " успешно удалён");
    }

    else
    {
        return Results.Problem("Ошибка удаления текста под номером " + id);
    }
});

app.MapPost("/texts/encrypt", [Authorize] ([FromBody] UserModel user, HttpContext context, DBManager db) =>
{
    if (string.IsNullOrEmpty(context.User.Identity.Name) || string.IsNullOrEmpty(user.Text) || string.IsNullOrEmpty(user.Key))
    {
        return Results.BadRequest("Логин, текст и ключ должны быть заполнены");
    }

    var encryptedText = db.EncryptText(context.User.Identity.Name, user.Text, user.Key);

    if (encryptedText != null)
    {
        db.AddRequestHistory(context.User.Identity.Name, "Текст зашифрован");
        return Results.Ok(new { EncryptedText = encryptedText });
    }

    else
    {
        return Results.Problem("Ошибка шифрования текста пользователем: " + context.User.Identity.Name);
    }
    
});

app.MapPost("/texts/decrypt", [Authorize] ([FromBody]UserModel user, HttpContext context, DBManager db) =>
{
    if (string.IsNullOrEmpty(context.User.Identity.Name) || string.IsNullOrEmpty(user.EncryptedText) || string.IsNullOrEmpty(user.Key))
    {
        return Results.BadRequest("Логин, зашифрованный текст и ключ должны быть заполнены");
    }

    var decryptedText = db.DecryptText(context.User.Identity.Name, user.EncryptedText, user.Key);

    if (decryptedText != null)
    {
        db.AddRequestHistory(context.User.Identity.Name, "Текст дешифрован");
        return Results.Ok(new { DecryptedText = decryptedText });
    }

    else
    {
        return Results.Problem("Ошибка дешифрования текста пользователем: " + context.User.Identity.Name);
    }
});

app.MapGet("/texts/{id}", [Authorize] (int id, HttpContext context, DBManager db) =>
{
    var text = db.GetText(id, context.User.Identity.Name);
    if (text == null)
    {
        return Results.NotFound();
    }

    db.AddRequestHistory(context.User.Identity.Name, "Просмотр текста номер " + id);
    return Results.Ok(text);
});

app.MapGet("/all_texts", [Authorize] (HttpContext context, DBManager db) =>
{
    var texts = db.GetAllTexts(context.User.Identity.Name);
    if (texts == null)
    {
        return Results.NotFound();
    }

    db.AddRequestHistory(context.User.Identity.Name, "Просмотр всех текстов");
    return Results.Ok(texts);
});

app.MapGet("/history", [Authorize] (HttpContext context, DBManager db) =>
{
    var history = db.GetRequestHistory(context.User.Identity.Name);
    if (history == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(history);
});

app.MapDelete("/history", [Authorize] (HttpContext context, DBManager db) =>
{
    if (db.DeleteRequestHistory(context.User.Identity.Name))
    {
        return Results.Ok("История успешно очищена");
    }
    
    else
    {
        return Results.Problem("Ошибка просмотра истории пользователя: " + context.User.Identity.Name);
    }
});

const string DB_PATH = "/home/ubuntu/WebApp/users.db";
if(!db.ConnectToDB(DB_PATH)){
    Console.WriteLine("Ошибка подключения к базе данных " + DB_PATH);
    Console.WriteLine("Выключение");
    return;
}

app.Run();

public class UserModel
{
    public string Login { get; set; }
    public string Password { get; set; }
    public string newPassword { get; set; }
    public string Text { get; set; }
    public string Key{ get; set; }
    public int Id{ get; set; }
    public string EncryptedText{ get; set; }
    public string DecryptedText{ get; set; }
}
