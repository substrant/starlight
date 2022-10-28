using System;

namespace Starlight.Except;

/// <summary>
///     Thrown when a file's integrity is compromised. This is usually caused by file corruption.
/// </summary>
public sealed class BadIntegrityException : Exception
{
    public BadIntegrityException(string message) : base(message)
    {
    }
}