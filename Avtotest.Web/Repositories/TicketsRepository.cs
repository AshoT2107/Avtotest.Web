using Avtotest.Web.Models;
using Microsoft.Data.Sqlite;

namespace Avtotest.Web.Repositories
{
    public class TicketsRepository
    {
        private readonly string connectionString = "Data source = avtotest.db";
        private SqliteConnection connection;
        public TicketsRepository()
        {
            connection = new SqliteConnection(connectionString);
            CreateTicketTable();
        }
        private void CreateTicketTable()
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS tickets(" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "userId INTEGER," +
                "fromIndex INTEGER," +
                "questionsCount INTEGER," +
                "correctAnswerCount INTEGER," +
                "isTraining BOOLEN)";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE TABLE IF NOT EXISTS ticketData(" +
                "id INTEGER PRIMARY KEY AUTOINCREMENT," +
                "ticketId INTEGER," +
                "questionId INTEGER," +
                "choiceId INTEGER," +
                "answer BOOLEN)";
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void InsertTicket(Ticket ticket)
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Tickets(userId,fromIndex,questionsCount,correctAnswerCount,isTraining)" +
                "VALUES(@userId,@fromIndex,@questionsCount,@correctAnswerCount,@isTraining)";
            command.Parameters.AddWithValue("userId",ticket.UserId);
            command.Parameters.AddWithValue("fromIndex", ticket.FromIndex);
            command.Parameters.AddWithValue("questionsCount", ticket.QuestionsCount);
            command.Parameters.AddWithValue("correctAnswerCount", ticket.CorrectAnswerCount);
            command.Parameters.AddWithValue("isTraining", ticket.IsTraining);
            command.Prepare();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public int GetLastRowId()
        {
            int id = 0;
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM tickets ORDER BY Id DESC LIMIT 1";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                id = reader.GetInt32(0);
            }
            connection.Close();
            return id;
        }

        public int GetTicketAnswerCount(int ticketId)
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) from ticketData WHERE ticketId = {ticketId}";
            var data = cmd.ExecuteReader();

            while (data.Read())
            {
                var count = data.GetInt32(0);
                connection.Close();
                data.Close();
                return count;
            }

            connection.Close();
            return 0;
        }
        public void InsertTicketData(TicketData ticketData)
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO ticketData(ticketId,questionId,choiceId,answer)" +
                "VALUES(@ticketId,@questionId,@choiceId,@answer)";
            command.Parameters.AddWithValue("@ticketId", ticketData.TicketId);
            command.Parameters.AddWithValue("@questionId", ticketData.QuestionId);
            command.Parameters.AddWithValue("@choiceId", ticketData.ChoiceId);
            command.Parameters.AddWithValue("@answer", ticketData.Answer);
            command.Prepare();
            command.ExecuteNonQuery();
            connection.Close();
        }
        public List<TicketData> GetTicketDataByTicketId(int ticketId)
        {
            var ticketDatas = new List<TicketData>();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT questionId,choiceId,answer from ticketData WHERE ticketId = {ticketId}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var ticketData = new TicketData()
                {
                    QuestionId = reader.GetInt32(0),
                    ChoiceId = reader.GetInt32(1),
                    Answer = reader.GetBoolean(2)
                };
                ticketDatas.Add(ticketData);
            }
            connection.Close();
            return ticketDatas;
        }

        public TicketData? GetTicketDataByQuestionId(int ticketId,int questionId)
        {
            
            connection.Open();
            var ticketData = new TicketData();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM ticketData WHERE ticketId = {ticketId} AND questionId = {questionId}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ticketData.Id = reader.GetInt32(0);
                ticketData.TicketId = reader.GetInt32(1);
                ticketData.QuestionId = reader.GetInt32(2);
                ticketData.ChoiceId = reader.GetInt32(3);
                ticketData.Answer = reader.GetBoolean(4);
            }
            connection.Close();
            if(ticketData.QuestionId == questionId)
                {
                    return ticketData; 
                }
                return null;
        }

        public void UpdateTicketCorrectAnswerCount(int ticketId)
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE tickets SET correctAnswerCount = correctAnswerCount + 1 WHERE id={ticketId}";
            command.ExecuteNonQuery();
            connection.Close();
        }

        public List<Ticket> GetTicketsByUserId(int userId)
        {
            var tickets = new List<Ticket>();

            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT id, fromIndex, questionsCount, correctAnswerCount FROM tickets WHERE userId = {userId} AND isTraining = true";
            var data = cmd.ExecuteReader();
            while (data.Read())
            {
                var ticketData = new Ticket()
                {
                    Id = data.GetInt32(0),
                    FromIndex = data.GetInt32(1),
                    QuestionsCount = data.GetInt32(2),
                    CorrectAnswerCount = data.GetInt32(3),
                    UserId = userId
                };
                tickets.Add(ticketData);
            }

            connection.Close();
            return tickets;
        }

        public Ticket GetTicketById(int id,int userId)
        {
            var ticket = new Ticket();
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM tickets WHERE id = {id} AND userId={userId}";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ticket.Id = reader.GetInt32(0);
                ticket.UserId = reader.GetInt32(1);
                ticket.FromIndex = reader.GetInt32(2);
                ticket.QuestionsCount = reader.GetInt32(3);
                ticket.CorrectAnswerCount = reader.GetInt32(4);
            }
            connection.Close();
            return ticket;
        }
        public void InsertUserTrainingTickets(int userId, int ticketsCount, int ticketQuestionsCount)
        {
            for (int i = 0; i < ticketsCount; i++)
            {
                InsertTicket(new Ticket()
                {
                    UserId = userId,
                    CorrectAnswerCount = 0,
                    IsTraining = true,
                    FromIndex = i * ticketQuestionsCount + 1,
                    QuestionsCount = ticketQuestionsCount
                });
            }
        }




    }
}
