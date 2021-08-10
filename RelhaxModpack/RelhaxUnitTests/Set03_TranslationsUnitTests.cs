using System;
using RelhaxModpack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Linq;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set03_TranslationsUnitTests
    {
        [TestMethod]
        public void Test01_LoadTranslationsTest()
        {
            Assert.IsFalse(Translations.TranslationsLoaded);
            Translations.LoadTranslations();
            Assert.IsTrue(Translations.TranslationsLoaded);
        }

        [TestMethod]
        public void Test02_NoEnglishTODOTest()
        {
            LoadTranslationsIfNotLoaded();

            //ensure that every English entry is not null, empty, whitespace, or TODO
            //English is the default language in case of errors
            foreach (string key in Translations.GetLanguageDictionaries(Languages.English).Keys)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(Translations.GetLanguageDictionaries(Languages.English)[key]));
                Assert.IsFalse(Translations.GetLanguageDictionaries(Languages.English)[key].Equals(Translations.TranslationNeeded));
            }
        }

        [TestMethod]
        public void Test03_TranslateRandomKeyTest()
        {
            LoadTranslationsIfNotLoaded();

            //English should have the complete list
            int total = Translations.GetLanguageDictionaries(Languages.English).Count;
            Random rand = new Random();
            int randomEntry = rand.Next(0, total-1);

            //get the key based on int index
            string key = Translations.GetLanguageDictionaries(Languages.English).Keys.ElementAt(randomEntry);

            //do it 100 times or so to ensure randomness
            for (int i = 0; i < 99; i++)
            {
                //loop to ensure we get keys and the entry isn't TODO
                while (!ValidEntryExistsInAll(key))
                {
                    randomEntry = rand.Next(0, total - 1);
                    key = Translations.GetLanguageDictionaries(Languages.English).Keys.ElementAt(randomEntry);
                }

                //make sure the random key returned a valid value
                foreach (Languages lang in Translations.SupportedLanguages)
                {
                    Translations.SetLanguage(lang);
                    string result = Translations.GetTranslatedString(key);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(result));
                }
            }
        }

        [TestMethod]
        public void Test04_TODOKeyReturnsEnglishTest()
        {
            LoadTranslationsIfNotLoaded();

            //in the event that a language has a TODO key (not translated yet), make sure that the returned item is English
            string key = string.Empty;
            Languages lang = Languages.English;

            //check for each key in the English dictionary
            foreach (string key_ in Translations.GetLanguageDictionaries(Languages.English).Keys)
            {
                //for each key, check the other language dictionaries
                foreach (Languages lang_ in Translations.SupportedLanguages)
                {
                    if (lang_ == Languages.English)
                        continue;

                    if (Translations.Exists(key_, lang_) && Translations.GetLanguageDictionaries(lang_)[key_].Equals(Translations.TranslationNeeded))
                    {
                        key = key_;
                        lang = lang_;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(key))
                    break;
            }

            //if key is still blank, then we have no TODO translations. Which is a good thing i guess
            if (string.IsNullOrEmpty(key))
                return;

            //set the language to the one with the TODO, and ensure that the English phrase is returned
            Translations.SetLanguage(lang);
            Assert.AreEqual(Translations.GetLanguageDictionaries(Languages.English)[key], Translations.GetTranslatedString(key));
        }

        /// <summary>
        /// Checks if a given key exists in all language dictionaries and is not null, whitespace, or TODO
        /// </summary>
        /// <param name="key">The translation ID (sometimes called component)</param>
        /// <returns>True if the translation entry exists and is an actual translation, false otherwise</returns>
        private bool ValidEntryExistsInAll(string key)
        {
            foreach(Languages lang in Translations.SupportedLanguages)
            {
                if (!Translations.ExistsAndValid(key,lang))
                    return false;
            }
            return true;
        }

        private void LoadTranslationsIfNotLoaded()
        {
            if (!Translations.TranslationsLoaded)
                Translations.LoadTranslations();
        }
    }
}
