namespace VimeoDownloader.Models
{
    internal class VideoList
    {
        public List<VideoItem> VideoItems { get; set; } = [];
        public long Size { get; set; }
    }

    internal class VideoItem
    {
        public string? DownloadLink { get; set; }
        public string? FileName { get; set; }
        public long Size { get; set; }
    }
}
