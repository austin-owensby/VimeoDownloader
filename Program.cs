using VimeoDownloader.Models;
using VimeoDownloader.Services;

// Configure these values as needed
string vimeoApiKey = "<Replace Me>";
string googleClientSecretFile = "<Replace Me>";
string vimeoUserId = "<Replace Me>";
string googleDriveFolderId = "<Replace Me>";

try
{
    VimeoDownloaderService vimeoDownloaderService = new(vimeoApiKey, vimeoUserId);

    VideoList? vidoes = await vimeoDownloaderService.GetVideosToDownload();

    if (vidoes == null)
    {
        return;
    }

    GoogleDriveUploadService googleDriveUploadService = new(googleClientSecretFile, googleDriveFolderId, vimeoDownloaderService.client);
    await googleDriveUploadService.StartUploadProcess(vidoes);
}
catch (Exception e)
{
    Console.WriteLine($"An unknown error occured: {e}");
}
