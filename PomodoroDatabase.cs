using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PomodoroTimer
{
    public class PomodoroDatabase
    {
        private static string databasePath = @"Pomodoro.db";
        private static string connectionString = @"Data Source=" + databasePath + ";Version=3;";
        private static PomodoroDatabase? _instance;
        private SQLiteConnection _connection;
        private static readonly object _lock = new();

        private PomodoroDatabase(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
        }

        public static PomodoroDatabase GetInstance(string connectionString)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new PomodoroDatabase(connectionString);
                    }
                }
            }
            return _instance;
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        public void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }
    }
}
