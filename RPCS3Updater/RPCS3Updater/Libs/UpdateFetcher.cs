using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static RPCS3Updater.Libs.Logger;
using static RPCS3Updater.Libs.Globals;
using SevenZipExtractor;
using System.Diagnostics;
using System.Threading;

namespace RPCS3Updater.Libs
{
    public static class UpdateFetcher
    {
        public const string release_url = @"https://api.github.com/repos/RPCS3/rpcs3-binaries-win/releases/latest";
        public static string JsonData = "";
        public static string CurrentVersion = "";
        public static RPCS3 Build;

        public static void Start()
        {
            CheckforUpdate();
        }
        public static bool CheckforUpdate()
        {
            bool result = false;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(release_url);
                request.UserAgent = "request";
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    JsonData = reader.ReadToEnd();
                    if (JsonData != null || JsonData != "") Build = Parse();
                    if (Build != null)
                        CurrentVersion = GetVersionFromLog(Build.Version);
                }
            }
            catch (HttpRequestException e)
            {
                Write("Http request error: {0}\nPlease make sure you have an active internet connection!", e.Message);
            }
            catch (WebException e)
            {
                Write("Http request error: {0}\nPlease make sure you have an active internet connection!", e.Message);
            }
            return result;
        }
        public static RPCS3 Parse()
        {
            RPCS3 _build = new RPCS3();
            try
            {
                var _release = (JObject)JsonConvert.DeserializeObject(JsonData);
                if (_release != null)
                {
                    _build.DownloadLink = _release.SelectToken("assets[0].browser_download_url").Value<string>();
                    _build.Version = _release.SelectToken("name").Value<string>();
                    _build.ReleaseTime = _release.SelectToken("published_at").Value<DateTime>();
                }
            }
            catch (JsonException e)
            {
                Write("Failed to read json data: {0}", e.Message);
            }
            return _build;
        }
        public static bool UpdateAvailable()
        {
            bool result = false;
            if (Build != null && CurrentVersion != "" && CurrentVersion != Build.Version)
                result = true;
            return result;

        }
        public static string GetVersionFromLog(string targetVersion)
        {
            string logFile = Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.log", ExecuteableName.ToUpper()));
            string versionPattern = @"([0-9]+.[0-9]+.[0-9]+-[0-9]+)";
            string result = "";
            if (File.Exists(logFile))
            {
                string text = File.ReadLines(logFile).First();
                Match match = Regex.Match(text, versionPattern);
                if (match.Success)
                {
                    result = match.Groups[1].Value;
                }
            }
            else
            {
                Write("Error: Couldn't find the log file: {0}! Please make sure {1} is inside {2}'s main folder.", ExecuteableName + ".log",
                    Application.ProductName, ExecuteableName);
            }
            return result;
        }
        public static void Update(ConsoleKey key)
        {
            while (true)
            {
                if (Console.ReadKey(true).Key == key)
                {
                    DownloadAndInstall();
                    break;
                }
                else break;
            }
        }
        public static void DownloadAndInstall()
        {
            string thisDir = Path.Combine(Directory.GetCurrentDirectory());
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Build.DownloadLink);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                string update = "";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    var fn = response.Headers["Content-Disposition"].Split(new string[] { "=" }, StringSplitOptions.None)[1].Replace("\"", "");
                    if (fn.Contains("?"))
                    {
                        fn = fn.Replace("?", "");
                    }
                    var responseStream = response.GetResponseStream();
                    string name = fn;

                    Write("Downloading update...");
                    // Otherwise do download.
                    using (var fileStream = File.Open(Path.Combine(thisDir, fn), FileMode.Create))
                    {
                        responseStream.CopyTo(fileStream);
                    }
                    update = Path.Combine(thisDir, fn);
                    Write("Download done. Extracting...");

                    ArchiveFile file = new ArchiveFile(update);
                    file.Extract(thisDir);

                    file.Dispose();
                    File.Delete(update);
                    Write("Update installed successfully!");
                    Thread.Sleep(1300);
                    Launch();
                }
            }
            catch (SevenZipException e)
            {
                Write("Archieve error: {0}", e.Message);
            }

        }
        public static void Launch()
        {
            string exec = Path.Combine(Directory.GetCurrentDirectory(), Executeable);
            if (File.Exists(exec))
                Process.Start(exec);
        }
    }
}
