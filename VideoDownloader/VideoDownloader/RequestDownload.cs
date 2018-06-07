using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoDownloader
{
    class RequestDownload
    {
        public string contentId { get; set; }
        public string contentTitle { get; set; }
        public string baseURL { get; set; }
        public string pathToSaveVideo { get; set; }
        public string SourceId { get; set; }

        public RequestDownload()
        {

        }

        public RequestDownload(string contentId, string contenttile, string baseURL, string pathToSaveVideo, string SourceId)
        {
            this.contentId = contentId;
            this.contentTitle = contentTitle;
            this.baseURL = baseURL;
            this.pathToSaveVideo = pathToSaveVideo;
            this.SourceId = SourceId;
        }
    }
}
