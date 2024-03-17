using VimeoDownloader.Models;
using VimeoDownloader.Services;

// Configure these values as needed
string vimeoApiKey = "<Replace Me>";
string googleClientSecretFile = "<Replace Me>";
string vimeoUserId = "<Replace Me>";
string googleDriveFolderId = "<Replace Me>";
int offset = 0; // If something goes wrong mid process, start again at a certain index

try
{
    VimeoDownloaderService vimeoDownloaderService = new(vimeoApiKey, vimeoUserId);

    VideoList? vidoes = await vimeoDownloaderService.GetVideosToDownload(offset);

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
