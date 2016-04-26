using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Code.External.Engine.Sqlite
{
    public static class ZipHelper
    {
        public static void Zip(string baseDir, string zipPath)
        {
            using (FileStream fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            {
                Zip(baseDir, fs);
            }
        }
        public static void Zip(string baseDir, Stream output)
        {
            ZipEntry entry;
            ZipConstants.DefaultCodePage = System.Text.Encoding.Default.CodePage;
            using (ZipOutputStream s = new ZipOutputStream(output))
            {
                string fileName = null;
                baseDir = baseDir.Trim();
                int startIndex = baseDir.Length;
                if (!(baseDir.EndsWith("/") || baseDir.EndsWith("\\")))
                    startIndex++;
                foreach (var dir in Directory.GetDirectories(baseDir, "*", SearchOption.AllDirectories))
                {
                    fileName = dir.Substring(startIndex);
                    fileName = fileName + "/";
                    entry = new ZipEntry(fileName);

                    entry.DateTime = DateTime.UtcNow;
                    s.PutNextEntry(entry);

                    Debug.Log(fileName);
                }

                foreach (var path in Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories))
                {

                    fileName = path.Substring(startIndex);
                    entry = new ZipEntry(fileName);

                    entry.DateTime = DateTime.UtcNow;
                    s.PutNextEntry(entry);
                    byte[] bytes = File.ReadAllBytes(path);
                    s.Write(bytes, 0, bytes.Length);
                    Debug.Log(fileName);
                }

                s.Flush();
            }
        }
        public static string[] Unzip(string zipPath, string baseDir)
        {

            if (!File.Exists(zipPath))
            {
                Debug.LogError("no file ," + zipPath);
                return new string[0];
            }

            using (FileStream fs = File.OpenRead(zipPath))
            {
                return Unzip(fs, baseDir);
            }
        }
        public static string[] Unzip(Stream s, string baseDir)
        {
            List<string> filePaths = new List<string>();

            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            ZipConstants.DefaultCodePage = System.Text.Encoding.Default.CodePage;

            using (ZipInputStream zis = new ZipInputStream(s))
            {
                ZipEntry entry;

                byte[] buffer = new byte[1024 * 4];
                string filePath;
                while (true)
                {
                    entry = zis.GetNextEntry();
                    if (entry == null)
                        break;
                    if (entry.IsDirectory)
                    {
                        filePath = baseDir + "/" + entry.Name;
                        if (Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);
                        continue;
                    }

                    if (entry.IsFile)
                    {
                        filePath = baseDir + "/" + entry.Name;
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            while (true)
                            {
                                int readCount = zis.Read(buffer, 0, buffer.Length);
                                if (readCount <= 0)
                                    break;
                                fs.Write(buffer, 0, readCount);
                            }
                        }
                        filePaths.Add(filePath);
                    }
                }
            }

            return filePaths.ToArray();
        }
        public static IEnumerable<byte[]> Unzip(Stream input)
        {
            using (ZipInputStream s = new ZipInputStream(input))
            {
                ZipEntry entry;

                entry = s.GetNextEntry();
                if (entry == null)
                    throw new Exception("zip null entry");
                byte[] buffer = new byte[1024 * 4];
                MemoryStream outMs = new MemoryStream(new byte[entry.Size]);
                while (true)
                {
                    int readCount = s.Read(buffer, 0, buffer.Length);
                    if (readCount <= 0)
                        break;
                    outMs.Write(buffer, 0, readCount);
                }

                var data = outMs.ToArray();
                yield return data;
            }
        }

    }
}
