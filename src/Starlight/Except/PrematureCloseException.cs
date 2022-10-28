using System;

namespace Starlight.Except;

/// <summary>
///     Thrown when a native process used by Starlight prematurely closes.
/// </summary>
public sealed class PrematureCloseException : Exception
{
}