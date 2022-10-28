using System;

namespace Starlight.Except;

public class SchemeParseException : Exception
{
    public SchemeParseException(string message) : base(message)
    {
    }
}