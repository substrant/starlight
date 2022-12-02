using System;

namespace Starlight.Misc.Extensions;

internal static class StringExtensions {
    public static string[] Split(this string str, string delim) {
        return str.Split(new[] { delim }, StringSplitOptions.None);
    }

    public static string[] Split(this string str, params string[] delim) {
        return str.Split(delim, StringSplitOptions.None);
    }
}