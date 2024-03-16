# VimeoDownloader
I built this command line tool to download your own videos from Vimeo and upload them to a Google Drive

## Requirements
[.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Setup
1. Pull down the code
1. Setup Vimeo Credentials
	1. On vimeo create an app here: https://developer.vimeo.com/
	1. Navigate to your app and generate a new access token and give it the needed permissions
		- These permissions were enough for me `private` and `video_files`.
1. Setup Google Drive Credentials
	1. Navigate to the Google Console: https://console.cloud.google.com/
	1. Create a new project
	1. Setup the OAuth consent screen: https://console.cloud.google.com/apis/credentials/consent
	1. Create a new OAuth Client ID: https://console.cloud.google.com/apis/credentials/consent
	1. Download the client secret json from the above created credentials
	1. Enable Google Drive API: https://console.cloud.google.com/apis/library/drive.googleapis.com
1. Set the configuration values in the Program.cs
1. Build and run the code
	- You can either use an IDE such as VSCode or Visual Studio
	- Or you can do this through the command line run `dotnet build` followed by `dotnet run`
1. When running for the first time, Google will prompt you to login and authorize your code to access the Google Drive
