using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using DotNetEnv;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Data;
using System.Net;
using System.Net.Mail;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PomodoroTimer
{
    class PomodoroDataManager
    {
        private static string databasePath = @"Pomodoro.db";
        private static string connectionString = @"Data Source=" + databasePath + ";Version=3;";
        private static DateTime date = DateTime.Now;
        TimerSettings timerSet = new(25, 5, 15, 4);

        public class SessionData
        {
            public int sID { get; set; }
            public string? sDate { get; set; }
            public string? sStartTime { get; set; }
            public int sDuration { get; set; }
            public string? sSessionType { get; set; }
            public string? sStatus { get; set; }
        }

        public void InitializeDatabase()
        {
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                using (var connection = PomodoroDatabase.GetInstance(connectionString).GetConnection())
                {
                    connection.Open();
                    string sessionSql = @"CREATE TABLE Sessions (
                                    sID INTEGER PRIMARY KEY AUTOINCREMENT, 
                                    sDate TEXT, 
                                    sStartTime TEXT, 
                                    sDuration INTEGER,
                                    sSessionType TEXT,
                                    sStatus TEXT)";
                    string scheduleSql = @"CREATE TABLE Schedules (
                                    schedId	INTEGER,
	                                schedEmail TEXT,
	                                schedLastSent TEXT DEFAULT NULL,
	                                schedNextSend TEXT DEFAULT NULL,
	                                schedIsEnabled INTEGER DEFAULT 0,
                                    schedIsSent	INTEGER DEFAULT 0, 
	                                PRIMARY KEY(schedId AUTOINCREMENT))";
                    using var sessionCommand = new SQLiteCommand(sessionSql, connection);
                    using var scheduleCommand = new SQLiteCommand(scheduleSql, connection);
                    sessionCommand.ExecuteNonQuery();
                    scheduleCommand.ExecuteNonQuery();
                }
                    
            }
        }

        public void InitializeUsage()
        {
            CheckSchedule(GetSessionData());
        }

        public void CheckSchedule(List<SessionData> sessions)
        {
            using (var connection = PomodoroDatabase.GetInstance(connectionString).GetConnection())
            {
                try
                {
                    connection.Open();
                    string sql = @"SELECT * FROM Schedules
                            WHERE schedNextSend = strftime('%m-%Y', date('now'));
                            ";
                    SQLiteCommand command = new(sql, connection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (reader.HasRows && reader.Read() && Convert.ToInt32(reader["schedIsEnabled"]) == 1)
                    {
                        if (Convert.ToInt32(reader["schedIsSent"]) == 0)
                        {
                            SendEmail(sessions);
                        }

                    }
                    else
                    {
                        SaveSchedule();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error while saving checking the schedule: {e.Message}");
                }
            }
        }

        public static void SaveSession(int duration, string sessionType, string status)
        {
            using (var connection = PomodoroDatabase.GetInstance(connectionString).GetConnection())
            {
                try
                {
                    connection.Open();
                    DateTime startTime = DateTime.Now.AddMinutes(-duration);
                    string sql = @"INSERT INTO Sessions (sDate, sStartTime, sDuration, sSessionType, sStatus) 
                                VALUES (@date, @startTime, @duration, @sessionType ,@status)";
                    using var command = new SQLiteCommand(sql, connection);
                    command.Parameters.AddWithValue("@date", date.ToString("dd-MM-yyyy"));
                    command.Parameters.AddWithValue("@startTime", startTime.ToString("HH:mm:ss"));
                    command.Parameters.AddWithValue("@duration", duration);
                    command.Parameters.AddWithValue("@sessionType", sessionType);
                    command.Parameters.AddWithValue("@status", status);
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error while saving session: {e.Message}");
                }
            }
                

        }
        
        public void SaveSchedule()
        {
            using (var connection = PomodoroDatabase.GetInstance(connectionString).GetConnection())
            {
                try
                {   
                    connection.Open();
                    string checkSql = @"SELECT * FROM Schedules WHERE schedLastSent = strftime('%m-%Y', date('now'))";
                    using var checkCommand = new SQLiteCommand(checkSql, connection);
                    using SQLiteDataReader reader = checkCommand.ExecuteReader();
                    if (!reader.HasRows)
                    {
                        string sql = @"INSERT INTO Schedules (schedEmail, schedLastSent, schedNextSend, schedIsEnabled) 
                VALUES (@email, @lastsent, @nextsend, @isenabled)";
                        using var command = new SQLiteCommand(sql, connection);
                        command.Parameters.AddWithValue("@email", timerSet.EmailAddress);
                        command.Parameters.AddWithValue("@lastsent", date.ToString("MM-yyyy"));
                        command.Parameters.AddWithValue("@nextsend", date.AddMonths(1).ToString("MM-yyyy"));
                        command.Parameters.AddWithValue("@isenabled", timerSet.SendReport ? 1 : 0);
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error while saving schedule: {e.Message}");
                }
            }
        }

        public List<SessionData> GetSessionData()
        {
            List<SessionData> sessions = new();
            DataTable dt = new();
            using (var connection = PomodoroDatabase.GetInstance(connectionString).GetConnection())
            {
                try
                {
                    connection.Open();
                    // This SQL query selects all sessions from last month, and converts the date to the format 'YYYY-MM-DD' for compatibility with SQLite
                    string sql = @"SELECT * FROM Sessions
                            WHERE strftime('%Y-%m', substr(sDate, 7, 4) || '-' || substr(sDate, 4, 2) || '-' || substr(sDate, 1, 2)) 
                            = strftime('%Y-%m', date('now', '-1 month'));
                            ";
                    using var command = new SQLiteCommand(sql, connection);
                    using var adapter = new SQLiteDataAdapter(command);
                    adapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        SessionData session = new SessionData
                        {
                            sID = Convert.ToInt32(row["sID"]),
                            sDate = row["sDate"].ToString(),
                            sStartTime = row["sStartTime"].ToString(),
                            sDuration = Convert.ToInt32(row["sDuration"]),
                            sSessionType = row["sSessionType"].ToString(),
                            sStatus = row["sStatus"].ToString()
                        };

                        sessions.Add(session);
                    }


                }

                catch (Exception e)
                {
                    MessageBox.Show($"Error while getting session data: {e.Message}");
                }
            }
            
            return sessions;
        }

        

        public string GenerateEmailContent(List<SessionData> sessions)
        {
            double totalFocusTime = CalculateFocusTime(sessions);
            double totalDuration = CalculateTotalDuration(sessions);
            int totalSessions = CalculateTotalSessions(sessions);
            int interruptedSessions = CalculateInterruptedSessions(sessions);
            int completedSessions = CalculateCompletedSessions(sessions);
            double averageSessionDuration = CalculateAverageSessionDuration(sessions);
            double completionRate = CalculateCompletionRate(sessions);
            int longestStreak = CalculateLongestCompletedStreak(sessions);
            string emailContent = $"Hello! Here is your usage summary for the month of {date.AddMonths(-1).ToString("MMMM yyyy")}: \n" +
                      $"Total Focus Time: {totalFocusTime} minutes\n" +
                      $"Total Duration: {totalDuration} minutes\n" +
                      $"Total Sessions: {totalSessions}\n" +
                      $"Interrupted Sessions: {interruptedSessions}\n" +
                      $"Completed Sessions: {completedSessions}\n" +
                      $"Average Session Duration: {averageSessionDuration} minutes\n" +
                      $"Completion Rate: {completionRate}%\n" +
                      $"Longest Completed Streak: {longestStreak} sessions";

            using (StreamWriter writer = new StreamWriter("LastEmailSent.txt"))
            {
                writer.WriteLine(emailContent);
            }
            return emailContent;
        }

        public static double CalculateFocusTime(List<SessionData> sessions)
        {
            double totalFocusTime = sessions.Where(s => s.sSessionType == "Session").Sum(sessions => sessions.sDuration);
            return totalFocusTime;
        }

        public static double CalculateTotalDuration(List<SessionData> sessions)
        {
            double totalDuration = sessions.Sum(sessions => sessions.sDuration);
            return totalDuration;
        }

        public static int CalculateTotalSessions(List<SessionData> sessions)
        {
            int totalSessions = sessions.Count();
            return totalSessions;
        }

        public static int CalculateInterruptedSessions(List<SessionData> sessions)
        {
            int interruptedSessions = sessions.Where(s => s.sStatus == "Interrupted" || s.sStatus == "Stopped").Count();
            return interruptedSessions;
        }

        public static int CalculateCompletedSessions(List<SessionData> sessions)
        {
            int completedSessions = sessions.Where(s => s.sStatus == "Completed").Count();
            return completedSessions;
        }

        public static double CalculateAverageSessionDuration(List<SessionData> sessions)
        {
            double averageSessionDuration = sessions.Average(s => s.sDuration);
            return averageSessionDuration;
        }

        public static double CalculateCompletionRate(List<SessionData> sessions)
        {
            double completionRate = (double)CalculateCompletedSessions(sessions) / CalculateTotalSessions(sessions) * 100;
            return completionRate;
        }

        public static int CalculateLongestCompletedStreak(List<SessionData> sessions)
        {
            int longestStreak = 0;
            int currentStreak = 0;

            foreach (var streak in sessions.OrderBy(s => s.sDate))
            {
                if (streak.sStatus == "Completed")
                {
                    currentStreak++;
                    if (currentStreak > longestStreak)
                    {
                        longestStreak = currentStreak;
                    }
                }
                else
                {
                    currentStreak = 0;
                }
            }


            return longestStreak;
        }

        public static string FormatEmailBody(string emailContent)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Pomodoro Monthly Usage Summary</h2>
                <p>{emailContent.Replace("\n", "<br>")}</p>
                <br>
                <p>Stay productive! 🍅</p>
            </body>
            </html>";
        }

        public MailMessage CreateEmailMessage(string receipent, string subject, string emailBody)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(timerSet.SenderEmailAddress);
            mail.To.Add(timerSet.EmailAddress);
            mail.Subject = "Your Monthly Pomodoro Report";
            mail.Body = emailBody;
            mail.IsBodyHtml = true;

            return mail;
        }

        public void SendEmail(List<SessionData> sessions)
        {
            try
            {
                if (timerSet.EmailAddress is not null) {
                    string emailContent = GenerateEmailContent(sessions);
                    string emailBody = FormatEmailBody(emailContent);

                    MailMessage mailMessage = CreateEmailMessage(timerSet.EmailAddress, "Your Pomodoro Report", emailBody);
                    using (SmtpClient client = new SmtpClient("smtp.gmail.com"))
                    {
                        client.Port = 587;
                        client.Credentials = new NetworkCredential(timerSet.SenderEmailAddress, timerSet.SenderEmailPassword);
                        client.EnableSsl = true;
                        client.Send(mailMessage);
                    }

                    UpdateScheduleAsSent();
                }
                
            }


            catch (Exception e)
            {
                MessageBox.Show($"Error while sending email: {e.Message}");
            }
        }

        public void UpdateScheduleAsSent()
        {
            using (var connection = PomodoroDatabase.GetInstance(connectionString).GetConnection())
            {
                try
                {
                    connection.Open();
                    string sql = @"UPDATE Schedules SET schedIsSent = 1
                            WHERE schedNextSend = strftime('%m-%Y', date('now'));
                            ";
                    using var command = new SQLiteCommand(sql, connection);
                    command.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error while updating schedule as sent: {e.Message}");
                }
            }
                
        }

        public void CreateLog()
        {
            try
            {
                using StreamWriter writer = new("log.txt", true);
                writer.WriteLine($"[{DateTime.Now}] - Session saved successfully");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

    }
}
