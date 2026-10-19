using System;
using System.Threading;
using System.Windows.Forms;
using ClipCourier.App;
using ClipCourier.Infrastructure;
using ClipCourier.Models;

namespace ClipCourier
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool created;
            // Only one instance can own the configured global shortcut.
            using (Mutex mutex = new Mutex(true, "Local\\ClipCourier.SingleInstance", out created))
            {
                if (!created)
                {
                    MessageBox.Show("ClipCourier is already running. Look for its icon in the notification area.",
                        "ClipCourier", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
                {
                    AppLog.Write(e.Exception);
                    MessageBox.Show("ClipCourier encountered an error. Details were saved in its log folder.\r\n\r\n" + e.Exception.Message,
                        "ClipCourier", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
                {
                    Exception error = e.ExceptionObject as Exception;
                    if (error != null) AppLog.Write(error);
                };

                AppConfig config;
                try { config = ConfigStore.Load(); }
                catch (Exception ex)
                {
                    AppLog.Write(ex);
                    MessageBox.Show("Settings could not be read. ClipCourier will start with fresh settings.\r\n\r\n" + ex.Message,
                        "ClipCourier", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    config = new AppConfig();
                }
                Application.Run(new TrayApplication(config));
            }
        }
    }

}
