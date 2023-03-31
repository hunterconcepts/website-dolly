using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using Hci.WebsiteDolly.Core.Business;
using Microsoft.VisualBasic.FileIO;

namespace Hci.WebsiteDolly.Core.Utility
{
    public static class FileUtility
    {
        public static string GetFileName(string url, string folder, out string originalFileName)
        {

            int queryPos = url.IndexOf('?');

            if (queryPos == -1)
                queryPos = url.Length;

            int lastWhackPos = url.LastIndexOf('/', queryPos - 1, queryPos - 1) + 1;

            string fileName = NormalizeFileName(url.Substring(lastWhackPos, queryPos - lastWhackPos));

            originalFileName = fileName;

            fileName = GetUniqueName(fileName, folder);

            return fileName;
        }

        static string NormalizeFileName(string fileName)
        {
            string invalidCharacters = "\"*/:<>?\\|/";

            foreach (char c in invalidCharacters.ToCharArray())
            {
                fileName = fileName.Replace(c.ToString(), string.Empty);
            }

            return fileName;
        }

        static string GetUniqueName(string fileName, string folder)
        {
            fileName = NormalizeFileName(fileName);

            if (File.Exists(Path.Combine(folder, fileName)))
            {
                int extensionStart = fileName.IndexOf('.');

                string file;
                string ext;

                if (extensionStart != -1)
                {
                    file = fileName.Substring(0, extensionStart);
                    ext = fileName.Substring(extensionStart);
                }
                else
                {
                    file = fileName;
                    ext = string.Empty;
                }

                int i = 0;

                do
                {
                    i++;

                    string newFile = file + "[" + i.ToString() + "]" + ext;

                    if (!File.Exists(Path.Combine(folder, newFile)))
                    {
                        return newFile;
                    }
                }
                while (true);
            }
            else
            {
                return fileName;
            }
        }

        public static bool WriteUrlToFile(string url, string fileName, string originalFileName, out bool useOriginal)
        {
            useOriginal = false;

            string contentType;
            long contentLength;

            return WriteUrlToFile(url, fileName, originalFileName, out useOriginal, out contentType, out contentLength);
        }

        public static bool WriteUrlToFile(string url, string fileName, string originalFileName, out bool useOriginal, out string contentType, out long contentLength)
        {
            int chunkSize = EnvironmentManager.ApplicationSettings.ChunkSize;
            int requestTimeout = EnvironmentManager.ApplicationSettings.RequestTimeout;

            useOriginal = false;

            try
            {
                //
                // Open the requested URL
                //
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                req.AllowAutoRedirect = true;
                req.MaximumAutomaticRedirections = 3;
                req.UserAgent = "Mozilla/6.0 (MSIE 6.0; Windows NT 5.1; WebCrawler)";
                req.KeepAlive = true;
                req.Timeout = requestTimeout;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                //
                // Get the stream from the returned web response
                //
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        contentType = resp.ContentType;
                        contentLength = resp.ContentLength;

                        byte[] buffer = new byte[chunkSize];
                        int read;

                        using (Stream inS = resp.GetResponseStream())
                        using (Stream outS = File.OpenWrite(fileName))
                        {
                            Stream readStream;

                            if (resp.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                                readStream = new GZipStream(inS, CompressionMode.Decompress);
                            else if (resp.ContentEncoding.Equals("deflate", StringComparison.InvariantCultureIgnoreCase))
                                readStream = new DeflateStream(inS, CompressionMode.Decompress);
                            else
                                readStream = inS;

                            using (readStream)
                            {
                                while ((read = readStream.Read(buffer, 0, chunkSize)) > 0)
                                {
                                    outS.Write(buffer, 0, read);
                                }
                            }
                        }

                        if (fileName == originalFileName)
                        {
                        }
                        else
                        {
                            if (File.Exists(originalFileName))
                            {
                                FileInfo newFile = new FileInfo(fileName);
                                FileInfo originalFile = new FileInfo(originalFileName);

                                if (newFile.Length == originalFileName.Length)
                                {
                                    useOriginal = true;

                                    newFile.Delete();
                                }
                            }
                        }

                    }
                    else
                    {
                        return false;
                    }
                }

            }
            catch
            {
                //
                // TODO: handle
                //
                return false;
            }
            finally
            {
                contentType = null;
                contentLength = -1;
            }

            return true;
        }
        
        public static void NormalizeDirectory(string path)
        {
            NormalizeDirectory(new DirectoryInfo(path));
        }

        static void NormalizeDirectory(DirectoryInfo directory)
        {
            NormalizeFiles(directory.GetFiles());
            NormalizeDirectories(directory.GetDirectories());
        }

        static void NormalizeDirectories(DirectoryInfo[] directories)
        {
            foreach (DirectoryInfo sub in directories)
            {
                NormalizeDirectory(sub);
            }
        }

        static void NormalizeFiles(FileInfo[] files)
        {
            foreach (FileInfo file in files)
            {
                if (file.Name.Contains("["))
                {
                    string bestFileName = file.Name.Substring(0, file.Name.IndexOf("["));

                    string bestPath = file.FullName.Replace(file.Name, bestFileName) + file.Extension;

                    if (File.Exists(bestPath))
                    {
                        FileInfo bestFile = new FileInfo(bestPath);

                        if (bestFile.Length == file.Length && bestFile.CreationTime == file.CreationTime)
                        {
                            file.Delete();
                        }
                    }
                    else
                    {
                        file.CopyTo(bestPath);
                        file.Delete();
                    }
                }
                else
                {
                    //
                    // Skip
                    //
                }
            }
        }

        public static void DeleteDirectory(string path)
        {
            DeleteDirectory(path, false);
        }

        public static void DeleteDirectory(string path, bool root)
        {
            UIOption option = root ? UIOption.AllDialogs : UIOption.OnlyErrorDialogs;

            //
            // Move contents to Recyling Bin
            //
            DirectoryInfo directory = new DirectoryInfo(path);

            foreach (DirectoryInfo sub in directory.GetDirectories())
            {
                DeleteDirectory(sub.FullName);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, Microsoft.VisualBasic.FileIO.UICancelOption.DoNothing);
            }

            FileSystem.DeleteDirectory(directory.FullName, option, RecycleOption.SendToRecycleBin);
        }
    }
}
