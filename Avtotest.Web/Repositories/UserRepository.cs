using Avtotest.Web.Models;
using Microsoft.Data.Sqlite;

namespace Avtotest.Web.Repositories
{
    public class UserRepository
    {
        private const string ConnectionString = "Data Source = users.db";
        private SqliteConnection connection;
        private SqliteCommand command;

        public UserRepository()
        {
            OpenConnection();
            CreateUserTable();
        }

        public void OpenConnection()
        {
            connection = new SqliteConnection(ConnectionString);
            connection.Open();
            command = connection.CreateCommand();
        }
        public void CreateUserTable()
        {
            command.CommandText = "CREATE TABLE IF NOT EXISTS users (" +
                                          "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                                          "name TEXT," +
                                          "phone TEXT," +
                                          "password TEXT," +
                                          "image TEXT)";
            command.ExecuteNonQuery();
        }
        public void InsertUser(User user)
        {
            command.CommandText = "INSERT INTO users(name,phone,password,image) " +
                                           "VALUES (@name,@phone,@password,@image)";
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@image", user.Image);
            command.Prepare();
            command.ExecuteNonQuery();
        }
        public List<User> GetUsers()
        {
            var users = new List<User>();
            command.CommandText = "SELECT * FROM user";
            var data = command.ExecuteReader();
            while (data.Read())
            {
                var user = new User
                {
                    Index = data.GetInt32(0),
                    Name = data.GetString(1),
                    Phone = data.GetString(2),
                    Image = data.GetString(4),
                };
                users.Add(user);
            }
            return users;
        }
        public User GetUserByIndex(int index)
        {
            var user = new User();
            command.CommandText = $"SELECT * FROM user WHERE id = {index}";
            var data = command.ExecuteReader();
            while (data.Read())
            {
                user.Index = data.GetInt32(0);
                user.Name = data.GetString(1);
                user.Phone = data.GetString(2);
                user.Image = data.GetString(4);
            }
            return user;
        }
        public User GetUserByPhoneNumber(string phoneNumber)
        {
            var user = new User();

            command.CommandText = $"SELECT * FROM users WHERE phone = '{phoneNumber}'";
            var data = command.ExecuteReader();
            while (data.Read())
            {
                user.Index = data.GetInt32(0);
                user.Name = data.GetString(1);
                user.Phone = data.GetString(2);
                user.Password = data.GetString(3);
                user.Image = data.GetString(4);
            }

            return user;
        }
        public void DeleteUser(int index)
        {
            command.CommandText = $"DELETE FROM users WHERE id = {index}";
            command.ExecuteNonQuery();
        }
        public void UpdateUser(User user)
        {
            command.CommandText = "UPDATE users SET name=@name,phone=@phone,password = @password,image = @image WHERE id = @userId";
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@phone", user.Phone);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@image", user.Image);
            command.Parameters.AddWithValue("@userId", user.Index);
            command.Prepare();

            command.ExecuteNonQuery();
        }
    }
}
