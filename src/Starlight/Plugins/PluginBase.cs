using System;
using Starlight.Bootstrap;
using Starlight.Launch;
using Starlight.PostLaunch;

namespace Starlight.Plugins;

public abstract class PluginBase
{
    public readonly PluginApi Api;

    bool _enabled = true;

    protected PluginBase()
    {
        Api = new PluginApi(this);
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
                Load();
            else
                Unload();
        }
    }

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