using System;

namespace Starlight.Core
{
    public class LaunchException : Exception
    {
        public LaunchException(string message) : base(message) { }
    }
}
