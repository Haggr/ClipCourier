using System;
using System.IO;
using System.Text;

namespace ClipCourier.Infrastructure
{
    // Ignore logging failures so they do not mask the original error.
    internal static class AppLog
    {
        public static void Write(Exception error)
        {
            try
            {
                Directory.CreateDirectory(ConfigStore.Folder);
                File.AppendAllText(Path.Combine(ConfigStore.Folder, "ClipCourier.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + error + Environment.NewLine + Environment.NewLine,
                    new UTF8Encoding(false));
            }
            catch { }
        }
    }

}
