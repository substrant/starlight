using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Starlight.Misc.Extensions;

internal static class HttpClientExtensions
{
    public static string DownloadString(this HttpClient client, string url)
    {
        using var stm = AsyncHelpers.RunSync(() => client.GetStreamAsync(url));
        using StreamReader rstm = new(stm);
        return rstm.ReadToEnd();
    }

    public static async Task<string> DownloadStringAsync(this HttpClient client, string url)
    {
        using var stm = await client.GetStreamAsync(url);
        using StreamReader rstm = new(stm);
        return await rstm.ReadToEndAsync();
    }

    public static void DownloadFile(this HttpClient client, string url, string filePath)
    {
        using var stm = AsyncHelpers.RunSync(() => client.GetStreamAsync(url));
        using var fstm = File.Create(filePath);
        stm.CopyTo(fstm);
    }

    public static async Task DownloadFileAsync(this HttpClient client, string url, string filePath)
    {
        using var stm = await client.GetStreamAsync(url);
        using var fstm = File.Create(filePath);
        await stm.CopyToAsync(fstm);
    }
}