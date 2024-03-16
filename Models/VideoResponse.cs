namespace VimeoDownloader.Models
{
    internal class VideoResponse
    {
        public string? Name { get; set; }
        public string? CreatedTime { get; set; }
        public List<Download> Download { get; set; } = [];
    }

    internal class Download
    {
        public string? Rendition { get; set; }
        public string? Link { get; set; }
        public long? Size { get; set; }
    }
}
