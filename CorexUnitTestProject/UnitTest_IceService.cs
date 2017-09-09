using System;
using System.Threading.Tasks;
using IceProvider;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;


namespace CorexUnitTestProject
{
    [Collection("CurrentDomainUpdates")]
    public class UnitTest_IceService_CurrentDomain
    {

        [Fact]
        public void Test_IceService_CurrentDomain_StartsEmpty()
        {
            var i = new IceProvider.IceService();
            Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Empty);

        }

        [Fact]
        public void Test_IceService_CurrentDomain_ResolvesToDefault()
        {
            var i = new IceProvider.IceService();
            i.IceUrlWithSlash(); // Calls CurrentDomainOrSetDefault
            Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Original);

        }


        [Fact]
        public void Test_IceService_CurrentDomain_AcceptsUpdatedUrl()
        {
            var i = new IceProvider.IceService();
            i.CurrentDomain = "http://ice/test";
            Assert.Equal(i.GetIceUrlStatus(), IceUrlStateEnum.Updated);

        }

    }

    [Collection("HttpGet and Post")]
    public class UnitTest_IceService_Http
    {
        [Fact]
        public async void Test_IceService_HttpGet_Not_Empty()
        {
            var i = new   IceProvider.IceService();
            //Task t = ;
                var Handle = await i.httpGet("https://www.bbc.co.uk/", "", "");
            //Task.WaitAll(t);
            var h = Handle;// t.Result;

            Assert.NotEmpty(h);
        }

    }
    
    
    
    
    [Collection("ResolveUrlAtInit")]
    public class UnitTest_IceService_ResolveUrlAtInit
    {

    [Fact]
        public async void Test_IceService_ResolveUrlAtInit_UpdatesUrl()
        {
            var i = new IceProvider.IceService();
            await Task.Run(()=>  i.resolveUrlAtInit() );
            Assert.NotEqual(i.CurrentDomain.Length,0);
            Assert.Equal(i.GetIceUrlStatus(),IceUrlStateEnum.Updated);
        }
        
    }

    [Collection("IgnoreHostList")]
    public class UnitTest_IceService_IgnoreHostList
    {
        [Fact]
        public void Test_IceService_IgnoreHostList_Not_Null()
        {
            var i = new   IceProvider.IceService();
            Assert.NotNull(i.IgnoreHostList);
        }
        
        [Fact]
        public void Test_IceService_IgnoreHostList_TakesNewHost()
        {
            var i = new   IceProvider.IceService();
            i.IgnoreHostList.Add("megaupload");
            Assert.NotNull(i.IgnoreHostList.Find(x=>x.Equals("megaupload")));
        }
        

    }

    [Collection("GetEpisodesFromUrl")]
    public class UnitTest_IceService_GetEpisodesFromUrl
    {
        [Fact]
        public async Task Test_IceService_ResultsGetClearedAndPopulated()
        {
            const string EP_URL = @"/tv/series/8/7367";
            var i = new IceProvider.IceService();
            i.GetLatestIceUrl(ForceUpdate: true);
            Assert.Equal(i.Results.Count,0);
            var url = i.IceUrlWithoutSlash() + EP_URL;
          await  i.GetEpisodesFromUrl(url);
            Assert.NotEmpty(i.Results);
        }
    }
}