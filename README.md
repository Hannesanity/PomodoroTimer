# Pomodoro Timer with Scheduling and Reporting

## Description
This is a **C# WPF-based Pomodoro Timer** that helps users efficiently manage their work sessions using the Pomodoro technique. The application includes additional features such as scheduling, historical tracking, email reports, and customizable settings.

## Features
- **Pomodoro Timer** ⏳
  - Set custom durations for **Pomodoro, Short Break, and Long Break**
  - Configurable session intervals
- **Task Logging & History** 📜
  - Saves completed sessions
  - Tracks Pomodoro history for efficiency analysis
- **Email Reporting** 📧
  - Automatically sends a monthly Pomodoro session summary
  - Configurable email settings
- **Schedule Management** 📅
  - Saves the last sent report and schedules the next one
  - Prevents duplicate reports within the same month
- **User Customization** ⚙️
  - Save preferences in a `.env` file
  - Sound notifications for session changes
  - Toggleable email reporting
- **Settings Management** 🎛️
  - Modify Pomodoro timings
  - Enable/Disable email reports
  - Select notification sound

---

## Getting Started

### Prerequisites
Ensure you have the following installed:
- **.NET (Windows Desktop Runtime)**
- **Visual Studio 2022 (or later)** with WPF Development
- **SQLite** (Bundled with the project)

### Installation
1. Clone the repository:
   ```sh
   git clone https://github.com/yourusername/pomodoro-timer.git
   cd pomodoro-timer
   ```
2. Open the project in **Visual Studio**
3. Build and run the solution (`Ctrl + F5`)

### Configurations
The application stores settings in a `.env` file. If not found, it will be automatically created with default values:
```
SOUND_FILE=
EMAIL_ADDRESS=
SEND_REPORTS=False
```
Modify the `.env` file to configure your settings:
- Set `EMAIL_ADDRESS=your_email@example.com` for email reports.
- Set `SEND_REPORTS=True` to enable automatic email reporting.

---

## 🖥️ Usage Guide
### Timer Controls
- **Start/Pause** the Pomodoro Timer
- **Reset** to restart a session
- **Toggle Reporting** to enable/disable email reports

### Managing Reports
- Reports are sent automatically at the beginning of each month
- If reporting is enabled, the system will log your sessions and email the summary

---

## 🛠️ Development
### Project Structure
```
PomodoroTimer/
│-- src/
│   ├── MainWindow.xaml.cs  # Main user interface
│   ├── PomodoroSettings.xaml.cs  # Settings user interface
│   ├── TimerSettings.cs    # Handles user settings
│   ├── PomodoroDataManager.cs  # Handles database interactions
│   ├── EmailManager.cs     # Manages email report sending
│-- assets/
│   ├── sounds/             # Notification sounds
│-- database/
│   ├── pomodoro.db         # SQLite database file
│-- .env                    # Environment config file
│-- README.md               # Project documentation
```

### Running in Debug Mode
- Open **Visual Studio**
- Run in Debug Mode (`F5`)
- Set breakpoints in `TimerSettings.cs` or `PomodoroDataManager.cs` to inspect behavior

---

## 📩 Idea
Feel free to submit **issues** or **pull requests**:
1. Fork the repository 🍴
2. Create a feature branch (`git checkout -b feature-new`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to the branch (`git push origin feature-new`)
5. Open a Pull Request 🎉

---

## Credits

## Credits & Acknowledgments
- System design inspired by [Pomofocus.io](https://pomofocus.io/)
- Pomodoro timer implementation based on techniques from [Replicon's guide](https://www.replicon.com/blog/pomodoro-technique/)
- UI components and styling using [Material Design in XAML](http://materialdesigninxaml.net/)
