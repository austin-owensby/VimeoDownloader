using System.Text.Json.Serialization;

namespace VimeoDownloader
{
    internal class PaginationResponse<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PerPage { get; set; }
        public Paging Paging { get; set; } = new();
        public List<T> Data { get; set; } = [];

    }

    internal class Paging
    {
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public string? First { get; set; }
        public string? Last { get; set; }
    }
}
