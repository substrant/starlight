using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;

namespace Starlight.Misc;

internal static class Extensions
{
    /* IEnumerable */

    internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source)
            action(element);
    }

    /* string */

    internal static string[] Split(this string str, string delim)
    {
        return str.Split(new[] { delim }, StringSplitOptions.None);
    }

    internal static string[] Split(this string str, params string[] delim)
    {
        return str.Split(delim, StringSplitOptions.None);
    }

    internal static string[] Split(this string str, StringSplitOptions opt, params string[] delim)
    {
        return str.Split(delim, StringSplitOptions.None);
    }

    internal static string[] Split(this string str, string delim, StringSplitOptions opt)
    {
        return str.Split(new[] { delim }, opt);
    }

    /* HttpClient */

    internal static string DownloadString(this HttpClient client, string url)
    {
        using var stm = client.GetStreamAsync(url).Result;
        using StreamReader rstm = new(stm);
        return rstm.ReadToEnd();
    }

    internal static void DownloadFile(this HttpClient client, string url, string filePath)
    {
        using var stm = client.GetStreamAsync(url).Result;
        using var fstm = File.Create(filePath);
        stm.CopyTo(fstm);
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
                throw new IOException(
                    "Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");

            // ReSharper disable AssignNullToNotNullAttribute
            var dirName = Path.GetDirectoryName(completeFileName);
            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            if (file.Name == "")
            {
                Directory.CreateDirectory(dirName);
                continue;
            }
            // ReSharper enable AssignNullToNotNullAttribute

            file.ExtractToFile(completeFileName, true);
        }
    }


    internal static void ExtractSelectedToDirectory(this ZipArchive file, string destDir, string selector)
    {
        var entries = file.Entries.Where(entry => selector == entry.FullName);
        var archiveEntries = entries as ZipArchiveEntry[] ?? entries.ToArray();
        if (!archiveEntries.Any())
            throw new IOException($"\"{selector}\" does not exist in the zip file.");

        archiveEntries.ForEach(entry => entry.ExtractToFile(Path.Combine(destDir, entry.FullName)));
    }
}