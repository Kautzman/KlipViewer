using System.Windows;
using System.Diagnostics;

namespace KlipViewer
{
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Icon = KlipViewer.Properties.Resources.iKon;
            _notifyIcon.Visible = true;
            CreateContextMenu();

            if (Process.GetProcessesByName("KlipViewer").Length > 1)
            {
                MessageBox.Show("KlipViewer is already running! (Toggle with Scroll Lock)");
                ExitApplication();
            }
        }

        private void CreateContextMenu()
        {
            _notifyIcon.ContextMenuStrip =
              new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add("Close").Click += (s, e) => ExitApplication();
        }

        private void ExitApplication()
        {
            // Cleanup the systray icons, because Windows sure won't...
            _notifyIcon.Dispose();
            _notifyIcon = null;
            Application.Current.Shutdown();
        }
    }
}
