using System;

namespace Starlight.Except;

public class PluginException : Exception
{
    public PluginException(string message) : base(message)
    {
    }

    public PluginException(string message, Exception inner) : base(message, inner)
    {
    }
}