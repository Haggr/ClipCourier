using System;
using System.IO;
using System.Text;

namespace ClipCourier.Services
{
    internal static class TextFileAppender
    {
        // Create a missing file, but require its parent folder to exist.
        public static void Append(string path, string text)
        {
            path = Environment.ExpandEnvironmentVariables(path);
            string folder = Path.GetDirectoryName(path);
            if (String.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                throw new DirectoryNotFoundException("The destination folder does not exist: " + folder);

            bool exists = File.Exists(path);
            long length = exists ? new FileInfo(path).Length : 0;
            Encoding encoding = exists && length > 0 ? DetectEncoding(path) : new UTF8Encoding(false);
            bool needsLineBreak = length > 0 && !EndsWithLineBreak(path, encoding, length);
            using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (StreamWriter writer = new StreamWriter(stream, encoding))
            {
                if (needsLineBreak) writer.WriteLine();
                writer.Write(text);
            }
        }

        // Separate entries by one line, while preserving a line break already at EOF.
        private static bool EndsWithLineBreak(string path, Encoding encoding, long length)
        {
            byte[] lineFeed = encoding.GetBytes("\n");
            byte[] carriageReturn = encoding.GetBytes("\r");
            int count = Math.Max(lineFeed.Length, carriageReturn.Length);
            byte[] tail = new byte[count];
            using (FileStream stream = File.OpenRead(path))
            {
                stream.Seek(-Math.Min(length, count), SeekOrigin.End);
                int read = stream.Read(tail, 0, (int)Math.Min(length, count));
                return EndsWith(tail, read, lineFeed) || EndsWith(tail, read, carriageReturn);
            }
        }

        private static bool EndsWith(byte[] value, int length, byte[] suffix)
        {
            if (length < suffix.Length) return false;
            for (int i = 0; i < suffix.Length; i++)
                if (value[length - suffix.Length + i] != suffix[i]) return false;
            return true;
        }

        // Preserve known Unicode BOMs; files without a BOM follow the app's UTF-8 default.
        private static Encoding DetectEncoding(string path)
        {
            byte[] bytes = new byte[4];
            using (FileStream stream = File.OpenRead(path))
            {
                int read = 0;
                while (read < bytes.Length)
                {
                    int count = stream.Read(bytes, read, bytes.Length - read);
                    if (count == 0) break;
                    read += count;
                }
            }
            if (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) return new UTF8Encoding(true);
            if (bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0 && bytes[3] == 0) return new UTF32Encoding(false, true);
            if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xFE && bytes[3] == 0xFF) return new UTF32Encoding(true, true);
            if (bytes[0] == 0xFF && bytes[1] == 0xFE) return Encoding.Unicode;
            if (bytes[0] == 0xFE && bytes[1] == 0xFF) return Encoding.BigEndianUnicode;
            return new UTF8Encoding(false);
        }
    }
}
