using System;
using System.Collections.Generic;
using System.Text;

namespace VideoDownloader
{
    public interface IDownloadState
    {
        string OnDownloadStarted();
        void OnDownloadError();
        void OnDownloadFinished(string path);
    }
}
