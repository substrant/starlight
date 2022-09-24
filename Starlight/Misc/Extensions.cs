using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Starlight
{
    internal static class Extensions
    {
        /* IEnumerable */

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        /* string */

        internal static string[] Split(this string str, string delim) =>
            str.Split(new string[] { delim }, StringSplitOptions.None);

        internal static string[] Split(this string str, string delim, StringSplitOptions opt) =>
            str.Split(new string[] { delim }, opt);

        /* HttpClient */

        internal static async Task<string> DownloadStringTaskAsync(this HttpClient client, string url)
        {
            using Stream stm = await client.GetStreamAsync(url);
            using StreamReader rstm = new(stm);
            return await rstm.ReadToEndAsync();
        }

        internal static async Task DownloadFileTaskAsync(this HttpClient client, string url, string filePath)
        {
            using Stream stm = await client.GetStreamAsync(url);
            using FileStream fstm = File.Create(filePath);
            await stm.CopyToAsync(fstm);
        }

        /* ZipArchive (because whoever wrote it doesn't know what the hell overwriting means) */

        // Roblox does something funny with the zip files so I have to redo their code...also I need overwriting.
        // Pasted from StackOverflow, still had to rewrite it because the code itself just didn't wanna work. :/
        internal static void ExtractToDirectory(this ZipArchive archive, string destDir, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destDir);
                return;
            }
            
            destDir = Directory.CreateDirectory(destDir).FullName;
            foreach (var file in archive.Entries)
            {
                if (file.FullName.StartsWith("\\")) continue; // Roblox raping the zip files.
                var completeFileName = Path.GetFullPath(Path.Combine(destDir, file.FullName));

                if (!completeFileName.StartsWith(destDir, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");

                var dirName = Path.GetDirectoryName(completeFileName);
                if (!Directory.Exists(dirName))
                    Directory.CreateDirectory(dirName);

                if (file.Name == "")
                {
                    Directory.CreateDirectory(dirName);
                    continue;
                }

                file.ExtractToFile(completeFileName, true);
            }
        }

        internal static void ExtractSelectedToDirectory(this ZipArchive file, string destDir, string selector)
        {
            var entries = file.Entries.Where(entry => selector == entry.FullName);
            if (entries.Count() < 1)
                throw new IOException($"\"{selector}\" does not exist in the zip file.");
            
            entries.ForEach(entry => entry.ExtractToFile(Path.Combine(destDir, entry.FullName)));
        }

        internal static void ExtractSelectedToDirectory(this ZipArchive file, string destDir, string[] selector)
        {
            var entries = file.Entries.Where(entry => selector.Contains(entry.FullName));
            if (entries.Count() < selector.Length)
            {
                var nonExistentEntries = selector
                    .Where(name => entries.Where(entry => name != entry.FullName).Count() > 0)
                    .Select(name => $"\"{name}\""); // I'd prefer a better way to do this.
                throw new IOException($"The paths [{string.Join(", ", nonExistentEntries)}] do not exist in the zip file.");
            }

            entries.ForEach(entry => entry.ExtractToFile(Path.Combine(destDir, entry.FullName)));
        }
    }
}
