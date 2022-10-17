using System;
using NUnit.Framework;

namespace Starlight.Test
{
    internal class Env
    {
        public static bool IsCi => Environment.GetEnvironmentVariable("IS_CI") is not null;

        public static string AuthToken => Environment.GetEnvironmentVariable("AUTH_TOKEN");

        public static void AssertCi()
        {
            if (IsCi)
                Assert.Inconclusive("Cannot run this test on CI.");
        }
    }
}
