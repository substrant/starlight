using System;

namespace Starlight.Core
{
    public class BootstrapException : Exception
    {
        public BootstrapException(string message) : base(message) { }
        public BootstrapException(string message, Exception inner) : base(message, inner) { }
    }
}
