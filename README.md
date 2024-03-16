# VimeoDownloader
I built this command line tool to download videos from Vimeo from some of your local folders in Vimeo.

## Requirements
[.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Setup
1. Pull down the code
1. On vimeo create an app here: https://developer.vimeo.com/
1. Navigate to your app and generate a new access token and give it the needed permissions
   - These permissions were enough for me `public`, `private`, `stats`, and `video_files`. You likely need a subset of these, I just haven't bothered fully testing this.
1. Set the configuration values in the Program.cs
1. Build and run the code
	- You can either use an IDE such as VSCode or Visual Studio
	- Or you can do this through the command line run `dotnet build` followed by `dotnet run`
