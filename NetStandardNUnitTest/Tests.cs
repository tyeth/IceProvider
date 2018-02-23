using NUnit.Framework;
using Ice.IcefilmsSeriesDownloader;

namespace NetStandardNUnitTest
{
    [TestFixture]
    public class Tests
    {
        private IcefilmsSeriesDownloader.frmSearch _ice;

        [SetUp]
        public void TestSetup()
        {
            _ice = new IcefilmsSeriesDownloader.frmSearch();

        }

        [Test]
        public void TestEpisode()
        {
            Assert.True(true);
        }
    }
}