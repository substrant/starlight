using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.PostLaunch;

namespace Starlight.Plugins;

public abstract class PluginBase
{
    public readonly PluginSdk Sdk;

    bool _enabled = true;

    protected PluginBase()
    {
        Sdk = new PluginSdk(Assembly.GetCallingAssembly(), this);
    }

    public abstract string Name { get; }

    public abstract string Author { get; }

    public abstract string Description { get; }

    public bool Enabled
    {
        get => _enabled;
        internal set
        {
            _enabled = value;
            if (_enabled)
            {
                PluginArbiter.Plugins.Add(this);
                Load();
            }
            else
            {
                Unload();
                PluginArbiter.Plugins.Remove(this);
            }
        }
    }

    public void OverloadConfig(IReadOnlyDictionary<string, dynamic> obj)
    {
        Sdk.MergeConfig(JObject.FromObject(obj));
    }

    /* Don't call load/unload, do enabled = true, enabled = false */

    public virtual void Load()
    {
    }

    public virtual void Unload()
    {
    }

    public virtual void PreLaunch(LaunchParams info, ref Client client)
    {
    }

    public virtual void PostLaunch(ClientInstance inst)
    {
    }

    public virtual void PostWindow(IntPtr hwnd)
    {
    }
}