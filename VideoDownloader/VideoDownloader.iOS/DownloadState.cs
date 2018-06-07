using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using VideoDownloader.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(DownloadState))]
namespace VideoDownloader.iOS
{
    class DownloadState : IDownloadState
    {
        public void OnDownloadError()
        {
            throw new NotImplementedException();
        }

        public void OnDownloadFinished(string path)
        {
            throw new NotImplementedException();
        }

        public string OnDownloadStarted()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            return documentsPath;
        }
    }
}