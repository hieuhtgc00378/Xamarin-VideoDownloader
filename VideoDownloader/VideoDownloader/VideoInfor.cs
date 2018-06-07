using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoDownloader
{
    public class VideoInfor
    {




        public string Title { get; set; }
        public VideoType Type { get; set; }
        public int FormatCode { get; set; }
        public int Resolution { get; set; }
        public string DownloadUrl { get; set; }
        public AdaptiveType AdaptiveType { get; set; }
        public bool RequiresDecryption { get; set; }
        public string Html5PlayerVersion { get; set; }
        public bool IsLiveStream { get; set; }
        public string BasURL { get; set; }
        public string Path { get; set; }
        public string VideoExtension
        {
            get
            {
                switch (this.Type)
                {
                    case VideoType.Mp4:
                        return ".mp4";

                    case VideoType.Mobile:
                        return ".3gp";

                    case VideoType.Flash:
                        return ".flv";

                    case VideoType.WebM:
                        return ".webm";
                    case VideoType.m3u8:
                        return ".m3u8";
                }

                return null;
            }
        }



        public VideoInfor(int formatCode, VideoType type, int resolution, AdaptiveType adaptiveType)
        {
            this.Type = type;
            this.FormatCode = formatCode;
            this.Resolution = resolution;
            this.AdaptiveType = adaptiveType;
        }

        public VideoInfor()
        {

        }


        internal static List<VideoInfor> Defaults = new List<VideoInfor>
        {
            /* Non-adaptive */
            new VideoInfor(5, VideoType.Flash, 240, AdaptiveType.None),
            new VideoInfor(6, VideoType.Flash, 270, AdaptiveType.None),
            new VideoInfor(13, VideoType.Mobile, 0,  AdaptiveType.None),
            new VideoInfor(17, VideoType.Mobile, 144,  AdaptiveType.None),
            new VideoInfor(18, VideoType.Mp4, 360, AdaptiveType.None),
            new VideoInfor(22, VideoType.Mp4, 720,  AdaptiveType.None),
            new VideoInfor(34, VideoType.Flash, 360,  AdaptiveType.None),
            new VideoInfor(35, VideoType.Flash, 480,   AdaptiveType.None),
            new VideoInfor(36, VideoType.Mobile, 240,  AdaptiveType.None),
            new VideoInfor(37, VideoType.Mp4, 1080,  AdaptiveType.None),
            new VideoInfor(38, VideoType.Mp4, 3072, AdaptiveType.None),
            new VideoInfor(43, VideoType.WebM, 360,  AdaptiveType.None),
            new VideoInfor(44, VideoType.WebM, 480,  AdaptiveType.None),
            new VideoInfor(45, VideoType.WebM, 720, AdaptiveType.None),
            new VideoInfor(46, VideoType.WebM, 1080, AdaptiveType.None),

            /* 3d */
            new VideoInfor(82, VideoType.Mp4, 360,AdaptiveType.None),
            new VideoInfor(83, VideoType.Mp4, 240, AdaptiveType.None),
            new VideoInfor(84, VideoType.Mp4, 720, AdaptiveType.None),
            new VideoInfor(85, VideoType.Mp4, 520, AdaptiveType.None),
            new VideoInfor(100, VideoType.WebM, 360, AdaptiveType.None),
            new VideoInfor(101, VideoType.WebM, 360,  AdaptiveType.None),
            new VideoInfor(102, VideoType.WebM, 720, AdaptiveType.None),

            /* Adaptive (aka DASH) - Video */
            new VideoInfor(133, VideoType.Mp4, 240, AdaptiveType.Video),
            new VideoInfor(134, VideoType.Mp4, 360,  AdaptiveType.Video),
            new VideoInfor(135, VideoType.Mp4, 480, AdaptiveType.Video),
            new VideoInfor(136, VideoType.Mp4, 720,  AdaptiveType.Video),
            new VideoInfor(137, VideoType.Mp4, 1080, AdaptiveType.Video),
            new VideoInfor(138, VideoType.Mp4, 2160,  AdaptiveType.Video),
            new VideoInfor(160, VideoType.Mp4, 144,  AdaptiveType.Video),
            new VideoInfor(242, VideoType.WebM, 240, AdaptiveType.Video),
            new VideoInfor(243, VideoType.WebM, 360,  AdaptiveType.Video),
            new VideoInfor(244, VideoType.WebM, 480,  AdaptiveType.Video),
            new VideoInfor(247, VideoType.WebM, 720,  AdaptiveType.Video),
            new VideoInfor(248, VideoType.WebM, 1080,  AdaptiveType.Video),
            new VideoInfor(264, VideoType.Mp4, 1440,  AdaptiveType.Video),
            new VideoInfor(271, VideoType.WebM, 1440, AdaptiveType.Video),
            new VideoInfor(272, VideoType.WebM, 2160,  AdaptiveType.Video),
            new VideoInfor(278, VideoType.WebM, 144,  AdaptiveType.Video),

            /* Adaptive (aka DASH) - Audio */
            new VideoInfor(139, VideoType.Mp4, 0, AdaptiveType.Audio),
            new VideoInfor(141, VideoType.Mp4, 0, AdaptiveType.Audio),
            new VideoInfor(171, VideoType.WebM, 0, AdaptiveType.Audio),
            new VideoInfor(172, VideoType.WebM, 0, AdaptiveType.Audio)
        };

        public override string ToString()
        {
            return Title;
        }
    }
}
