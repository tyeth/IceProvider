using System;
using System.Text.RegularExpressions;
using IceProvider;
using Xunit;

namespace CorexUnitTestProject
{
    public class UnitTestIceUtils
    {
        [Theory,
         InlineData(".*", "abc", "groupname"), InlineData("(?<groupname>.*)", "abc", "test"),
         InlineData("test", "abc", "test"), InlineData("", "abc", "")]
        public void Test_SafelyGetRegExMatchGroupValue_ReturnsEmptyStringForNoMatch(string regex, string data,
            string groupname)
        {
            var i = new Regex(regex);
            var m = i.Match(data);
                Assert.Equal(
                    "",
                    m.SafelyGetGroupValue( groupname)
                );
        }


        [Theory, 
         InlineData("(?<groupname>.*)", "abc", "groupname", "abc"),
         InlineData("a(?<groupname>.*)", "abc", "groupname", "bc"),
        InlineData("(a(?<groupname>.*))","abc","groupname","bc"),
        InlineData("(a(?<b>b)c)","abc","b","b")]
        
        public void Test_SafelyGetRegExMatchGroupValue_ReturnsCorrectStringForSingleLevelMatch(string regex,
            string data, string groupname, string expected)
        {
            
            var i = new Regex(regex);
            var m = i.Match(data);
            var s = m.SafelyGetGroupValue(groupname);
            Assert.Equal(expected,s);
        }

        [Theory,
        InlineData("http://wasteman.org/waste/", true),
        InlineData("http://wasteman.org/waste/", false),
        InlineData("http://wasteman.org/waste", false),
        InlineData("http://wasteman.org/waste", true)]
        public void Test_WithOrWithoutSlash(string url, bool trailingSlash)
        {
            if (trailingSlash)
            {
                var l = IceUtils.WithOrWithoutTrailingChar(url, '/', trailingSlash);
                Assert.EndsWith("/", l, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                var l = IceUtils.WithOrWithoutTrailingChar(url, '/', trailingSlash);
                Assert.NotEqual(l[l.Length - 1], '/');
            }
        }

        [Fact]
        public void Test_IsValidUrl_Fails_NonUrl()
        {
            const string a = "a";
            Assert.Equal(IceUtils.IsValidIceUrl(a), false);
        }


        [Fact]
        public void Test_IsValidUrl_Fails_TrueUrlWithoutContainsIce()
        {
            const string a = "http://a/";
            Assert.Equal(IceUtils.IsValidIceUrl(a), false);
        }

        [Fact]
        public void Test_IsValidUrl_Passes_TrueIceUrl()
        {
            const string a = "http://aice/";
            Assert.Equal(IceUtils.IsValidIceUrl(a), true);
        }


        [Fact]
        public void Test_ListViewItem_Is_IIceEpisode()
        {
            var a = new ListViewItem
            {
                Name = "Test",
                Url = "//a/",
                Checked = true
            };

            Assert.Equal(a.Checked == a.Selected, true);
            Assert.Equal(a.Selected, true);
            Assert.Equal(a.Name == "Test", true);
            Assert.Equal(a is IIceEpisode, true);
        }

        [Fact]
        public void Test_StripUrlToDomainAndPort_WithRealUrl()
        {
            const string a = "https://ice:8080/face/you";
            Assert.Equal(IceUtils.StripToDomainAndPort(a), "ice:8080");
        }
    }
}