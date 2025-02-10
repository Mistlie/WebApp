using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

public class DBManager
{
    private SqliteConnection? connection = null;

    private string HashPassword(string password)
    {
        using (var algorithm = SHA256.Create())
        {
            var bytes_hash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }

    public bool ConnectToDB(string path)
    {
        Console.WriteLine("Подключение к базе данных...");

        try
        {
            connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            if (connection.State != System.Data.ConnectionState.Open){
                Console.WriteLine("Не удалось подключиться");
                return false;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
            return false;
        }
        
        Console.WriteLine("Подключение выполнено");
        return true;
    }

    public void DisconnectFromDB()
    {
        if (connection?.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
            connection = null;
            Console.WriteLine("Отключение базы данных");
        }
    }

    public bool AddUser(string login, string password)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        string REQUEST = "INSERT INTO users (Login, Password) VALUES (@login, @password)";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@password", HashPassword(password));

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при добавлении пользователя: {exp.Message}");
            return false;
        }
    }

    public bool CheckUser(string login, string password)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        string REQUEST = "SELECT Login,Password FROM users WHERE Login = @login AND Password = @password";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@password", HashPassword(password));

        try
        {
            using var reader = command.ExecuteReader();
            return reader.HasRows;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке пользователя: {ex.Message}");
            return false;
        }
    }

    public bool ChangePassword(string login, string newPassword)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        string REQUEST = "UPDATE users SET Password = @password WHERE Login = @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@password", HashPassword(newPassword));

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при изменении пароля: {exp.Message}");
            return false;
        }
    }

    public bool AddText(string login, string text, string key)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        var encryptedText = VigenereCipher.Encrypt(text, key);
        var decryptedText = VigenereCipher.Decrypt(text, key);

        string REQUEST = "INSERT INTO texts (Login, Text, EncryptedText, DecryptedText, Key) VALUES  (@login, @text, @encryptedText, @decryptedText, @key)";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@text", text);
        command.Parameters.AddWithValue("@encryptedText", encryptedText);
        command.Parameters.AddWithValue("@decryptedText", decryptedText);
        command.Parameters.AddWithValue("@key", key);

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при добавлении текста: {exp.Message}");
            return false;
        }
    }

    public bool UpdateText(int id, string login, string text, string key)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        var encryptedText = VigenereCipher.Encrypt(text, key);
        var decryptedText = VigenereCipher.Decrypt(text, key);

        string REQUEST = "UPDATE texts SET Text = @text, EncryptedText = @encryptedText, DecryptedText = @decryptedText, Key = @key WHERE ID = @id AND Login = @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@text", text);
        command.Parameters.AddWithValue("@encryptedText", encryptedText);
        command.Parameters.AddWithValue("@decryptedText", decryptedText);
        command.Parameters.AddWithValue("@key", key);

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при изменении текста: {exp.Message}");
            return false;
        }
    }

    public bool DeleteText(int id, string login)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        string REQUEST = "DELETE FROM texts WHERE ID = @id AND Login= @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@login", login);

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при удалении текста: {exp.Message}");
            return false;
        }
    }

    public string EncryptText(string login, string text, string key)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return null;
        }

        var encryptedText = VigenereCipher.Encrypt(text, key);

        string REQUEST = "INSERT INTO texts (Login, Text, EncryptedText, Key) VALUES  (@login, @text, @encryptedText, @key)";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@text", text);
        command.Parameters.AddWithValue("@encryptedText", encryptedText);
        command.Parameters.AddWithValue("@key", key);

        try
        {
            if (command.ExecuteNonQuery() > 0);{
                return encryptedText;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при добавлении текста: {exp.Message}");
            return null;
        }
    }

    public string DecryptText(string login, string encryptedText, string key)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return null;
        }

        var decryptedText = VigenereCipher.Decrypt(encryptedText, key);

        string REQUEST = "INSERT INTO texts (Login, Text, DecryptedText, Key) VALUES  (@login, @text, @decryptedText, @key)";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@text", encryptedText);
        command.Parameters.AddWithValue("@decryptedText", decryptedText);
        command.Parameters.AddWithValue("@key", key);

        try
        {
            if (command.ExecuteNonQuery() > 0);{
                return decryptedText;
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при добавлении текста: {exp.Message}");
            return null;
        }
    }

    public bool AddRequestHistory(string login, string requestData)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        string REQUEST = "INSERT INTO history (Login, Request) VALUES  (@login, @request)";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);
        command.Parameters.AddWithValue("@request", requestData);

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при добавлении запроса: {exp.Message}");
            return false;
        }
    }

    public bool DeleteRequestHistory(string login)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return false; 
        }

        string REQUEST = "DELETE FROM history WHERE Login = @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);

        try
        {
            return command.ExecuteNonQuery() > 0;
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Ошибка при удалении истории: {exp.Message}");
            return false;
        }
    }

    public List<string> GetRequestHistory(string login)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return new List<string>();
        }
        string REQUEST = "SELECT Request FROM history WHERE Login = @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);

        var history = new List<string>();

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            history.Add(reader.GetString(0));
        }

        return history;
    }

    public TextResponse? GetText(int id, string login)
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return null; 
        }

        string REQUEST = "SELECT ID, Text, EncryptedText, DecryptedText, Key FROM texts WHERE ID = @id AND Login = @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@login", login);

        using var reader = command.ExecuteReader();
        return reader.Read()
            ? new TextResponse
            {
                ID = reader.GetInt32(0),
                Text = reader.GetString(1),
                EncryptedText = reader.GetString(2),
                DecryptedText = reader.GetString(3),
                Key = reader.GetString(4)
            }
            : null;
    }

    public List<TextResponse> GetAllTexts(string login)
    {
        if (connection?.State != System.Data.ConnectionState.Open)
        {
            return new List<TextResponse>();
        }

        string REQUEST = "SELECT ID, Text, EncryptedText, DecryptedText, Key FROM texts WHERE Login = @login";
        var command = new SqliteCommand(REQUEST, connection);
        command.Parameters.AddWithValue("@login", login);

        var texts = new List<TextResponse>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            texts.Add(new TextResponse
            {
                ID = reader.GetInt32(0),
                Text = reader.GetString(1),
                EncryptedText = reader.GetString(2),
                DecryptedText = reader.GetString(3),
                Key = reader.GetString(4)
            });
        }

        return texts;
    }

    public class TextResponse
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string EncryptedText { get; set; }
        public string DecryptedText { get; set; }
        public string Key { get; set; }
    }
}