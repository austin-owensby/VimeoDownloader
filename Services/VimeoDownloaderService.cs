using System.Text.Json;
using VimeoDownloader.Models;

namespace VimeoDownloader.Services
{
    internal class VimeoDownloaderService
    {
        public readonly HttpClient client;
        private readonly JsonSerializerOptions jsonOptions;
        private readonly string userId;

        public VimeoDownloaderService(string apiKey, string userId)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

            this.userId = userId;
        }

        public async Task<VideoList?> GetVideosToDownload(int offset)
        {
            // Get list of folders and select one
            List<FoldersResponse>? folders = await GetFolders();

            if (folders == null)
            {
                return null;
            }

            FoldersResponse selectedFolder = SelectFolder(folders);

            // Fetch the list of videos and select a video size
            List<VideoResponse>? videos = await GetVideos(selectedFolder.URI, offset);

            if (videos == null)
            {
                return null;
            }

            (string, long) size = SelectSize(videos);

            int rendition = int.Parse(size.Item1.TrimEnd('p'));

            List<VideoItem> videoItems = [];

            foreach (VideoResponse video in videos)
            {
                Download? match = video.Download.FirstOrDefault(d => rendition == int.Parse(d.Rendition?.TrimEnd('p') ?? "0"));

                if (match == null)
                {
                    match = video.Download.Where(d => rendition < int.Parse(d.Rendition?.TrimEnd('p') ?? "0")).MaxBy(d => int.Parse(d.Rendition?.TrimEnd('p') ?? "0"));
                    match ??= video.Download.Where(d => rendition > int.Parse(d.Rendition?.TrimEnd('p') ?? "0")).MinBy(d => int.Parse(d.Rendition?.TrimEnd('p') ?? "0"));
                }

                DateTime createdDate = DateTime.Parse(video.CreatedTime!);

                videoItems.Add(new()
                {
                    DownloadLink = match!.Link,
                    FileName = MakeValidFileName($"{createdDate:yyyy-MM-dd_hh-mm-ss}-{video.Name}.mp4"),
                    Size = (long)match.Size!
                });
            }

            // Download each video
            VideoList response = new()
            {
                VideoItems = videoItems,
                Size = size.Item2
            };

            return response;
        }

        private async Task<List<VideoResponse>?> GetVideos(string? folderURI, int offset)
        {
            Console.WriteLine("Fetching list of videos...");
            string videosUrl = $"https://api.vimeo.com{folderURI}/videos?sort=date&direction=desc&fields=name,created_time,download.link,download.rendition,download.size";
            List<VideoResponse>? videos = await GetAllDataFromPages<VideoResponse>(videosUrl);

            if (videos == null)
            {
                return null;
            }

            if (videos.Count == 0)
            {
                Console.WriteLine("No videos found in folder.");
                return null;
            }

            if (videos.Count <= offset)
            {
                Console.WriteLine($"Offset was larger than total number of videos: {videos.Count}");
                return null;
            }

            videos = videos.Skip(offset).ToList();

            Console.WriteLine($"{videos.Count} videos found.");

            return videos;
        }

        private static (string, long) SelectSize(List<VideoResponse> videos)
        {
            List<string> sizes = videos.SelectMany(v => v.Download).Where(d => d.Rendition != null).Select(d => d.Rendition!).Distinct().OrderBy(s => int.Parse(s.TrimEnd('p'))).ToList();

            List<long> downloadSizes = [];

            var videosSimple = videos.Select(v => v.Download.Select(d => new { d.Size, Rendition = int.Parse(d.Rendition?.TrimEnd('p') ?? "0") })).ToList();

            foreach (string size in sizes)
            {
                int rendition = int.Parse(size.TrimEnd('p'));
                long downloadSize = 0;

                foreach (var video in videosSimple)
                {
                    var match = video.FirstOrDefault(d => d.Rendition == rendition);

                    if (match == null)
                    {
                        match = video.Where(d => d.Rendition < rendition).MaxBy(d => d.Rendition);
                        match ??= video.Where(d => d.Rendition > rendition).MinBy(d => d.Rendition);
                    }

                    downloadSize += match?.Size ?? 0;
                }

                downloadSizes.Add(downloadSize);
            }

            Console.WriteLine("Select a video size to download:");
            Console.WriteLine("If a size is not available for a video, the next smallest size will be used, and if still not available, the next largest size will be used.");

            int maxSize = sizes.Select(s => s.Length).Max();

            for (int i = 0; i < sizes.Count; i++)
            {
                string size = sizes[i];
                string downloadSize = BytesToString(downloadSizes[i]);
                Console.WriteLine($"{i + 1}) {size.PadLeft(maxSize)} {downloadSize}");
            }

            int sizeInput = 0;

            do
            {
                string? input = Console.ReadLine();

                if (input == null)
                {
                    Console.WriteLine("No input detected, please enter a number.");
                    continue;
                }

                if (!int.TryParse(input, out int parsedInput))
                {
                    Console.WriteLine("Unable to read input as a whole number.");
                    continue;
                }

                if (parsedInput <= 0 || parsedInput > sizes.Count)
                {
                    Console.WriteLine("Input was not a valid option.");
                    continue;
                }

                sizeInput = parsedInput;
            }
            while (sizeInput == 0);
            string selectedSize = sizes[sizeInput - 1];

            Console.WriteLine($"'{selectedSize}' size selected");

            return (selectedSize, downloadSizes[sizeInput - 1]);
        }

        private async Task<List<FoldersResponse>?> GetFolders()
        {
            Console.WriteLine("Fetching list of folders...");
            string foldersUrl = $"https://api.vimeo.com/users/{userId}/folders?sort=name&direction=asc&fields=name,uri";
            List<FoldersResponse>? folders = await GetAllDataFromPages<FoldersResponse>(foldersUrl);

            if (folders == null)
            {
                return null;
            }

            if (folders.Count == 0)
            {
                Console.WriteLine("No folders found.");
                return null;
            }

            return folders;
        }

        private static FoldersResponse SelectFolder(List<FoldersResponse> folders)
        {
            Console.WriteLine("Select a folder to download:");

            int magnitude = GetMagnitudeOfNumber(folders.Count);

            for (int i = 0; i < folders.Count; i++)
            {
                FoldersResponse folder = folders[i];
                Console.WriteLine($"{(i + 1).ToString().PadLeft(magnitude)}) {folder.Name}");
            }

            int folderInput = 0;

            do
            {
                string? input = Console.ReadLine();

                if (input == null)
                {
                    Console.WriteLine("No input detected, please enter a number.");
                    continue;
                }

                if (!int.TryParse(input, out int parsedInput))
                {
                    Console.WriteLine("Unable to read input as a whole number.");
                    continue;
                }

                if (parsedInput <= 0 || parsedInput > folders.Count)
                {
                    Console.WriteLine("Input was not a valid option.");
                    continue;
                }

                folderInput = parsedInput;
            }
            while (folderInput == 0);

            FoldersResponse selectedFolder = folders[folderInput - 1];

            Console.WriteLine($"'{selectedFolder.Name}' folder selected");

            return selectedFolder;
        }

        private async Task<List<T>?> GetAllDataFromPages<T>(string url)
        {
            List<T> list = [];

            PaginationResponse<T>? page = await GetPageOfData<T>($"{url}&page=1");

            if (page == null)
            {
                return null;
            }

            list.AddRange(page.Data);

            while (page.Paging.Next != null)
            {
                Console.WriteLine("Fetching next page of data...");

                page = await GetPageOfData<T>($"https://api.vimeo.com{page.Paging.Next}");

                if (page == null)
                {
                    return null;
                }

                list.AddRange(page.Data);
            }

            return list;
        }

        private async Task<PaginationResponse<T>?> GetPageOfData<T>(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Unable to get list: {response.StatusCode} {response.ReasonPhrase} {json}.");
                return null;
            }
            PaginationResponse<T>? responseContent = JsonSerializer.Deserialize<PaginationResponse<T>>(json, jsonOptions);

            if (responseContent == null)
            {
                Console.WriteLine("Unable to parse response.");
                return null;
            }

            return responseContent;
        }

        private static string BytesToString(long byteCount)
        {
            // https://stackoverflow.com/a/281679
            string[] suf = ["B", "KB", "MB", "GB", "TB", "PB", "EB"]; //Longs run out around EB
            if (byteCount == 0)
            {
                return "0" + suf[0];
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0,5:###.0}", Math.Sign(byteCount) * num) + suf[place];
        }

        private static string MakeValidFileName(string name)
        {
            // https://stackoverflow.com/a/847251
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private static int GetMagnitudeOfNumber(int number)
        {
            // https://stackoverflow.com/a/6865024
            return (int)(Math.Log10(Math.Max(Math.Abs(number), 0.5)) + 1);
        }
    }
}
