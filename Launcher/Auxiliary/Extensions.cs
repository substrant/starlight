/* 
 *  Extensions.cs
 *  Author: RealNickk
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Starlight
{
    public static class Extensions
    {
        public static bool IsEmpty(this string szStr)
        {
            return szStr == null || szStr.Length == 0;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite) // i pasted this but had to add code to fix zipping problems anyways so dont call me a skid
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                if (file.FullName.StartsWith("\\")) continue; // roblox rapes the zip folders :/
                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");

                string szDirectoryName = Path.GetDirectoryName(completeFileName);
                //Console.WriteLine("Extracting {0}...", file.Name);
                //Console.WriteLine("Directory Path: {0}", szDirectoryName);
                //Console.WriteLine("Directory Exists? {0}", Directory.Exists(szDirectoryName).ToString());
                
                if (!Directory.Exists(szDirectoryName))
                    Directory.CreateDirectory(szDirectoryName);

                if (file.Name == "")
                {
                    Directory.CreateDirectory(szDirectoryName);
                    continue;
                }

                file.ExtractToFile(completeFileName, true);
            }
        }

        public static void ExtractSelectedToDirectory(this ZipArchive file, string szOutDir, string selector)
        {
            var entries = file.Entries.Where(entry => selector == entry.FullName);
            if (entries.Count() < 1) throw new IOException($"\"{selector}\" does not exist in the zip file.");
            entries.ForEach(entry => entry.ExtractToFile(Path.Combine(szOutDir, entry.FullName)));
        }

        public static void ExtractSelectedToDirectory(this ZipArchive file, string szOutDir, string[] selector)
        {
            var entries = file.Entries.Where(entry => selector.Contains(entry.FullName));
            if (entries.Count() < selector.Length)
            {
                var nonExistentEntries = selector // if yk any better way to do this lmk
                    .Where(name => entries.Where(entry => name != entry.FullName).Count() > 0)
                    .Select(name => $"\"{name}\"");
                throw new IOException($"The paths [{string.Join(", ", nonExistentEntries)}] do not exist in the zip file.");
            }
            entries.ForEach(entry => entry.ExtractToFile(Path.Combine(szOutDir, entry.FullName)));
        }

        public static string[] Split(this string szStr, string szDelimeter)
        {
            return szStr.Split(new string[] { szDelimeter }, StringSplitOptions.None);
        }

        public static string[] Split(this string szStr, string szDelimeter, StringSplitOptions opt)
        {
            return szStr.Split(new string[] { szDelimeter }, opt);
        }

        public static async Task<string> DownloadStringTaskAsync(this HttpClient client, string szUrl)
        {
            using Stream stm = await client.GetStreamAsync(szUrl);
            using StreamReader rstm = new(stm);
            return await rstm.ReadToEndAsync();
        }

        public static async Task DownloadFileTaskAsync(this HttpClient client, string szUrl, string szFilePath)
        {
            using Stream stm = await client.GetStreamAsync(szUrl);
            using FileStream fstm = File.Create(szFilePath);
            await stm.CopyToAsync(fstm);
        }
    }
}
