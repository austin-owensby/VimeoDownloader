using VimeoDownloader.Services;

// Configure these values as needed
string apiKey = "<Replace Me>";
string userId = "<Replace Me>";
string downloadPath = $"VimeoDownload-{DateTime.Now:yyyy-MM-dd_hh-mm-ss}";

try
{
    VimeoDownloaderService vimeoDownloaderService = new(userId, apiKey, downloadPath);

    bool success = await vimeoDownloaderService.StartDownloadProcess();
}
catch (Exception e)
{
    Console.WriteLine($"An unknown error occured: {e}");
}
