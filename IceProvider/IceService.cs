using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IceProvider.IceUtils;

namespace IceProvider
{
    public class IceService : IIceService
    {
        public delegate void ListViewDelegate(IIceEpisode l);

        public delegate void ProgressDelegate(int progress, int total);

        public delegate void StatusDelegate(string status);

        public const string standardIceFilmsUrl = "https://www.icefilms.info/";

        private static readonly string unblocked = "https://unblocked-pw.github.io";

        private static string yahooReferrer = "https://query.yahooapis.com/";
        private static readonly string yqlHTMLtablePREFIX =
                   "USE%20%22http%3A%2F%2Fwww.datatables.org%2Fdata%2Fhtmlstring.xml%22%20AS%20htmlstring%3B%20";

        private static string unyql = "https://query.yahooapis.com/v1/public/yql?q=" + yqlHTMLtablePREFIX +
                                      "select%20*%20from%20htmlstring%20where%20url%3D%22https%3A%2F%2Funblocked-pw.github.io%2F%22%20and%20xpath%3D%22%2F%2Fa%22&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback="
            ;


        //TODO: take Episode implementation and recreate as API
        //TODO: Add support for IgnoreHostList
        //TODO: expand to support searching and series listing

        private CookieContainer _cookieContainer;
        private int _currentProgressTotal;
        private int _currentProgressValue;
        public List<string> IgnoreHostList = new List<string>(); // { "fileweed" };

        private ListViewDelegate _listViewDelegate;

        private ProgressDelegate _progressDelegate;
        private readonly Random _rand = new Random();

        public List<IIceEpisode> Results = new List<IIceEpisode>();

        private string _standardUseragent =
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
            ;

        private readonly List<string> _status = new List<string>();

        private readonly StatusDelegate _statusDelegate;
        private string _statusStream;
        private bool _useUSERAGENT = true;
        private bool _useStandardReferrer = false;

        public IceService()
        {
            _progressDelegate = UpdateProgress;
            _statusDelegate = UpdateStatus;
            _listViewDelegate = UpdateResults;
            _cookieContainer = new CookieContainer();
        }

        public string CurrentDomain { get; set; } = "";

        private void UpdateResults(IIceEpisode i)
        {
            Results.Add(i);
        }

        public IceUrlStateEnum GetIceUrlStatus()
        {
            switch (CurrentDomain)
            {
                case "":
                case null:
                    return IceUrlStateEnum.Empty;

                case standardIceFilmsUrl:
                    return IceUrlStateEnum.Original;

                default:
                    return IceUrlStateEnum.Updated;
            }
        }

        public void GetLatestIceUrl(bool forceUpdate = false)
        {
            try
            {
                if (GetIceUrlStatus() != IceUrlStateEnum.Updated || forceUpdate)
                {
                    GetIceUrlFromGithub();

                }
            }
            catch (Exception e)
            {
                console.log(e);
                UpdateStatus(string.Format("GetLatestUrl: Failed to get url, exception:{0} inner exception:{1}",e,e.InnerException));
                //throw;
            }
        }

        private void UpdateStatus(string status)
        {
            console.log("Status change: " + status);
            try
            {
                _status.Add(status);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // private updateListView(l:any) {
        //     this.lvEpisodes.Items.Add(l);
        //    this.lvEpisodes.Refresh();
        // }

        private void UpdateProgress(int progress, int total)
        {
            _currentProgressTotal = total;
            _currentProgressValue = progress;
            console.log("Progress change _-_ Total: " + total + " Done: " + progress);
        }

        private void GetIceUrlFromGithub()
        {
            try
            {
                Task.WaitAll(resolveUrlAtInit());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IIceEpisode[]> GetEpisodesFromSeriesUrl(string url,bool resetResults=true)
        {
            if(resetResults)Results.Clear();
            await GetSeries(url);
            return await Task.FromResult(Results.ToArray());
        }

        private async Task GetSeries(object urlo)
        {
            int doneEpisodes = 0, totalEpisodes = 0;
            var url = urlo.ToString();


            var showPage = await GetEpisodesFromPageByIPdotPhp(url, doneEpisodes);
            var episodeMatches = Regex.Matches(
                //match.SafelyGetGroupValue("episodes").ToString()
                showPage
                , "ip.php\\?v=(?<vid>[0-9]+)&[\"']?>(?<season>[0-9]+)x(?<episode>[0-9]+) (?<title>.*?)</a>");
            if (episodeMatches.Count < 1)
            {
                //textOutput.Text = "Error: No episodes found for season #" + match.SafelyGetGroupValue("season");
                //MessageBox.Show("Error: No episodes found for season #" + match.SafelyGetGroupValue("season") + ".");
                Invoke(_progressDelegate, new object[] { 0, 100 });
                Invoke(_statusDelegate, new object[]
                {
                    "Error: No episodes found for season #" +
                    1 +
                    //match.SafelyGetGroupValue("season") + 
                    "."
                });
                return;
            }
            totalEpisodes = episodeMatches.Count;
            Invoke(_progressDelegate, new object[] {doneEpisodes, totalEpisodes});
            foreach (Match episode in episodeMatches)
            {
                await GetEpisode(episode, totalEpisodes,doneEpisodes);
            }
            Invoke(_progressDelegate, new object[] { 100, 100 });
            Invoke(_statusDelegate, new object[] { "Done!" });
        }

        private async Task<string> GetEpisodesFromPageByIPdotPhp(string url, int doneEpisodes)
        {
            int totalEpisodes;
            Invoke(_statusDelegate, new object[] {"Grabbing IceFilms page..."});
            var showPage = await httpGet(url, "");
            if (showPage == "")
            {
                //textOutput.Text = "Error: Couldn't grab page. Icefilms down?";
                Invoke(_progressDelegate, new object[] {0, 100});
                Invoke(_statusDelegate, new object[] {"Error: Couldn't grab page. Is icefilms down?"});
                throw new Exception(_statusStream);
            }

            // Find the number of episodes
            var eMatches = Regex.Matches(showPage, "<a href=(\\\")?/ip.php");
            totalEpisodes = eMatches.Count;
            
            Invoke(_statusDelegate, new object[] {"Found " + totalEpisodes + " episodes."});
            return showPage;
        }

        public async Task GetEpisode(string url, int totalEpisodes = 1, bool resetResults = true)
        {
            if(resetResults)Results.Clear();
           await GetEpisode(Regex.Match(url, "[='\"]?(.*?ip.php\\?v=(?<vid>[0-9]+)&)"),totalEpisodes);
        }
        private async Task GetEpisode(Match episode, int totalEpisodes=1,int doneEpisodes=0)
        {
            await GetSecretSourcesPage(episode, out var sourcesPage, out var sec);
            if (sec == "")
            {
                // Oh well... let's just skip this episode.
                Console.WriteLine("Error: Couldn't find sec " + episode.SafelyGetGroupValue("season") + "x" +
                                  episode.SafelyGetGroupValue("episode"));
                return;
            }

             
            var sourceMatches = Regex.Matches(sourcesPage, "go\\(([0-9]+)\\)['\"]?>Source #[0-9]+:.*?</a>");
            var breakOut = false;
            var retrylogic = false;
            var myArr = Array.Empty<Match>();

            for (var i = sourceMatches.Count - 1; i >= 0 && !breakOut; i--)
            {
                var filehost = sourceMatches[i].Groups[0].ToString();
                filehost = filehost.Substring(filehost.IndexOf("<span", StringComparison.OrdinalIgnoreCase));
                var fsizePos = filehost.IndexOf("<span class=\'fsize", StringComparison.OrdinalIgnoreCase);
                var fsize = fsizePos == -1 ? "-1" : GetCleanHtml(filehost.Substring(fsizePos));
                var fname = fsizePos == -1 ? "" : GetTitleAttributeFromHtml(filehost.Substring(fsizePos));
                if (fsizePos >= 0) filehost = filehost.Substring(0, fsizePos);
                filehost = GetCleanHtml(filehost);
                if (IsInHostIgnoreList(filehost))
                    if (sourceMatches.Count - i - 1 > 1)
                        continue;
                    else
                        Console.WriteLine("no alternative sources left, using host anyway");

                var retries = 0;
                var sourceResponse = await getSourceDetails(episode, sec, sourceMatches, i);
                if (sourceResponse == "" && retrylogic)
                {
                    tryagain:
                    Console.Write("Error: Retrying source #" + i + " for " + episode.SafelyGetGroupValue("season") + "x" +
                                  episode.SafelyGetGroupValue("episode") + ".");
                    string tsourcesPage;
                    //textOutput.Text += "Error: Couldn't grab source for " + episode.SafelyGetGroupValue("season") + "x" + episode.SafelyGetGroupValue("episode") + "." ;

                    // Get fresh "sec" code from episode page;
                    await GetSecretSourcesPage(episode, out tsourcesPage, out sec);
                    sourceResponse = await getSourceDetails(episode, sec, sourceMatches, i);
                    retries++;
                    if (sourceResponse != "")
                    {
                        Console.Write("Success!" + Environment.NewLine);
                    }
                    else
                    {
                        Console.Write("\t Re-Retrying source #" + i + " for " + episode.SafelyGetGroupValue("season") + "x" +
                                      episode.SafelyGetGroupValue("episode") + ".");
                        await GetSecretSourcesPage(episode, out tsourcesPage, out sec);
                        sourceResponse = await getSourceDetails(episode, sec, sourceMatches, i);
                        retries++;
                        if (sourceResponse != "")
                        {
                            Console.Write("Success!" + Environment.NewLine);
                        }
                        else
                        {
                            Console.Write("Failed: Source #" + i + " S" + episode.SafelyGetGroupValue("season") + "E" +
                                          episode.SafelyGetGroupValue("episode") + Environment.NewLine);
                            continue;
                        }
                    }
                    if (retries < 5) goto tryagain;
                }
                 if (!sourceResponse.Contains("GMorBMlet"))
                    continue;
                var urlSplit = Regex.Split(sourceResponse, "GMorBMlet\\.php\\?url=");
                var sourceURL = urlSplit[1];
                 string[] cols =
                {
                    episode.SafelyGetGroupValue("season") , episode.SafelyGetGroupValue("episode") ,
                    WebUtility.HtmlDecode(episode.SafelyGetGroupValue("title") ) + string.Format(" ({0}", fsize),
                    Uri.UnescapeDataString(sourceURL)
                };
                var lvEpisode = new ListViewItem
                {
                    Name = WebUtility.HtmlDecode(episode.SafelyGetGroupValue("title") ) + string.Format(" ({0}", fsize),
                    
                    Url = Uri.UnescapeDataString(sourceURL),
                    Checked = i == 0 ? true : false, FileName = fname
                };
                 Invoke(_listViewDelegate, new object[] {lvEpisode});
                 
            }
            doneEpisodes++;
            Invoke(_progressDelegate, new object[] {doneEpisodes, totalEpisodes});
        }

        internal void Invoke(Delegate d, object[] p)
        {
            try
            {
                d.DynamicInvoke(p);
            }
            catch (Exception)
            {
                console.log("failed to invoke deleggate");
                // throw;
            }
        }

        public async Task<string> httpGet(string strPage, string strVars)
        {
            return await httpGet(strPage, strVars, _useStandardReferrer ? standardIceFilmsUrl : IceUrlWithSlash());
        }

        internal async Task<string> httpGet(string strPage, string strVars, string referer)
        {
            //Initialization
            var WebReq = (HttpWebRequest)WebRequest.Create(string.Format("{0}{1}", strPage, strVars));
            //This time, our method is GET.
            ConfigureGetRequest(referer, WebReq);
            //From here on, it's all the same as above.
            var WebResp = await WebReq.GetResponseAsync();
            //Let's show some information about the response
            //Console.WriteLine(WebResp.StatusCode);
            //Console.WriteLine(WebResp.Server);

            //Now, we read the response (the string), and output it.
            var Answer = WebResp.GetResponseStream();
            var _Answer = new StreamReader(Answer);
            return _Answer.ReadToEnd();
        }

        private void ConfigureGetRequest(string referer, HttpWebRequest WebReq)
        {
            WebReq.Method = "GET";
            WebReq.Headers.AddOrUpdate("Referer", referer);
            WebReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            WebReq.Headers.AddOrUpdate("Accept-Language", "en-US,en;q=0.8");
            WebReq.Headers.AddOrUpdate("Upgrade-Insecure-Requests", "1");
            WebReq.Headers.AddOrUpdate("User-Agent",
                _useUSERAGENT
                    ? _standardUseragent
                    : "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
            );
            // Timeout
            WebReq.ContinueTimeout = 60000; // 10 second timeout. Should be long enough.

            WebReq.CookieContainer = _cookieContainer;
        }


        private bool IsInHostIgnoreList(string filehost)
        {
            return IgnoreHostList.Contains(filehost.ToLower());
        }

        private async Task<string> getSourceDetails(Match episode, string sec, MatchCollection sourceMatches, int i)
        {
            //this.Invoke(statusDelegate, new object[] { "Trying source #" + i + " for " + episode.SafelyGetGroupValue("season") + "x" + episode.SafelyGetGroupValue("episode") + "..." });
            // Craft our POST data
            var postData = "id=" + sourceMatches[i].Groups[1] + "&s=" + (10000 + _rand.Next(2, 18)) +
                           "&iqs=&url=&m=" + (10000 + _rand.Next(80, 200)) + "&cap= &sec=" + sec + "&t=" +
                           episode.SafelyGetGroupValue("vid");

            var sourceResponse = await HttpPost(
                IceUrlWithSlash() + "membersonly/components/com_iceplayer/video.phpAjaxResp.php?s=" +
                sourceMatches[i].Groups[1] + "&t=" + episode.SafelyGetGroupValue("vid"), postData,
                IceUrlWithSlash() + "membersonly/components/com_iceplayer/video.php?h=374&w=631&vid=" +
                episode.SafelyGetGroupValue("vid") + "&img=");
            return sourceResponse;
        }

        private Task GetSecretSourcesPage(Match episode, out string sourcesPage, out string sec)
        {
            this.Invoke(_statusDelegate, new object[] { "Grabbing sources for " + episode.SafelyGetGroupValue("season") + "x" + episode.SafelyGetGroupValue("episode") + "..." });
            var t = httpGet(
                IceUrlWithSlash() + "membersonly/components/com_iceplayer/video.php?h=374&w=631&vid=" +
                episode.SafelyGetGroupValue("vid") + "&img=", "", IceUrlWithSlash() + "ip.php?v=" + episode.SafelyGetGroupValue("vid"));
            Task.WaitAll(t);
            sourcesPage = t.Result;

            //            sourcesPage = httpGet("https://ipv6.icefilms.info/membersonly/components/com_iceplayer/video.php?h=374&w=631&vid=" + episode.SafelyGetGroupValue("vid") + "&img=", "", "https://ipv6.icefilms.info/ip.php?v=" + episode.SafelyGetGroupValue("vid"));
            // # video.php might (should) give us a cookie. We need to remember it!
            // Need to extract sec. This is the same for any source.
            var secMatch = Regex.Match(sourcesPage, "f\\.lastChild\\.value=\"(?<sec>.*?)\"");
            sec = secMatch.SafelyGetGroupValue("sec") ;
            return Task.FromResult(sourcesPage);
        }

        private string HttpPost(string strPage, string strBuffer)
        {
            var t = HttpPost(strPage, strBuffer, _useStandardReferrer ? standardIceFilmsUrl : IceUrlWithSlash());
            Task.WaitAll(t);
            return t.Result;
        }

        private async Task<string> HttpPost(string strPage, string strBuffer, string referer)
        {
            try
            {
                //Our postvars
                var buffer = Encoding.ASCII.GetBytes(strBuffer);
                //Initialization
                //   System.Net.ServicePointManager.Expect100Continue = false;
                var WebReq = (HttpWebRequest)WebRequest.Create(strPage);

                ConfigurePostRequest(referer, WebReq, buffer);
                Console.WriteLine(WebReq.RequestUri);
                //We open a stream for writing the postvars
                var PostData = await WebReq.GetRequestStreamAsync();
                //Now we write, and afterwards, we close. Closing is always important!

                await PostData.WriteAsync(buffer, 0, buffer.Length);
                //   await PostData.FlushAsync();
                //Get the response handle, we have no true response yet!
                var WebResp = (HttpWebResponse)await WebReq.GetResponseAsync();
                //Let's show some information about the response
                //Console.WriteLine(WebResp.StatusCode);
                //Console.WriteLine(WebResp.Server);

                //Now, we read the response (the string), and output it.
                var Answer = WebResp.GetResponseStream();
                var _Answer = new StreamReader(Answer);
                return await _Answer.ReadToEndAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(((WebException)e.InnerException).Status);
                Console.WriteLine(((WebException)e.InnerException).Response);
                throw;
            }
        }

        private void ConfigurePostRequest(string referer, HttpWebRequest WebReq, byte[] buffer)
        {
            WebReq.Accept = "application/json";
            WebReq.Headers.AddOrUpdate("Referer", referer);
            WebReq.Headers.AddOrUpdate("Accept-Language", "en-US,en;q=0.8");
            WebReq.Headers.AddOrUpdate("Upgrade-Insecure-Requests", "1");
            WebReq.Headers.AddOrUpdate("User-Agent",
                _useUSERAGENT
                    ? _standardUseragent
                    : "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
            );
            // Timeout
            WebReq.ContinueTimeout = 60000; // 10 second timeout. Should be long enough.

            WebReq.CookieContainer = _cookieContainer;
            //From here on, it's all the same as above.
            //var WebResp = await WebReq.GetResponseAsync();


            WebReq.Method = "POST";
            //Timeout
            // WebReq.ContinueTimeout = 10000; // 10 second timeout. Should be long enough.
            //We use form contentType, for the postvars.
            WebReq.ContentType = "application/x-www-form-urlencoded";
            //The length of the buffer (postvars) is used as contentlength.
            //WebReq.ContentLength = buffer.Length;
            //WebReq.Referer = referer;
            WebReq.Headers["Origin"] = IceUrlWithoutSlash(); //"https://ipv6.icefilms.info";
            WebReq.Accept = "*/*";
            WebReq.Headers.AddOrUpdate("Accept-Encoding", "en-GB,en-US;q=0.8,en;q=0.6");
            // WebReq.Headers.Add("Upgrade-Insecure-Requests", "1");
            WebReq.Headers.AddOrUpdate("User-Agent",
                _useUSERAGENT
                    ? _standardUseragent
                    : "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
            );
            WebReq.Headers.AddOrUpdate("Content-Length", buffer.Length.ToString());
            WebReq.CookieContainer = _cookieContainer;
        }


        /// <summary>
        ///     Resolve Domain via listings site or similar example
        /// </summary>
        /// <returns>Task</returns>
        public async Task resolveUrlAtInit()
        {
            var doneNow = false;
            //var my = await httpGet(unyql, "",yahooReferrer);
            var result = await httpGet(unblocked, "", unblocked);

            //var json = JsonConvert.DeserializeObject<YQL.RootObject>(my);
            //var result = json?.query?.results?.result;
            // console.log(result);
            var rx = new Regex("<a.*?href=['\"](.*?icefilms.*?)['\"]",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);

            var ix = new Regex("icefilms",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);


            var x = rx.Matches(result);
            console.log(x);
            foreach (Match match in x)
                CurrentDomain = match.Groups[1].Value;

            //this.currentDomain = x;
        }

        public string IceUrlWithoutSlash()
        {
            try
            {
                CurrentDomainOrSetDefault();
                return WithOrWithoutTrailingChar(CurrentDomain, '/', false);
            }
            catch (Exception e)
            {
                console.log("Fatal error returning iceurl withOUT a slash, returned CurrentDomain anyway");
                console.log(e);
                return CurrentDomain;
            }
        }

        internal void CurrentDomainOrSetDefault()
        {
            if (CurrentDomain == "") CurrentDomain = standardIceFilmsUrl;
        }

        public string IceUrlWithSlash()
        {
            try
            {
                CurrentDomainOrSetDefault();
                return WithOrWithoutTrailingChar(CurrentDomain, '/', true);
            }
            catch (Exception e)
            {
                console.log("Fatal error returning iceurl with a slash, returned CurrentDomain anyway");
                console.log(e);
                return CurrentDomain;
            }
        }
    }
}