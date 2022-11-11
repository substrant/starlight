using System;

namespace Starlight.Bootstrap;

/// <summary>
///     Thrown when a file's integrity is compromised. This is usually caused by file corruption.
/// </summary>
public sealed class BadIntegrityException : Exception
{
    public BadIntegrityException(Downloadable file) : base("Downloaded file is corrupt")
    {
        Data.Add("FileName", file.Name);
    }
}