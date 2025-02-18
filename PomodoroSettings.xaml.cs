using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Text.Json;
using System.IO;
using DotNetEnv;

namespace PomodoroTimer
{

    public partial class PomodoroSettings : Window
    {
        private readonly MainWindow mainWindow;
        private bool programStarted = false;
        public TimerSettings TimerSettings { get; set; }

        public PomodoroSettings(MainWindow mainWindow, TimerSettings TimerSettings)
        {
            InitializeComponent();
            this.TimerSettings = TimerSettings;
            this.mainWindow = mainWindow;
            InitializeEnvFile();
            InitializeSendReportToggle();            
            UpdateTimerVariables();
            this.Loaded += new RoutedEventHandler(LoadWindow);
        }

        public void InitializeEnvFile()
        {
            string envFilePath = ".env";
            if (!File.Exists(envFilePath))
            {
                File.WriteAllLines(envFilePath, new[]
                {
                    "SOUND_FILE=",
                    "EMAIL_ADDRESS=",
                    "SEND_REPORTS=False"
                });
            }
            Env.Load();
        }

        private void InitializeSendReportToggle()
        {
            if (TimerSettings.SendReport)
            {
                sendRepToggle.IsChecked = true;
                emailTxtBox.IsEnabled = true;
            }
            else
            {
                sendRepToggle.IsChecked = false;
                emailTxtBox.IsEnabled = false;
            }
            programStarted = true;
        }

        private void UpdateTimerVariables()
        {
            emailTxtBox.Text = TimerSettings.EmailAddress;
            sendRepToggle.IsChecked = TimerSettings.SendReport;
            soundLocLbl.Content = System.IO.Path.GetFileName(TimerSettings.SoundFileLocation);
            emailTxtBox.IsReadOnly = !TimerSettings.SendReport;
        }

        private void UpdateEnvVariables()
        {
            string envFilePath = ".env";
            if (!File.Exists(envFilePath))
            {
                File.Create(envFilePath).Close();
            }
            string[] lines = File.ReadAllLines(envFilePath);
            var updatedFile = new List<string>();
            var updatedVariables = new HashSet<string>();
            foreach (string line in lines)
            {
                string updatedLine = line;
                if (line.StartsWith("SOUND_FILE="))
                {
                    updatedLine = $"SOUND_FILE={TimerSettings.SoundFileLocation}";
                    updatedVariables.Add("SOUND_FILE");
                }
                else if (line.StartsWith("EMAIL_ADDRESS="))
                {
                    updatedLine = $"EMAIL_ADDRESS={emailTxtBox.Text}";
                    updatedVariables.Add("EMAIL_ADDRESS");
                }
                else if (line.StartsWith("SEND_REPORTS="))
                {
                    updatedLine = $"SEND_REPORTS={TimerSettings.SendReport}";
                    updatedVariables.Add("SEND_REPORTS");
                }
                updatedFile.Add(updatedLine);
            }
            if (!updatedVariables.Contains("SOUND_FILE"))
            {
                updatedFile.Add($"SOUND_FILE={TimerSettings.SoundFileLocation}");
            }
            if (!updatedVariables.Contains("EMAIL_ADDRESS"))
            {
                updatedFile.Add($"EMAIL_ADDRESS={TimerSettings.EmailAddress}");
            }
            if (!updatedVariables.Contains("SEND_REPORTS"))
            {
                updatedFile.Add($"SEND_REPORTS={TimerSettings.SendReport}");
            }
            File.WriteAllLines(envFilePath, updatedFile);
            Env.Load();
        }

        private void SendRepToggle_Check(object sender, RoutedEventArgs e)
        {
            if (!programStarted)
            {
                programStarted = true;
                return;
            }


            sendRepToggle.Checked -= SendRepToggle_Check;
            sendRepToggle.Unchecked -= SendRepToggle_Uncheck;

            var result = MessageBox.Show("Are you sure?", "Send Report", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                TimerSettings.SendReport = true;
                sendRepToggle.IsChecked = true;
                emailTxtBox.IsEnabled = true;

            }
            else
            {
                sendRepToggle.IsChecked = false;
            }

            sendRepToggle.Checked += SendRepToggle_Check;
            sendRepToggle.Unchecked += SendRepToggle_Uncheck;

        }

        private void SendRepToggle_Uncheck(object sender, RoutedEventArgs e)
        {
            if (!programStarted)
            {
                programStarted = true;
                return;
            }

            sendRepToggle.Checked -= SendRepToggle_Check;
            sendRepToggle.Unchecked -= SendRepToggle_Uncheck;

            var result = MessageBox.Show("Are you sure?", "Send Report", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                TimerSettings.SendReport = false;
                sendRepToggle.IsChecked = false;
                emailTxtBox.IsEnabled = false;
                programStarted = true;
            }
            else
            {
                sendRepToggle.IsChecked = true;
            }
            sendRepToggle.Checked += SendRepToggle_Check;
            sendRepToggle.Unchecked += SendRepToggle_Uncheck;
        }

        private void BackToMainWindow() 
        {
            mainWindow.Show();
            this.Hide();
        }

        private void backBtn_Click(object sender, RoutedEventArgs e)
        {
            BackToMainWindow();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TimerSettings.PomodoroTime = int.Parse(pomodoroTxtBox.Text);
                TimerSettings.ShortBreakTime = int.Parse(shortBreakTxtBox.Text);
                TimerSettings.LongBreakTime = int.Parse(longBreakTxtBox.Text);
                TimerSettings.SessionInterval = int.Parse(intervalTxtBox.Text);

                if (TimerSettings.PomodoroTime < 1 || TimerSettings.ShortBreakTime < 1 || TimerSettings.LongBreakTime < 1 || TimerSettings.SessionInterval < 1)
                {
                    MessageBox.Show("Numbers should be positive.");
                    return;
                }

                mainWindow.UpdateTimerSettings();
                UpdateEnvVariables();
                MessageBox.Show("Your settings is changed.");
                BackToMainWindow();
            }
            catch (FormatException)
            {
                MessageBox.Show("Please enter a valid number.");
            }
        }

        

        private void selectLocBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new();
            fileDialog.Filter = "Audio files (*.wav;*.mp3;*.wma;*.aac)|*.wav;*.mp3;*.wma;*.aac";
            if (fileDialog.ShowDialog() == true)
            {
                TimerSettings.SoundFileLocation = fileDialog.FileName;

                string fileName = System.IO.Path.GetFileName(fileDialog.FileName);
                soundLocLbl.Content = fileName;
            }
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void LoadWindow(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }
    }
}
