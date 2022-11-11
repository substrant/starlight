using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starlight.Misc.Extensions;

internal static class StringExtensions
{
    public static string[] Split(this string str, string delim)
    {
        return str.Split(new[] { delim }, StringSplitOptions.None);
    }

    public static string[] Split(this string str, params string[] delim)
    {
        return str.Split(delim, StringSplitOptions.None);
    }
}