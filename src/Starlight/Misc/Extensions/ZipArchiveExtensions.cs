using System;
using System.IO;
using System.IO.Compression;

namespace Starlight.Misc.Extensions;

internal static class ZipArchiveExtensions {
    public static void ExtractToDirectoryEx(this ZipArchive archive, string destinationDirectoryName,
        bool overwrite = false) {
        destinationDirectoryName = Directory.CreateDirectory(destinationDirectoryName).FullName;

        foreach (var file in archive.Entries) {
            if (file.FullName.StartsWith("\\"))
                continue;

            var completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryName, file.FullName));

            if (!completeFileName.StartsWith(destinationDirectoryName, StringComparison.OrdinalIgnoreCase))
                throw new IOException(
                    "Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");

            if (File.Exists(completeFileName) && !overwrite)
                continue;

            // ReSharper disable AssignNullToNotNullAttribute
            var dirName = Path.GetDirectoryName(completeFileName);

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            if (string.IsNullOrEmpty(file.Name)) {
                Directory.CreateDirectory(dirName);
                continue;
            }
            // ReSharper enable AssignNullToNotNullAttribute

            try {
                file.ExtractToFile(completeFileName, true);
            }
            catch
                (IOException) // Roblox likes to make my life harder than it has to be. No clue why this throws but it's going into production because it works.
            { }
        }
    }
}