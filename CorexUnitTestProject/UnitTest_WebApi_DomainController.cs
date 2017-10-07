using IceProvider;
using Microsoft.AspNetCore.Mvc;
using WebApiCoreApplication.Controllers;
using Xunit;

namespace CorexUnitTestProject
{
    public class UnitTest_WebApi_DomainController
    {
        [Fact]
        public void Test_Controller_AcceptsChangeOfDomain()
        {
            const string testurl = "http://ice/test";
            var i = new IceService();
            var a = new DomainController(i);
            Assert.NotEqual(a.Get(false), testurl);
            a.Post(testurl);
            var ret = a.Get(false);
            Assert.Equal(ret, testurl);
        }

        [Fact]
        public void Test_Controller_Not_Null()
        {
            var i = new IceService();
            var a = new DomainController(i);
            var ret = a.Get();

            Assert.NotNull(ret);
        }
    }

}