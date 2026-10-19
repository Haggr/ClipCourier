using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using ClipCourier.Models;
using ClipCourier.Platform;

namespace ClipCourier.Infrastructure
{
    internal static class ConfigStore
    {
        public static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClipCourier");
        public static readonly string FilePath = Path.Combine(Folder, "settings.json");

        public static AppConfig Load()
        {
            string backupPath = FilePath + ".bak";
            if (!File.Exists(FilePath))
            {
                if (!File.Exists(backupPath)) return new AppConfig();
                AppConfig recovered = LoadFile(backupPath);
                AppLog.Write(new FileNotFoundException("The settings file was missing; settings were recovered from the backup.", FilePath));
                return recovered;
            }
            try
            {
                return LoadFile(FilePath);
            }
            catch (Exception primaryError)
            {
                if (!File.Exists(backupPath)) throw;

                try
                {
                    AppConfig recovered = LoadFile(backupPath);
                    AppLog.Write(new InvalidDataException("The settings file was unreadable; settings were recovered from the backup.", primaryError));
                    return recovered;
                }
                catch (Exception backupError)
                {
                    throw new InvalidDataException("Neither the settings file nor its backup could be read.",
                        new AggregateException(primaryError, backupError));
                }
            }
        }

        private static AppConfig LoadFile(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                AppConfig config = (AppConfig)new DataContractJsonSerializer(typeof(AppConfig)).ReadObject(stream);
                if (config == null) config = new AppConfig();
                if (config.Destinations == null) config.Destinations = new List<Destination>();

                // Use a default shortcut and normalize missing destination fields.
                if (!Native.IsValidHotkey(config.HotkeyModifiers, config.HotkeyKey))
                {
                    config.HotkeyKey = (int)Keys.J;
                    config.HotkeyModifiers = Native.ModControl | Native.ModAlt;
                }
                for (int i = config.Destinations.Count - 1; i >= 0; i--)
                {
                    Destination destination = config.Destinations[i];
                    if (destination == null) config.Destinations.RemoveAt(i);
                    else
                    {
                        if (destination.Name == null) destination.Name = "";
                        if (destination.Path == null) destination.Path = "";
                    }
                }
                return config;
            }
        }

        // Save atomically and retain the previous version as a backup for automatic recovery.
        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(Folder);
            string temp = FilePath + ".tmp";
            using (FileStream stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                new DataContractJsonSerializer(typeof(AppConfig)).WriteObject(stream, config);
            try
            {
                if (File.Exists(FilePath))
                {
                    string backup = FilePath + ".bak";
                    File.Replace(temp, FilePath, backup, true);
                }
                else File.Move(temp, FilePath);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }

}
