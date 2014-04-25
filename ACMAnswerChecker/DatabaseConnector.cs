using System;
using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace ACMAnswerChecker
{
    static class DatabaseConnector
    {
        public static MySqlConnection TheMySqlConnection { get; private set; }

        public static void Init(String server, String database, String uid, String pwd)
        {
            TheMySqlConnection = new MySqlConnection("Server=" + server + ";Database=" + database + ";Uid=" + uid + ";Pwd=" + pwd + ";charset=utf8;");
        }

        public static Answer GetEarliestPendingAnswer()
        {
            Answer thisAnswer = null;
            var thisMySqlCommand = TheMySqlConnection.CreateCommand();
            thisMySqlCommand.CommandText = "SELECT `answer`.* FROM `answer` WHERE `answer`.StatusCode = 1 LIMIT " + Program.Offset + ",1";
            if (TheMySqlConnection.State == ConnectionState.Closed) TheMySqlConnection.Open();
            var thisMySqlDataReader = thisMySqlCommand.ExecuteReader();
            try
            {
                while (thisMySqlDataReader.Read())
                {
                    thisAnswer = new Answer(
                        thisMySqlDataReader.GetInt64("ID"),
                        thisMySqlDataReader.GetInt64("ProblemID"),
                        thisMySqlDataReader.GetInt64("UserID"),
                        thisMySqlDataReader.GetInt16("LanguageCode"),
                        thisMySqlDataReader.GetString("SourceCode"),
                        "",
                        "",
                        thisMySqlDataReader.GetInt64("UsedTime"),
                        thisMySqlDataReader.GetInt64("UsedMemory"),
                        thisMySqlDataReader.GetInt16("StatusCode"),
                        thisMySqlDataReader.GetString("Info"),
                        thisMySqlDataReader.GetMySqlDateTime("SubmitTime").GetDateTime(),
                        thisMySqlDataReader.GetMySqlDateTime("MarkedTime").GetDateTime()
                        );
                }
            }
            catch (MySqlConversionException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                thisMySqlDataReader.Close();
            }
            return thisAnswer;
        }

        public static int UpdateAnswer(Answer thatAnswer)
        {
            try
            {
                var command = TheMySqlConnection.CreateCommand();
                command.CommandText = "UPDATE answer " +
                                               "SET " +
                                               "answer.UsedTime = @UsedTime, " +
                                               "answer.UsedMemory = @UsedMemory, " +
                                               "answer.StatusCode = @StatusCode, " +
                                               "answer.Info = @Info, " +
                                               "answer.MarkedTime = @MarkedTime " +
                                               "WHERE answer.ID = @ID";
                command.Parameters.AddWithValue("@ID", thatAnswer.Id);
                command.Parameters.AddWithValue("@UsedTime", thatAnswer.UsedTime);
                command.Parameters.AddWithValue("@UsedMemory", thatAnswer.UsedMemory);
                command.Parameters.AddWithValue("@StatusCode", thatAnswer.StatusCode);
                command.Parameters.AddWithValue("@Info", thatAnswer.Info);
                command.Parameters.AddWithValue("@MarkedTime", new MySqlDateTime(thatAnswer.MarkedTime));

                if (TheMySqlConnection.State == ConnectionState.Closed) TheMySqlConnection.Open();
                var result = command.ExecuteNonQuery();
                return result;
            }
            catch (MySqlConversionException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static Problem GetProblemById(Int64 id)
        {
            var command = TheMySqlConnection.CreateCommand();
            command.CommandText = "SELECT `problem`.* FROM `problem` WHERE `problem`.ID = @ID";
            command.Parameters.AddWithValue("@ID", id);

            if (TheMySqlConnection.State == ConnectionState.Closed) TheMySqlConnection.Open();
            var thisMySqlDataReader = command.ExecuteReader();
            Problem thisProblem = null;
            try
            {
                while (thisMySqlDataReader.Read())
                {
                    thisProblem = new Problem(
                        thisMySqlDataReader.GetInt64("ID"),
                        thisMySqlDataReader.GetInt64("TimeLimitNormal"),
                        thisMySqlDataReader.GetInt64("TimeLimitJava"),
                        thisMySqlDataReader.GetInt64("MemoryLimitNormal"),
                        thisMySqlDataReader.GetInt64("MemoryLimitJava"),
                        thisMySqlDataReader.GetString("StandardInput"),
                        thisMySqlDataReader.GetString("StandardOutput")
                        );
                }
            }
            catch (MySqlConversionException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                thisMySqlDataReader.Close();
            }
            return thisProblem;
        }

    }
}
