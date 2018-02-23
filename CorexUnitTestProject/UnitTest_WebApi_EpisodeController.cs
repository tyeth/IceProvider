using IceProvider;
using Microsoft.AspNetCore.Mvc;
using WebApiCoreApplication.Controllers;
using Xunit;

namespace CorexUnitTestProject
{
    public class UnitTest_WebApi_EpisodeController
    {
        
        /// <summary>
        ///     ////////////
        /// </summary>
        [Collection("GetEpisodesSeriesFromUrl")]
        public class UnitTest_IceService_GetEpisodesSeriesFromUrl
        {
            [Fact]
            public void Test_IceService_ResultsGetClearedAndPopulated()
            {
                var i = new IceService();
                var a = new EpisodeController(i); //i);
                var ret = a.GetSeries("8/737");
                i.GetLatestIceUrl(true);
                Assert.NotEqual("", new JsonResult(ret).ToString()); //;t);
            }
            
            
        }
        
        /// <summary>
        ///     ////////////
        /// </summary>
        [Collection("GetEpisodeSourcesFromUrl")]
        public class UnitTest_IceService_GetEpisodeSourcesFromUrl
        {
            [Fact]
            public async void Test_IceService_ResultsGetClearedAndPopulated()
            {
                const string url = "https://icefilms.unblocked.pl/ip.php?v=242220&";
                var i = new IceService();
                var a = new EpisodeController(i); //i);
                i.GetLatestIceUrl(true);
                await i.httpGet(url,"");
                var ret = await a.Get(url);
                Assert.NotEqual("", ret.ToString()); //;t);
            }
            
            
            [Fact]
            public void Test_IceService_ResultsGetClearedAndPopulatedWithPartialUrl()
            {
                var i = new IceService();
                var a = new EpisodeController(i); //i);
                var ret = a.Get("/ip.php?v=242220&");
                i.GetLatestIceUrl(true);
                Assert.NotEqual("", new JsonResult(ret).ToString()); //;t);
            }
        }
    }
}