using Hardcodet.Wpf.TaskbarNotification;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PomodoroTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TimerSettings timerSet = new(25, 5, 15, 4);
        private PomodoroSettings pomodoroSettings;
        private DispatcherTimer dispatcherTimer;

        private int pomodoroTimer;
        private int shortBreakTimer;
        private int longBreakTimer;
        private int sessionInterval;
        private int toSecond = 60;
        private int sessionCount = 1;
        private int duration = 0;
        private int elapsedSecond = 0;
        private int currentTimer;

        private string soundFileLocation;
        private string sessionType = "";
        private string sessionStatus = "";
        
        private bool isMinimized = false;


        public MainWindow()
        {
            pomodoroTimer = timerSet.PomodoroTime;
            shortBreakTimer = timerSet.ShortBreakTime;
            longBreakTimer = timerSet.LongBreakTime;
            sessionInterval = timerSet.SessionInterval;
            soundFileLocation = timerSet.SoundFileLocation;
            dispatcherTimer = new DispatcherTimer();
            this.pomodoroSettings = new PomodoroSettings(this, timerSet);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += timerTick;
            currentTimer = pomodoroTimer * toSecond;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LoadWindow);
            PomodoroDataManager pomodoroManageData = new();
            PomodoroSession();
            UpdateTimerLabel();
            pomodoroManageData.InitializeDatabase();
            pomodoroManageData.InitializeUsage();
        }

        private void PomodoroSession()
        {
            timerSet.IsBreak = false;
            currentTimer = pomodoroTimer * toSecond;
            sessionLbl.Content = $"Session: {sessionCount} / {sessionInterval}";
            sessionType = "Session";
            if (isMinimized)
            {
                ShowNotification("It's session time.");
            }
            UpdateTimerLabel();
            ToggleStop();
            RunTimerInBackground();
        }

        private void ShortBreakSession()
        {
            if (sessionCount == sessionInterval)
            {
                LongBreakSession();
            }
            else
            {
                timerSet.IsBreak = true;
                sessionType = "Short Break";
                sessionLbl.Content = sessionType;
                currentTimer = shortBreakTimer * toSecond;
                if (isMinimized)
                {
                    ShowNotification("It's short break time.");
                }
                UpdateTimerLabel();
                ToggleStop();
                RunTimerInBackground();
            }
        }

        private void LongBreakSession()
        {
            timerSet.IsBreak = true;
            sessionType = "Long Break";
            sessionLbl.Content = sessionType;
            currentTimer = longBreakTimer * toSecond;
            sessionCount = 0;
            if (isMinimized)
            {
                ShowNotification("It's long break time.");
            }
            UpdateTimerLabel();
            ToggleStop();
            RunTimerInBackground();
        }

        private void TogglePlay()
        {
            playBtn.Visibility = Visibility.Collapsed;
            pauseBtn.Visibility = Visibility.Visible;
            statusLbl.Content = "Running";
            dispatcherTimer.Start();
            if (isMinimized)
            {
                ShowNotification("Timer started.");
            }
        }

        private void UpdateTimerLabel()
        {
            int minutes = currentTimer / toSecond;
            int seconds = currentTimer % toSecond;
            timerLbl.Content = $"{minutes:D2}:{seconds:D2}";
        }

        private void timerTick(object? sender, EventArgs e)
        {
            if (currentTimer > 0)
            {
                currentTimer--;
                elapsedSecond++;
                UpdateTimerLabel();
                if (isMinimized)
                {
                    UpdateTooltip($"Time left: {currentTimer / toSecond} minutes {currentTimer % toSecond} seconds");
                }
                if (elapsedSecond % toSecond == 0)
                {
                    duration++;
                    elapsedSecond = 0;
                }
            }
            else
            {
                PlaySound();
                if (timerSet.IsBreak)
                {
                    if (!isMinimized) MessageBox.Show("Break time is over!");
                }
                else
                {
                    if (!isMinimized) MessageBox.Show("Session is done!");
                }
                sessionStatus = "Completed";
                PomodoroDataManager.SaveSession(duration, sessionType, sessionStatus);
                ChangeSession();

            }
        }

        private void ChangeSession()
        {
            if (timerSet.IsBreak)
            {
                sessionCount++;
            }

            timerSet.IsBreak = !timerSet.IsBreak;

            if (timerSet.IsBreak)
            {
                if (sessionCount == sessionInterval)
                {
                    LongBreakSession();
                }
                else
                {
                    ShortBreakSession();
                }
            }
            else
            {
                PomodoroSession();
            }

        }
        private void TogglePause()
        {
            pauseBtn.Visibility = Visibility.Collapsed;
            dispatcherTimer.Stop();
            if (playBtn.Visibility == Visibility.Collapsed)
            {
                playBtn.Visibility = Visibility.Visible;
            }
            sessionStatus = "Interrupted";
            statusLbl.Content = "Paused";
            PomodoroDataManager.SaveSession(duration, sessionType, sessionStatus);
            if (isMinimized)
            {
                ShowNotification("Timer paused.");
                UpdateTooltip("Pomodoro is paused.");
            }
        }

        private void ToggleStop()
        {
            playBtn.Visibility = Visibility.Visible;
            pauseBtn.Visibility = Visibility.Collapsed;
            sessionStatus = "Stopped";
            statusLbl.Content = sessionStatus;
            dispatcherTimer.Stop();
            duration = 0;
            if (timerSet.IsBreak)
            {
                if (sessionCount == sessionInterval)
                {
                    currentTimer = longBreakTimer * toSecond;
                }
                else
                {
                    currentTimer = shortBreakTimer * toSecond;
                }
            }
            else
            {
                currentTimer = pomodoroTimer * toSecond;
            }
            UpdateTimerLabel();
            if (isMinimized)
            {
                ShowNotification("Timer stopped.");
                UpdateTooltip("Pomodoro is stopped.");
            }
        }

        private void ToggleMute()
        {
            muteBtn.Visibility = Visibility.Visible;
            unmuteBtn.Visibility = Visibility.Collapsed;
            timerSet.IsMuted = true;
        }

        private void ToggleUnmute()
        {
            muteBtn.Visibility = Visibility.Collapsed;
            unmuteBtn.Visibility = Visibility.Visible;
            timerSet.IsMuted = false;
        }

        private void PlaySound()
        {
            MediaPlayer player = new MediaPlayer();
            player.Open(new Uri(soundFileLocation, UriKind.Relative));
            if (!timerSet.IsMuted)
            {
                player.Play();
            }
        }

        private void NextSession()
        {
            ChangeSession();
            ToggleStop();
        }

        private void ResetSession()
        {
            sessionCount = 1;
            ToggleStop();
            PomodoroSession();
        }

        private void GoToSettings()
        {
            pomodoroSettings.TimerSettings = timerSet;
            pomodoroSettings.Show();
            this.Hide();
        }

        public void UpdateTimerSettings()
        {
            pomodoroTimer = timerSet.PomodoroTime;
            shortBreakTimer = timerSet.ShortBreakTime;
            longBreakTimer = timerSet.LongBreakTime;
            sessionInterval = timerSet.SessionInterval;
            soundFileLocation = timerSet.SoundFileLocation;

            if (timerSet.IsBreak)
            {
                if (sessionCount == sessionInterval)
                {
                    LongBreakSession();
                }
                else
                {
                    ShortBreakSession();
                }
            }

            else
            {
                PomodoroSession();
            }

            if (dispatcherTimer.IsEnabled)
            {
                ToggleStop();
            }

            UpdateTimerLabel();
        }

        private void UpdateTooltip(string text)
        {
            PomodoroNotifyIcon.ToolTipText = text;
        }

        private void ShowNotification(string message)
        {
            PomodoroNotifyIcon.ShowBalloonTip("Pomodoro Timer", message, BalloonIcon.Info);
        }

        private void ShowContextMenu(ContextMenu? menu)
        {
            if (menu != null)
            {
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                menu.IsOpen = true;
            }
        }

        private void playBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimerLabel();
            TogglePlay();
        }

        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            NextSession();
        }

        private void settingsBtn_Click(object sender, RoutedEventArgs e)
        {
            GoToSettings();
        }

        private void resetBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetSession();
        }

        private void timerBtn_Click(object sender, RoutedEventArgs e)
        {
            PomodoroSession();
        }

        private void shortBreakBtn_Click(object sender, RoutedEventArgs e)
        {
            ShortBreakSession();
        }

        private void longBreakBtn_Click(object sender, RoutedEventArgs e)
        {
            LongBreakSession();
        }

        private void muteBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleUnmute();
        }
        private void unmuteBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleMute();
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            MinimizeWindow();
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            PomodoroNotifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void PomodoroNotifyIcon_LeftClick(object sender, RoutedEventArgs e)
        {
            ContextMenu? leftMenu = FindResource("LeftClickMenu") as ContextMenu;
            ShowContextMenu(leftMenu);
        }

        private void StartPomodoroMenu_Click(object sender, RoutedEventArgs e)
        {
            UpdateTimerLabel();
            TogglePlay();
        }

        private void PausePomodoroMenu_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void StopPomodoroMenu_Click(object sender, RoutedEventArgs e)
        {
            ToggleStop();
        }

        private void ResetPomodoroMenu_Click(object sender, RoutedEventArgs e)
        {
            ResetSession();
        }

        private void NextPomodoroMenu_Click(object sender, RoutedEventArgs e)
        {
            NextSession();
        }


        private void PomodoroNotifyIcon_RightClick(object sender, RoutedEventArgs e)
        {
            ContextMenu? rightMenu = FindResource("RightClickMenu") as ContextMenu;
            ShowContextMenu(rightMenu);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            GoToSettings();
        }

        private void PomodoroMenu_Click(object sender, RoutedEventArgs e)
        {
            PomodoroSession();
        }

        private void ShortBreakMenu_Click(object sender, RoutedEventArgs e)
        {
            ShortBreakSession();
        }

        private void LongBreakMenu_Click(object sender, RoutedEventArgs e)
        {
            LongBreakSession();
        }

        private void PomodoroNotifyIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            if (isMinimized)
            {
                ShowWindow(sender, e);
            }
            else
            {
                MinimizeWindow();
            }
        }

        private void ShowWindow(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            isMinimized = false;
        }

        private void LoadWindow(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        public void MinimizeWindow()
        {
            isMinimized = true;
            WindowState = WindowState.Minimized;
            this.Hide();
            ShowNotification("Pomodoro is running in the background!");
        }

        private async void RunTimerInBackground()
        {
            if (isMinimized)
            { 
                await Task.Delay(1000);
                TogglePlay();
            }
        }
 
    }
}