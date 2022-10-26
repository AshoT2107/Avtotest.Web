using Avtotest.Web.Models;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace Avtotest.Web.Repositories
{
    public class QuestionsRepository
    {
        private readonly string connectionString = "Data Source = Avtotest.db";
        private SqliteConnection connection;
        private bool isFull = false;
        public QuestionsRepository()
        {
            connection = new SqliteConnection(connectionString);
            CreateQuestionsAndChoicesTable();
            if(GetQuestionsCount() == 0)
            {
                AddQuestionToDatabase();
            }
            
        }
        private void CreateQuestionsAndChoicesTable()
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS questions(" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "question TEXT," +
                "description TEXT," +
                "image TEXT)";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE TABLE IF NOT EXISTS choices(" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "text TEXT," +
                "answer BOOLEN," +
                "questionId INTEGER)";
            command.ExecuteNonQuery();

            connection.Close();
        }
        private void AddQuestionToDatabase()
        {
            var questions = ConvertQuestionToJson();
            foreach(var question in questions)
            {
                AddQuestion(question);
            }
        }


        public List<QuestionEntity> ConvertQuestionToJson()
        {
            var json = File.ReadAllText("JsonData/uzlotin.json");
            var questions = JsonConvert.DeserializeObject<List<QuestionEntity>>(json);
            return questions!;
        }

        private void AddQuestion(QuestionEntity question)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            if (question.Media!.Name == null)
            {
                cmd.CommandText = "INSERT INTO questions(id,question,description)" +
                    "VALUES (@id,@question,@description)";
                cmd.Parameters.AddWithValue("@id", question.Id);
                cmd.Parameters.AddWithValue("@question", question.Question);
                cmd.Parameters.AddWithValue("@description", question.Description);
                cmd.Prepare();
            }
            else
            {
                cmd.CommandText = "INSERT INTO questions(id,question,description,image)" +
                    "VALUES (@id,@question,@description,@image) ";
                cmd.Parameters.AddWithValue("@id", question.Id);
                cmd.Parameters.AddWithValue("@question", question.Question);
                cmd.Parameters.AddWithValue("@description", question.Description);
                cmd.Parameters.AddWithValue("@image", question.Media.Name);
                cmd.Prepare();
            }
            cmd.ExecuteNonQuery();
            connection.Close();
            AddChoice(question.Choices!, question.Id);
            
        }
        private void AddChoice(List<Choice> choices,int questionId)
        {
            foreach(var choice in choices)
            {
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO choices(text,answer,questionId)" +
                    "VALUES (@text,@answer,@questionId)";
                cmd.Parameters.AddWithValue("@text", choice.Text);
                cmd.Parameters.AddWithValue("@answer", choice.Answer);
                cmd.Parameters.AddWithValue("@questionId", questionId);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                connection.Close();
            }
            
        }

        public int GetQuestionsCount()
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM questions";
            var reader = command.ExecuteReader();
            int count = 0;
            while (reader.Read())
            {
                 count = reader.GetInt32(0);
            }
            connection.Close();
            return count;
        }
        public Question GetQuestionById(int questionId)
        {
            connection.Open();
            var question = new Question();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM questions WHERE id = {questionId}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                question.Id = reader.GetInt32(0);
                question.QuestionText = reader.GetString(1);
                question.Description = reader.GetString(2);
                question.Image =reader.IsDBNull(3) ? null: reader.GetString(3);
            }
            connection.Close();
            question.Choices = GetQuestionChoices(questionId);
            return question;
        }
        public List<Choice> GetQuestionChoices(int questionId)
        {
            connection.Open();
            var choices = new List<Choice>();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM choices WHERE questionId = {questionId}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var choice = new Choice();
                choice.Id = reader.GetInt32(0);
                choice.Text = reader.GetString(1);
                choice.Answer = reader.GetBoolean(2);
                choices.Add(choice);
            }
            connection.Close();
            return choices;
        }
    }
}
