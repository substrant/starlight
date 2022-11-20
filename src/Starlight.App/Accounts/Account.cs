using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Starlight.Apis;

namespace Starlight.App.Accounts;

public class Account
{
    [JsonProperty("userId")]
    public long UserId { get; protected set; }

    [JsonProperty("annotation")]
    public string Annotation { get; set; }

    [JsonProperty("token")]
    public string Token { get; set; }
}