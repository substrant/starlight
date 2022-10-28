using System;

namespace Starlight.Except;

public class PostLaunchException : Exception
{
    public PostLaunchException(string message) : base(message)
    {
    }
}