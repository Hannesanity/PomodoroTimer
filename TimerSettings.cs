using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DotNetEnv;

namespace PomodoroTimer
{
    public class TimerSettings
    {
        public int PomodoroTime { get; set; } = 25;
        public int ShortBreakTime { get; set; } = 5;
        public int LongBreakTime { get; set; } = 15;
        public int SessionInterval { get; set; } = 4;

        public bool IsMuted { get; set; } = false;
        public bool IsBreak { get; set; } = false;
        public bool SendReport { get; set; } = false;
        public bool IsMinimized { get; set; } = false;

        public string? SoundFileLocation { get; set; }
        public string? EmailAddress { get; set; }
        public string? SenderEmailAddress { get; set; }
        public string? SenderEmailPassword { get; set; }

        public TimerSettings(int pomodoroTime, int shortBreakTime, int longBreakTime, int sessionInterval)
        {
            try
            {
                Env.Load();
                SoundFileLocation = Environment.GetEnvironmentVariable("SOUND_FILE")
                                ?? System.AppDomain.CurrentDomain.BaseDirectory + "timer.wav";
                EmailAddress = Environment.GetEnvironmentVariable("EMAIL_ADDRESS") ?? "example@example.com";
                SenderEmailAddress = Environment.GetEnvironmentVariable("SENDER_EMAIL_ADDRESS") ?? "";
                SenderEmailPassword = Environment.GetEnvironmentVariable("SENDER_EMAIL_PASSWORD") ?? "password";
                SendReport = Environment.GetEnvironmentVariable("SEND_REPORTS")?.ToLower() == "true";
                PomodoroTime = pomodoroTime;
                ShortBreakTime = shortBreakTime;
                LongBreakTime = longBreakTime;
                SessionInterval = sessionInterval;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
