using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelhaxModpack;
using RelhaxModpack.Patching;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;

namespace RelhaxUnitTests
{
    [TestClass]
    public class Set06_PatchingUnitTests
    {
        [TestMethod]
        public void Test01_RegexRegressionTesting()
        {
            Logging.Info("Regex regressions start");
            PatchRegression regression = new PatchRegression(PatchRegressionTypes.regex, BuildRegexUnittests());
            Assert.IsTrue(regression.RunRegressions());
            Logging.Info("Regex regressions end");
        }

        [TestMethod]
        public void Test02_XmlRegressionTesting()
        {
            Logging.Info("Xml regressions start");
            PatchRegression regression = new PatchRegression(PatchRegressionTypes.xml, BuildXmlUnittests());
            Assert.IsTrue(regression.RunRegressions());
            Logging.Info("Xml regressions end");
        }

        [TestMethod]
        public void Test03_JsonRegressionTesting()
        {
            Logging.Info("Json regressions start");
            PatchRegression regression = new PatchRegression(PatchRegressionTypes.json, BuildJsonUnittests());
            Assert.IsTrue(regression.RunRegressions());
            Logging.Info("Json regressions end");
        }

        [TestMethod]
        public void Test04_FollowPathRegressionTesting()
        {
            Logging.Info("FollowPath regressions start");
            PatchRegression regression = new PatchRegression(PatchRegressionTypes.followPath, BuildFollowPathUnittests());
            Assert.IsTrue(regression.RunRegressions());
            Logging.Info("FollowPath regressions end");
        }

        #region Json regressions
        private List<PatchUnitTest> BuildJsonUnittests()
        {
            return new List<PatchUnitTest>()
            {
                new PatchUnitTest()
                {
                    Description = "add test 1: basic add",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "awesome/false",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 2: repeat of basic add. should do nothing",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "awesome/false",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 3: same path as basic add, but different value to insert. should update the value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "awesome/true",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 4: add of a new object as well as the path. should create object paths to value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memes/awesome/true",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 5: add of a new property to part object path that already exists. should add the value without overwriting the path",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memes/dank/true",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 6: add of a new blank array",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memelist[array]",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 7: add of a new blank object",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "objectname[object]",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 8: add of new property with slash escape",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$",
                        Search = "",
                        Replace = "memeville/spaces[sl]hangar_premium_v2",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 1: edit attempt of path that does not exist. should note it log and abort",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.fakePath",
                        Search = "",
                        Replace = "null",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 2: object edit attempt of array. should note in log and abort",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.nations",
                        Search = "",
                        Replace = "null",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 3: edit attempt of simple path. should change the one value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.mode",
                        Search = "normal",
                        Replace = "epic",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 4: edit attempt of simple path. should change the one value (false, should report value entry same and exit)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.mode",
                        Search = "epic",
                        Replace = "epic",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "array edit test 1: edit of array of values. should change the last value in the array",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist[*]",
                        Search = "ttest",
                        Replace = "test",
                        Mode = "arrayEdit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "array edit test 2: edit of array of objects. should parse every value of 421 or above to be 420 (regex style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers.starttime[*]",
                        Search = @"^[4-9][2-9][0-9]\d*$",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "array edit test 3: edit of array of objects. should parse every value of 419 or below to be 420 (regex style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers.starttime[*]",
                        Search = @"^([0123]?[0-9]?[0-9]|4[01][0-9]|41[0-9])$",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "array edit test 4: edit array of objects. should parse every value less than 420 to be 420 (jsonpath style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers2.starttime[?(@<420)]",
                        Search = ".*",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "array edit test 5: edit array of objects. should parse every value more than 420 to be 420 (jsonpath style)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = @"$.screensavers2.starttime[?(@>420)]",
                        Search = ".*",
                        Replace = "420",
                        Mode = "arrayEdit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "remove test 1: basic remove test with property",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.game_greeting2",
                        Search = ".*",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "remove test 2: advanced remove test with property matching (should not remove as text not matched)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.game_greeting",
                        Search = "not match this text",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "remove test 3: advanced remove test with property matching (should remove, text matched)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.game_greeting",
                        Search = "match this text",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayAdd test 1: basic add of jValue at index 0",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = ".*",
                        Replace = "spaces[sl]urmom[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayAdd test 2: basic add of jValue at index -1 (last)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = ".*",
                        Replace = "spaces[sl]urmom2[index=-1]",
                        Mode = "arrayAdd"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayAdd test 3: attempt add of object to array of JValue, should fail",
                    ShouldPass = false,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = ".*",
                        Replace = "enable/true[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayAdd test 4: attempt add of jValue to array of object, should fail",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.sample_object_array",
                        Search = ".*",
                        Replace = "spaces[sl]urmom[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayAdd test 5: basic add of object",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.sample_object_array",
                        Search = ".*",
                        Replace = "enable/true[index=0]",
                        Mode = "arrayAdd"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayRemove test 1: basic remove of jValue \"test\"",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "test",
                        Replace = "",
                        Mode = "arrayRemove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayRemove test 2: basic remove of jValue \"test3\" (does not exist, should fail)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "test3",
                        Replace = "",
                        Mode = "arrayRemove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayRemove test 3: basic remove of jObject \"enable:true\"",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.sample_object_array",
                        Search = ".*",
                        Replace = "",
                        Mode = "arrayRemove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayClear test 1: basic clear of jValue \"username\"",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "username",
                        Replace = "",
                        Mode = "arrayClear"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayClear test 2: basic clear of jValue \"username\" (does not exist)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.ignorelist",
                        Search = "username",
                        Replace = "",
                        Mode = "arrayClear"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "arrayClear test 3: basic clear of object \".*\" (all)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "$.screensavers.starttime",
                        Search = ".*",
                        Replace = "",
                        Mode = "arrayClear"
                    }
                }
            };
        }
        #endregion

        #region Xml regressions
        private List<PatchUnitTest> BuildXmlUnittests()
        {
            return new List<PatchUnitTest>()
            {
                new PatchUnitTest()
                {
                    Description = "add test 1: adding element with value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        //type set in regression test
                        Path = "//audio_mods.xml/loadBanks",
                        Search = "",
                        Replace = "bank/sound_bank_name",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 2: adding element with levels (with value)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/events",
                        Search = "",
                        Replace = "event/name/vo_ally_killed_by_player",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 3: adding in element where child inner text equals",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "/audio_mods.xml/events/event[name = \"vo_ally_killed_by_player\"]",
                        Search = "",
                        Replace = "mod/simple_sounds_teamkill",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "add test 4: adding element with escape for slash",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/RTPCs",
                        Search = "",
                        Replace = "RTPC/RTPC[sl]volume_slider_name",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 1: edit of a value matching parameter to new value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/random_property",
                        Search = "value",
                        Replace = "better_value",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 2: edit of a value matching parameter to a new value (but match does not exist)",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/random_property",
                        Search = "fake_value",
                        Replace = "more_fake_value",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "edit test 3: edit of matching any value to a new value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/random_property2",
                        Search = "",
                        Replace = "new_value",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "remove test 1: remove matching element name",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/prop_to_remove",
                        Search = "",
                        Replace = "",
                        Mode = "remove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "remove test 2: remove matching element name and value",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Path = "//audio_mods.xml/prop_to_remove2",
                        Search = "remove_me",
                        Replace = "",
                        Mode = "remove"
                    }
                }
            };
        }
        #endregion

        #region Regex regressions
        private List<PatchUnitTest> BuildRegexUnittests()
        {
            return new List<PatchUnitTest>()
            {
                new PatchUnitTest()
                {
                    Description = "multiple matches, only replaces on specified lines",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Line = "3,5",
                        Search = "should match",
                        Replace = "replaced",
                        Type = "regex"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "multiple matches, replaces all lines",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        Line = "",
                        Search = "should match",
                        Replace = "replaced",
                        Type = "regex"
                    }
                }
            };
        }
        #endregion

        #region FollowPath regressions
        private List<PatchUnitTest> BuildFollowPathUnittests()
        {
            return new List<PatchUnitTest>()
            {
                new PatchUnitTest()
                {
                    Description = "disable damageLog, follow path @xvm.xc->check_01.xc, includes \"$ref\" inside file",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.damageLog.enabled",
                        Search = ".*",
                        Replace = "false",
                        Type = "json",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "update hitlogHeader's updateEvent property to be on dank memes. followPath @xvm.xc->battleLabels.xc->battleLabelsTemplates.xc",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        //@"$.screensavers2.starttime[?(@<420)]"
                        //https://support.smartbear.com/alertsite/docs/monitors/api/endpoint/jsonpath.html#filters
                        Path = @"$.battleLabels.formats[?(@ =~ /hitLogHeader/)].updateEvent",
                        Search = ".*",
                        Replace = "PY(ON_DANK_MEMES)",
                        Type = "json",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "remove array reference entry of fire",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.battleLabels.formats",
                        Search = "fire",
                        Replace = "",
                        Type = "json",
                        Mode = "arrayRemove"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "part 1 of 4: add a object to playersPanel definition-> change link in root file from 'playersPanel' to 'def'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel",
                        Search = ".*",
                        Replace = @"[xvm_dollar][lbracket][quote]playersPanel.xc[quote][colon][quote]def[quote][xvm_rbracket]",
                        Type = "json",
                        Mode = "edit"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "part 2 of 4: add a object to playersPanel definition-> change link in root file from 'playersPanel' to 'def'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel",
                        Search = ".*",
                        Replace = @"newDef[object]",
                        Type = "json",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "part 3 of 4: add a object to playersPanel definition-> change link in root file from 'playersPanel' to 'def'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel.newDef",
                        Search = ".*",
                        Replace = @"isThisTheBestXvmParserEver/true",
                        Type = "json",
                        Mode = "add"
                    }
                },
                new PatchUnitTest()
                {
                    Description = "part 4 of 4: add a object to playersPanel definition-> change link in root file back to 'def' to 'playersPanel'",
                    ShouldPass = true,
                    Patch = new Patch()
                    {
                        FollowPath = true,
                        Path = @"$.playersPanel",
                        Search = ".*",
                        Replace = @"[xvm_dollar][lbracket][quote]playersPanel.xc[quote][colon][quote]playersPanel[quote][xvm_rbracket]",
                        Type = "json",
                        Mode = "edit"
                    }
                }
            };
        }
        #endregion

    }
}
