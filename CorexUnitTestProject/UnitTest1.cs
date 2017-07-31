using System;
using IceProvider;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace CorexUnitTestProject
{
    public class UnitTest1
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

        }

        [Fact]
        public void Test_IsValidUrl_Passes_TrueUrl()
        {
            const string a = "http://aice/";
            Assert.Equal(IceUtils.IsValidIceUrl(a),true);
        }
        
        [Fact]
        public void Test_IsValidUrl_Fails_NonUrl()
        {
            const string a = "a" ;
            Assert.Equal(IceUtils.IsValidIceUrl(a),false);
        }
        
        
    }
}