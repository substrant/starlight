using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Starlight.App;

public static class AutoUpdate
{
    const string Repo = "Substrant/Starlight";
    const string Branch = "v2";

    public static async Task<bool> IsUpdateAvailableAsync(CancellationToken token = default)
    {
#   if DEBUG
        return await Task.FromResult(false);
#   else

        using var ghClient = new RestClient("https://api.github.com/").UseNewtonsoftJson();
        var relData = await ghClient.GetJsonAsync<ReleaseData>($"/repos/{Repo}/releases/latest", token);
        if (relData?.TagName == null)
            return false;
        
        var asmVersion = Assembly.GetExecutingAssembly().GetName().Version;
        return Version.TryParse(relData.TagName, out var tagVersion) && tagVersion > asmVersion;
#   endif
    }

    public static bool IsUpdateAvailable()
    {
        return AsyncHelpers.RunSync(() => IsUpdateAvailableAsync());
    }

    public static void UpdateOnExit()
    {
        var info = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
            Arguments = $"& {{$Update = $true; Invoke-WebRequest https://github.com/{Repo}/raw/{Branch}/install.ps1 | Invoke-Expression}}"
        };
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>  Process.Start(info);
    }

    class ReleaseData
    {
        [JsonProperty("tag_name")] public string TagName;
    }
}