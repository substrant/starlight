using System;
using NUnit.Framework;
using Starlight.Misc;

namespace Starlight.Test
{
    [TestFixture, Order(1)]
    internal class Initial
    {
        [OneTimeSetUp]
        public void Initialize()
        {
            Logger.Init(true);
            if (Env.IsCi)
                Console.WriteLine("Running on CI. ENSURE THAT INCONCLUSIVE TESTS PASS BEFORE YOU MERGE A PR!");
        }
    }
}
