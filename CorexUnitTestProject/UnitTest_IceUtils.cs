using System;
using IceProvider;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace CorexUnitTestProject
{
    public class UnitTest_IceUtils
    {
        
        
        [Fact]
        public void Test_ListViewItem_Is_IIceEpisode()
        {
            var a = new IceProvider.ListViewItem
            {
Name="Test",
                Url="//a/"
               ,
                Checked = true
            };
            
            Assert.Equal(a.Checked==a.Selected,true);
            Assert.Equal(a.Selected,true);
            Assert.Equal(a.Name == "Test", true);
        }

        [Fact]
        public void Test_StripUrlToDomainAndPort_WithRealUrl()
        {
            const string a = "https://ice:8080/face/you";
            Assert.Equal(IceUtils.StripToDomainAndPort(a),"ice:8080");
        }
        
        [Fact]
        public void Test_IsValidUrl_Passes_TrueIceUrl()
        {
            const string a = "http://aice/";
            Assert.Equal(IceUtils.IsValidIceUrl(a),true);
        }
        
        
        [Fact]
        public void Test_IsValidUrl_Fails_TrueUrlWithoutContainsIce()
        {
            const string a = "http://a/";
            Assert.Equal(IceUtils.IsValidIceUrl(a),false);
        }
        
        [Fact]
        public void Test_IsValidUrl_Fails_NonUrl()
        {
            const string a = "a" ;
            Assert.Equal(IceUtils.IsValidIceUrl(a),false);
        }

        [Theory]
        [InlineData("http://wasteman.org/waste/",false)]
        [InlineData("http://wasteman.org/waste/",true)]
        [InlineData("http://wasteman.org/waste",false)]
        [InlineData("http://wasteman.org/waste",true)]
        public void Test_WithOrWithoutSlash(string url, bool trailingSlash)
        {
            if (trailingSlash)
            {
                var l = IceUtils.WithOrWithoutTrailingChar(url, '/', trailingSlash);
                Assert.EndsWith( "/",l,StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                var l = IceUtils.WithOrWithoutTrailingChar(url, '/', trailingSlash);
                Assert.NotEqual(l[l.Length - 1],'/');
            }
        }
        
    }
}