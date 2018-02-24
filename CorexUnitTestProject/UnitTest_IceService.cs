using System.Threading.Tasks;
using IceProvider;
using Xunit;

namespace CorexUnitTestProject
{
    [Collection("CurrentDomainUpdates")]
    public class UnitTest_IceService_CurrentDomain
    {
        [Fact]
        public void Test_IceService_CurrentDomain_AcceptsUpdatedUrl()
        {
            var i = new IceService();
            i.CurrentDomain = "http://ice/test";
              Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Updated);
        }

        [Fact]
        public void Test_IceService_CurrentDomain_ResolvesToDefault()
        {
            var i = new IceService();
            i.IceUrlWithSlash(); // Calls CurrentDomainOrSetDefault
            Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Original);
        }

        [Fact]
        public void Test_IceService_CurrentDomain_StartsEmpty()
        {
            var i = new IceService();
            Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Empty);
        }
    }

    [Collection("HttpGet and Post")]
    public class UnitTest_IceService_Http
    {
        [Fact]
        public async void Test_IceService_HttpGet_Not_Empty()
        {
            var i = new IceService();
            //Task t = ;
            var Handle = await i.httpGet("https://www.bbc.co.uk/", "", "");
            //Task.WaitAll(t);
            var h = Handle; // t.Result;

            Assert.NotEmpty(h);
        }
    }


    [Collection("ResolveUrlAtInit")]
    public class UnitTest_IceService_ResolveUrlAtInit
    {
        [Fact]
        public async void Test_IceService_ResolveUrlAtInit_UpdatesUrl()
        {
            var i = new IceService();
            await Task.Run(() => i.resolveUrlAtInit());
            Assert.NotEqual(i.CurrentDomain.Length, 0);
            Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Updated);
        }
    }

    [Collection("IgnoreHostList")]
    public class UnitTest_IceService_IgnoreHostList
    {
        [Fact]
        public void Test_IceService_IgnoreHostList_Not_Null()
        {
            var i = new IceService();
            Assert.NotNull(i.IgnoreHostList);
        }

        [Fact]
        public void Test_IceService_IgnoreHostList_TakesNewHost()
        {
            var i = new IceService();
            i.IgnoreHostList.Add("megaupload");
            Assert.NotNull(i.IgnoreHostList.Find(x => x.Equals("megaupload")));
        }
    }

    [Collection("GetEpisodesFromUrl")]
    public class UnitTest_IceService_GetEpisodesFromUrl
    {
        [Fact]
        public async Task Test_IceService_ResultsGetClearedAndPopulated()
        {
            const string EP_URL = @"/tv/series/6/5231";
            var i = new IceService();
            i.GetLatestIceUrl(true);
            Assert.Equal(i.Results.Count, 0);
            var url = i.IceUrlWithoutSlash() + EP_URL;
            await i.GetEpisodesFromSeriesUrl(url);
            Assert.NotEmpty(i.Results);
        }
    }
}