using System;
using RelhaxModpack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set03_TranslationsTest : UnitTestLogBase
    {
        [TestMethod]
        public void Test01_LoadTranslations()
        {
            Assert.IsFalse(Translations.TranslationsLoaded);
            Translations.LoadTranslations();
            Assert.IsTrue(Translations.TranslationsLoaded);
        }
    }
}
