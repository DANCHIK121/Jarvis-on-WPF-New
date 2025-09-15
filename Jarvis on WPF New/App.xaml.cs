// Standart usings
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Jarvis_on_WPF_New
{
    public partial class App : Application
    {
        private NotifyIcon? _trayIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load icon
            System.Drawing.Icon icon = new System.Drawing.Icon(Directory.GetCurrentDirectory() + "\\Icon\\Icon.ico");

            // Create app icon in tray
            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = icon;
            _trayIcon.Text = "Jarvis";

            // Простое меню
            var menu = new ContextMenuStrip();
            menu.Items.Add("Открыть", null, (s, e) => ShowWindow());
            menu.Items.Add("Выход", null, (s, e) => Shutdown());

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, e) => ShowWindow();
            _trayIcon.Visible = true;
        }

        private void ShowWindow()
        {
            if (MainWindow != null)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
