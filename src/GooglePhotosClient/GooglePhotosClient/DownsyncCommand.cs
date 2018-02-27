﻿namespace GooglePhotosClient
{
    using GooglePhotosClient.Assets;
    using GooglePhotosClient.Extensions;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using C = Colorful;

    public class DownsyncCommand
    {
        private readonly CommandOption clientSecretSavePath;

        private readonly CommandOption clientSecretPath;

        private readonly CommandOption backupPath;

        public DownsyncCommand(CommandOption clientSecretSavePath, CommandOption clientSecretPath, CommandOption backupPath)
        {
            this.clientSecretSavePath = clientSecretSavePath;
            this.clientSecretPath = clientSecretPath;
            this.backupPath = backupPath;
        }

        public bool Validate()
        {
            C.Console.WriteLine(General.Downsync);
            var savePath = this.GetSavePath();
            var error = false;
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                C.Console.WriteLine(General.ClientSecretSavePathError, Color.Red);
                error = true;
            }

            if (this.clientSecretPath.Values.Count == 0 || !File.Exists(this.clientSecretPath.Values[0]))
            {
                C.Console.WriteLine(General.ClientSecretPathError, Color.Red);
                error = true;
            }

            if (this.backupPath.Values.Count == 0 || !Directory.Exists(this.backupPath.Values[0]))
            {
                C.Console.WriteLine(General.BackupPathError, Color.Red);
                error = true;
            }

            return !error;
        }

        public int Execute()
        {
            var savePath = this.GetSavePath();
            var path = this.backupPath.Values[0];
            var download = new List<Google.Apis.Drive.v3.Data.File>();
            var downloadClient = new DownloadClient(
                this.clientSecretPath.Value(),
                savePath);
            var photos = downloadClient.IteratePhotos().ToList();
            foreach (var photo in photos)
            {
                var photoPath = Path.Combine(path, photo.GetLocalIdentifierFileName());
                if (!File.Exists(photoPath))
                {
                    download.Add(photo);
                }
            }

            downloadClient.DownloadPhotos(photos, path, state =>
            {
                switch (state.Status)
                {
                    case Google.Apis.Download.DownloadStatus.Downloading:
                        C.Console.WriteLine($"\rDownloaded {state.BytesDownloaded} bytes.", Color.Green);
                        break;
                    case Google.Apis.Download.DownloadStatus.Completed:
                        C.Console.WriteLine($"Finished download with {state.BytesDownloaded} bytes.", Color.Green);
                        break;
                    case Google.Apis.Download.DownloadStatus.Failed:
                        C.Console.WriteLine($"Download failed: \n{state.Exception.Message}", Color.Red);
                        break;
                }
            });
            return 0;
        }

        private string GetSavePath()
        {
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".credentials",
                "GooglePhotosClientSecret.json");
            return this.clientSecretSavePath.HasValue()
                ? this.clientSecretSavePath.Value() ?? defaultPath
                : defaultPath;
        }
    }
}
