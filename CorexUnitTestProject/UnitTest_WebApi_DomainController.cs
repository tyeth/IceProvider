using System;
using System.Threading.Tasks;
using IceProvider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;



namespace CorexUnitTestProject
{
    public class UnitTest_WebApi_DomainController
    {
        [Fact]
        public void Test_Controller_Not_Null()
        {
            var i = new   IceProvider.IceService();
            var a = new WebApiCoreApplication.Controllers.DomainController(i);
            var ret = a.Get();
            
            Assert.NotNull(ret);
        }
        
        
        [Fact]
        public void Test_Controller_AcceptsChange()
        {
            const string testurl = "http://ice/test";
            var i = new   IceProvider.IceService();
            var a = new WebApiCoreApplication.Controllers.DomainController(i);
            Assert.NotEqual(a.Get(withSlash: false),testurl);
            a.Post(testurl );
            var ret = a.Get(withSlash:false);
            Assert.Equal(ret,testurl);
        }
        
    }

    public class UnitTest_WebApi_EpisodeController
    {
        /// <summary>
        /// ////////////
        /// </summary>
        [Collection("GetEpisodesFromUrl")]
        public class UnitTest_IceService_GetEpisodesFromUrl
        {
            [Fact]
            public void Test_IceService_ResultsGetClearedAndPopulated()
            {
                var i = new   IceProvider.IceService();
                var a = new WebApiCoreApplication.Controllers.EpisodeController(i);//i);
                var ret = a.Get("8/737");
                i.GetLatestIceUrl(ForceUpdate: true);
                Assert.NotEqual("",new JsonResult(ret).ToString());//;t);
            }
        }
        
        
    }
}