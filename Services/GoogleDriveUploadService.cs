﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System.Diagnostics;
using VimeoDownloader.Models;

namespace VimeoDownloader.Services
{
    internal class GoogleDriveUploadService(string clientSecretFileName, string folderId, HttpClient client)
    {
        private readonly string clientSecretFileName = clientSecretFileName;
        private readonly string folderId = folderId;
        private DriveService? driveService;
        private readonly HttpClient client = client;

        public async Task StartUploadProcess(VideoList videos)
        {
            driveService = await SetupDriveService();

            await UploadVideos(videos);
        }

        private async Task<DriveService> SetupDriveService()
        {
            Console.WriteLine("Fetching Google Credentials...");

            UserCredential credential;

            using (FileStream stream = new(clientSecretFileName, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    [DriveService.ScopeConstants.Drive],
                    "user",
                    CancellationToken.None);
            }

            DriveService driveService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "File Uploader C# Console App",
            });

            return driveService;
        }

        private async Task UploadVideos(VideoList videos)
        {
            Console.WriteLine($"Uploading {videos.VideoItems.Count} videos to the Google Drive...");

            Stopwatch stopwatch = Stopwatch.StartNew();
            long totalSize = videos.Size;
            long sizeSoFar = 0;

            for (int i = 0; i < videos.VideoItems.Count; i++)
            {
                VideoItem video = videos.VideoItems[i];

                string message = $"Downloading video '{video.FileName}' {i + 1} out of {videos.VideoItems.Count}";

                if (i > 0)
                {
                    double ratio = (double)totalSize / sizeSoFar;
                    double secondsSoFar = stopwatch.Elapsed.TotalSeconds;
                    double totalSeconds = ratio * secondsSoFar;

                    // https://stackoverflow.com/a/9994060
                    TimeSpan t = TimeSpan.FromSeconds(totalSeconds - secondsSoFar);
                    string formattedTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds);

                    message += $". Estimated time remaining {formattedTime}";
                }

                Console.WriteLine($"{message}...");

                // https://stackoverflow.com/a/70418747
                Stream responseStream = await client.GetStreamAsync(video.DownloadLink);
                using (FileStream fileStream = new(video.FileName!, FileMode.Create))
                {

                    Console.WriteLine("Uploading video...");

                    Google.Apis.Drive.v3.Data.File fileMetadata = new()
                    {
                        Name = video.FileName!,
                        Parents = [folderId]
                    };
                    FilesResource.CreateMediaUpload request;
                    request = driveService!.Files.Create(fileMetadata, responseStream, "application/octet-stream");
                    request.Fields = "id";
                    request.SupportsAllDrives = true;
                    IUploadProgress uploadProgress = await request.UploadAsync();

                    if (uploadProgress.Status == UploadStatus.Failed)
                    {
                        Console.WriteLine($"Error uploading video {uploadProgress.Exception}");
                        return;
                    }

                    sizeSoFar += video.Size;
                }

                // Clean up the file now that it's uploaded
                File.Delete(video.FileName!);
            }

            stopwatch.Stop();
            // https://stackoverflow.com/a/9994060
            TimeSpan totalT = TimeSpan.FromSeconds(stopwatch.Elapsed.TotalSeconds);
            string formattedTotalTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                    totalT.Hours,
                                    totalT.Minutes,
                                    totalT.Seconds);
            Console.WriteLine($"Elapsed time: {formattedTotalTime}");

            Console.WriteLine("Finished Uploading all videos.");
        }
    }
}
