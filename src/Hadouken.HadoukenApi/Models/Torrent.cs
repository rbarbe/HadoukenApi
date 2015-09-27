using System;

namespace Hadouken.HadoukenApi.Models
{
    public class Torrent
    {
        public string InfoHash { get; set; }
        public object State { get; set; }
        public string Name { get; set; }
        public float Progress { get; set; }
        public long DownloadedBytesTotal { get; set; }
        public long UploadedBytesTotal { get; set; }
        public float Ratio { get; set; }
        public TimeSpan Eta { get; set; }
        public string Label { get; set; }
        public long DownloadRate { get; set; }
        public long UploadRate { get; set; }
        public int Seeds { get; set; }
        public int Peers { get; set; }
        public long TotalSize { get; set; }
    }
}