using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace VideoDownloader
{
    public partial class MainPage : TabbedPage
    {
        public const string HLS_HOST = "https://hls.mediacdn.vn/";
        public const string HLS = "http://vcplayer.mediacdn.vn";
        public const string HLS_1 = "http://vcplayer.vcmedia.vn";
        public const string FACEBOOK_VIDEO_BASE = "https://www.facebook.com/plugins/video.php";
        public string[] HLS_REGEX = { @"dantri/(.*?)(\.mp4)", @"dantri/(.*?)(\.flv)" };

        public static string[] types = { "mp4", "flv", "m3u8", "3gp" };

        public const string YOUTUBE_LINK_01 = "youtube-nocookie.com";
        public const string YOUTUBE_LINK_02 = "youtube.com";
        ObservableCollection<VideoInfor> videos = null;
        public MainPage()
        {
            InitializeComponent();
            try
            {
                videos = RetrieveVideoList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
            if (null != videos)
            {
                if (videos.Count > 0)
                {
                    downloadedListView.ItemsSource = videos;
                }
            }
        }

        void OnBtnStartDownloadSourceClicked(object sender, EventArgs args)
        {

        }

        void OnBtnbtnStartDownloadUrlClicked(object sender, EventArgs args)
        {
            RequestDownload request = new RequestDownload();
            request.baseURL = txtPageUrl.Text;
            object[] objs = new object[2];
            objs[0] = request;
            objs[1] = "1";
            Thread t = new Thread(new ParameterizedThreadStart(findURLandDownload));
            t.Start(objs);
            //findURLandDownload(objs);

        }

        public async void findURLandDownload(object args)
        {
            object[] param = args as object[];
            RequestDownload request = param[0] as RequestDownload;
            string isRetriedString = param[1] as string;
            int isRetried = Convert.ToInt32(isRetriedString);
            try
            {

                string url = request.baseURL;
                List<string> urlsWithIframe = new List<string>();
                List<string> urlsToDownload = new List<string>();
                List<string> urlsWithFindLink = new List<string>();
                downloadAsync(urlsWithFindLink, urlsWithIframe, urlsToDownload, url, request);
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception line : " + ex.ToString());
                if (isRetried == 1)
                {
                    object[] objs = new object[2];
                    objs[0] = request;
                    objs[1] = 0;
                    findURLandDownload(objs);
                }

            }
            finally
            {

            }
        }


        private async void downloadAsync(List<string> urlsWithFindLink, List<string> urlsWithIframe, List<string> urlsToDownload, string url, RequestDownload request)
        {
            try
            {
                string pageSource = "";

                pageSource = getPageSource(url);
                urlsWithFindLink = extractURLFromHTMLHasVideoURL(url);

                if (pageSource.Contains("<iframe"))
                {
                    //urlsWithIframe = extractURLFromIframeTag(path, url);
                }

                if (null != urlsWithFindLink)
                {
                    for (int i = 0; i < urlsWithFindLink.Count; i++)
                    {
                        if (!urlsToDownload.Contains(urlsWithFindLink[i]))
                        {
                            urlsToDownload.Add(urlsWithFindLink[i]);
                        }
                    }
                }

                if (null != urlsWithIframe)
                {
                    for (int i = 0; i < urlsWithIframe.Count; i++)
                    {
                        if (!urlsToDownload.Contains(urlsWithIframe[i]))
                        {
                            urlsToDownload.Add(urlsWithIframe[i]);
                        }
                    }
                }



                if (null != urlsToDownload)
                {
                    for (int i = 0; i < urlsToDownload.Count; i++)
                    {
                        try
                        {
                            VideoInfor video = new VideoInfor();
                            video.DownloadUrl = WebUtility.UrlDecode(urlsToDownload[i]);
                            video.BasURL = url;
                            string[] splitedURL = urlsToDownload[i].Split('/');
                            string title = RemoveIllegalPathCharacters(splitedURL[splitedURL.Length - 1]).ToLower();
                            if (!title.Contains(".mp4") && !title.Contains(".flv") && !title.Contains(".3gp") && !video.DownloadUrl.Contains(".m3u8"))
                            {
                                title = title + ".mp4";
                            }
                            video.Title = title;

#if __ANDROID__
                            var dir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim);
                            var newFilepath = System.IO.Path.Combine(dir.AbsolutePath, title);
                            video.Path = newFilepath;
#endif
#if __IOS__
                            video.Path = System.IO.Path.Combine(DependencyService.Get<IDownloadState>().OnDownloadStarted(), title);
#endif
                            new FileInfo(video.Path).Directory.Create();
                            downloadVideo(video, false);
                        }
                        catch (Exception ex)
                        {
                            txtPageSource.Text = ex.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }


        private static string getPageSource(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "SO/1.0";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = null;

                    if (response.CharacterSet == null)
                    {
                        readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    }
                    else
                    {
                        readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    }
                    string data = WebUtility.HtmlDecode(readStream.ReadToEnd());
                    response.Close();
                    readStream.Close();
                    return data;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }


        public async void downloadVideo(VideoInfor video, bool isRetried)
        {
            if (!video.DownloadUrl.Contains(HLS) && !video.DownloadUrl.Contains(HLS_1))
            {
                string filepath = video.Path;

                if (Uri.IsWellFormedUriString(video.DownloadUrl, UriKind.RelativeOrAbsolute) && !video.DownloadUrl.Contains("https://v.vnecdn.net/vnexpress/video/video_default.mp4"))
                {
                    video.DownloadUrl = video.DownloadUrl.Replace("&amp;", "&");
                    Console.WriteLine("Downloading url:  " + video.DownloadUrl);
                    try
                    {
                        if (!video.DownloadUrl.Contains(".m3u8"))
                        {
                            Console.WriteLine("Downloading.....");

                            var request = (HttpWebRequest)WebRequest.Create(video.DownloadUrl);
                            if (isRetried)
                            {
                                request.Timeout = 120000;
                            }
                            using (WebResponse response = request.GetResponse())
                            {
                                using (Stream source = response.GetResponseStream())
                                {
                                    using (FileStream target = File.Open(filepath, FileMode.Create, System.IO.FileAccess.Write))
                                    {
                                        var buffer = new byte[1024];
                                        bool cancel = false;
                                        int bytes;
                                        int copiedBytes = 0;
                                        while (!cancel && (bytes = source.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            target.Write(buffer, 0, bytes);
                                            copiedBytes += bytes;
                                            Double percent = ((Double)copiedBytes / (Double)source.Length) * 100;
                                            Console.WriteLine("downloading  " + percent + "%");
                                            Device.BeginInvokeOnMainThread(() =>
                                            {
                                                progressBar1.Progress = percent / 100;
                                                percent1.Text = (int)percent + " %";
                                            });

                                        }
                                        if (null == videos)
                                        {
                                            videos = RetrieveVideoList();
                                            downloadedListView.ItemsSource = videos;
                                        }
                                        else
                                        {
                                            videos.Add(video);
                                        }
                                    }
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IDownloadState>().OnDownloadFinished(video.Path);
                                    });
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception download url = " + video.DownloadUrl);
                        Console.WriteLine("Exception download url Exception = " + ex.ToString());
                        if (!isRetried)
                        {
                            downloadVideo(video, true);
                        }
                        else
                        {
                            // DependencyService.Get<IDownloadState>().OnDownloadError();
                        }

                    }
                }
            }
        }

        string[] prefixs = { "filename=", "src=", "file=", "url=", "href=", "http://", "https://", "file:", "data-vid=" };
        private List<String> extractURLFromHTMLHasVideoURL(string url)
        {
            try
            {
                string pageSource = "";
                string baseUrl = "";
                baseUrl = url;
                pageSource = getPageSource(url).ToLower();
                List<string> urls = new List<string>();
                foreach (string prefix in prefixs)
                {
                    foreach (string t in types)
                    {
                        Regex dataregex = new Regex("(" + prefix + @")(.*?)(\.)(" + t + ")", RegexOptions.Multiline);
                        Uri uri = new Uri(baseUrl);
                        string host = baseUrl.Split(new string[] { "://" }, StringSplitOptions.None)[0] + "://" + uri.Host;
                        if (dataregex.IsMatch(pageSource))
                        {
                            Match match = dataregex.Match(pageSource);
                            string firstURL = match.Groups[0].ToString();
                            firstURL = normalizeURL(firstURL, host);
                            urls.Add(firstURL);
                            string hlsUrl = findVideoInHLS(firstURL);

                            if (!string.IsNullOrEmpty(hlsUrl) && !urls.Contains(hlsUrl))
                            {
                                urls.Add(hlsUrl);
                            }
                            string subString = pageSource.Substring(pageSource.IndexOf(match.Groups[0].ToString()) + match.Groups[0].ToString().Length);
                            while (dataregex.IsMatch(subString))
                            {
                                match = dataregex.Match(subString);
                                string link = match.Groups[0].ToString();
                                string[] splitedURL = link.Split('-');
                                string videoID = splitedURL[splitedURL.Length - 1];
                                bool contained = false;
                                for (int i = 0; i < urls.Count; i++)
                                {
                                    if (urls[i].Contains(videoID) || urls[i] == link)
                                    {
                                        contained = true;
                                        break;
                                    }
                                }
                                if (!contained)
                                {
                                    link = normalizeURL(link, host);
                                    string hlsUrl2 = findVideoInHLS(link);
                                    if (!string.IsNullOrEmpty(hlsUrl2) && !urls.Contains(hlsUrl2))
                                    {
                                        urls.Add(hlsUrl2);
                                    }
                                    if (!link.Contains(@"\"))
                                    {
                                        urls.Add(link);
                                    }
                                }
                                subString = subString.Substring(subString.IndexOf(match.Groups[0].ToString()) + match.Groups[0].ToString().Length);
                            }
                        }
                    }
                }

                return urls;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        private string normalizeURL(string url, string host)
        {
            Console.WriteLine("normalizing url: " + url + " - - host: " + host);
            if (url.LastIndexOf("http") > 0)
            {
                url = url.Substring(url.LastIndexOf("http"));
            }
            else if (url.IndexOf("http") < 0)
            {
                if (url.Contains("data-vid"))
                {
                    if (url.StartsWith("/"))
                    {
                        try
                        {
                            url = url.Substring(1, url.Length - 1);
                        }
                        catch
                        {

                        }
                    }
                    url = HLS_HOST + url;
                }
                url = url.Replace("filename=", "");
                url = url.Replace("src=", "");
                url = url.Replace("file=", "");
                url = url.Replace("url=", "");
                url = url.Replace("href=", "");
                url = url.Replace("file:", "");
                url = url.Replace("data-vid=", "");
                url = url.Replace("\'", "");
                url = url.Replace("\"", "");
                url = url.Replace(" ", "");
                if (!url.Contains(HLS_HOST))
                {
                    if (url.StartsWith("/"))
                    {
                        url = host + url;
                    }
                    else
                    {
                        url = host + "/" + url;
                    }
                }
            }
            Console.WriteLine("Done normalize url, final url = " + WebUtility.UrlDecode(url));
            return WebUtility.UrlDecode(url);
        }

        private string findVideoInHLS(string url)
        {
            if (url.Contains(HLS) || url.Contains(HLS_1))
            {
                foreach (string regex in HLS_REGEX)
                {
                    Regex r = new Regex(regex);
                    if (r.IsMatch(url))
                    {
                        return WebUtility.UrlDecode(HLS_HOST + r.Match(url).Groups[0].ToString());
                    }
                }
            }
            return string.Empty;
        }


        //public string getAndroidVideoPath(string filename)
        //{
        //    var dir = Android.OS.Environment.GetExternalStoragePublicDirectory(
        //    Android.OS.Environment.DirectoryDcim);
        //    //var dir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        //    var pictures = dir.AbsolutePath;
        //    var newFilepath = System.IO.Path.Combine(pictures, filename);
        //    return newFilepath;
        //}


        //private List<string> extractURLFromIframeTag(string path, string url)
        //{
        //    try
        //    {
        //        List<string> urls = null;
        //        try
        //        {
        //            string pageSource = getPageSource(url).ToLower();
        //            if (pageSource.Contains("<iframe"))
        //            {
        //                var htmlDoc = new XmlDocument();
        //                htmlDoc.LoadXml(pageSource);
        //                urls = new List<string>();
        //                var htmlNodes = htmlDoc.DocumentNode.SelectNodes(".//iframe");
        //                if (null != htmlNodes)
        //                {
        //                    foreach (var node in htmlNodes)
        //                    {
        //                        string iframeSrc = node.Attributes["src"].Value;
        //                        if (!string.IsNullOrEmpty(iframeSrc))
        //                        {

        //                            if (!iframeSrc.Contains(YOUTUBE_LINK_01) && !iframeSrc.Contains(YOUTUBE_LINK_02))
        //                            {
        //                                if (!iframeSrc.Contains("google.com/") && !iframeSrc.Contains("twitter.com") && !iframeSrc.Contains("eclick.vn") && !iframeSrc.Contains("googletagmanager.com"))
        //                                {
        //                                    if (!iframeSrc.Contains(FACEBOOK_VIDEO_BASE))
        //                                    {

        //                                        iframeSrc = iframeSrc.Replace("&amp;", "&");
        //                                        if (iframeSrc != "" && iframeSrc != null && !urls.Contains(iframeSrc))
        //                                        {
        //                                            List<string> links = extractURLFromHTMLHasVideoURL(null, iframeSrc);
        //                                            if (null != links)
        //                                            {
        //                                                urls.AddRange(links);
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        FacebookDownloader.Downloader downloader = new FacebookDownloader.Downloader();
        //                                        urls.AddRange(downloader.findURL(iframeSrc));
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                Console.WriteLine("there is youtube embeded in iframe, calling youtubedownloader...");
        //                                WriteLog("there is youtube embeded in iframe,youtube link = " + iframeSrc + "| calling youtubedownloader...");
        //                                Lcbc_YouTubeDownloader.YouTubeDownloader.Instant().findLinkAndDownload(iframeSrc, path);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            WriteLog("Exception in extractURLFromIframeTag, ex = " + ex.ToString());
        //        }
        //        return urls;
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteLog("Exception extractURLFromIframeTag, ex = " + ex.ToString());
        //        return new List<string>();
        //    }
        //}

        private ObservableCollection<VideoInfor> RetrieveVideoList()
        {
            string path = string.Empty;
#if __ANDROID__
            path = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;
#endif
#if __IOS__
            path = DependencyService.Get<IDownloadState>().OnDownloadStarted();
#endif
            //string[] filePaths = Directory.GetFiles(path, "*.mp4");
            string[] array1 = Directory.GetFiles(path, "*.flv");
            string[] array2 = Directory.GetFiles(path, "*.mp4");
            int array1OriginalLength = array1.Length;
            Array.Resize<string>(ref array1, array1OriginalLength + array2.Length);
            Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
            if (null != array2)
            {
                if (array2.Length > 0)
                {
                    ObservableCollection<VideoInfor> videos = new ObservableCollection<VideoInfor>();
                    foreach (string p in array2)
                    {
                        VideoInfor v = new VideoInfor();
                        v.Path = p;
                        int e = p.LastIndexOf(@"/");
                        v.Title = p.Substring(p.LastIndexOf(@"/"), (p.Length - 1) - p.LastIndexOf(@"/"));
                        v.Title = v.Title.Replace(@"/", "");
                        v.Title = v.Title.Replace(".mp4", "");
                        v.Title = v.Title.Replace(".flv", "");
                        videos.Add(v);
                    }
                    return videos;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

    }
}
