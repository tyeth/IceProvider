using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static IceProvider.IceUtils;

namespace IceProvider
{
    public class IceService : IIceService
    {
        private List<string> status = new List<string>();
        public Random rand = new Random();
        public int currentProgressValue = 0;
        public int currentProgressTotal = 0;
        public string statusStream;
        public CookieContainer cookieContainer;

        public delegate void ProgressDelegate(int progress, int total);

        public ProgressDelegate progressDelegate;

        public delegate void ListViewDelegate(IIceEpisode l);

        public ListViewDelegate listViewDelegate;

        public delegate void StatusDelegate(string status);

        public StatusDelegate statusDelegate;
        public const string standardIceFilmsUrl = "https://www.icefilms.info/";
        public bool useStandardReferrer = false;
        public bool useUSERAGENT = true;
        public List<string> IgnoreHostList = new List<string>(); // { "fileweed" };

        public string standardUSERAGENT =
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
            ;

        public string CurrentDomain { get; set; } = "";

        private static string unblocked = "https://unblocked-pw.github.io";

        private static string yahooReferrer = "https://query.yahooapis.com/";
        private static string unyql = "https://query.yahooapis.com/v1/public/yql?q=" + IceService.yqlHTMLtablePREFIX +
                                      "select%20*%20from%20htmlstring%20where%20url%3D%22https%3A%2F%2Funblocked-pw.github.io%2F%22%20and%20xpath%3D%22%2F%2Fa%22&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback="
            ;

        private static string yqlHTMLtablePREFIX =
            "USE%20%22http%3A%2F%2Fwww.datatables.org%2Fdata%2Fhtmlstring.xml%22%20AS%20htmlstring%3B%20";

        public List<IIceEpisode> Results = new List<IIceEpisode>();

        //TODO: take Episode implementation and recreate as API
        //TODO: Add support for IgnoreHostList
        //TODO: expand to support searching and series listing
        private HttpMessageInvoker _http;

        public IceService() : this(new HttpClient())
        {
        }

        public IceService(HttpMessageInvoker http)
        {
            _http = http;
            progressDelegate = this.updateProgress;
            statusDelegate = this.updateStatus;
            listViewDelegate = this.updateResults;
            cookieContainer = new CookieContainer();
        }

        public void updateResults(IIceEpisode i)
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
                    break;

                case standardIceFilmsUrl:
                    return IceUrlStateEnum.Original;
                    break;

                default:
                    return IceUrlStateEnum.Updated;
                    break;
            }
        }

        public bool GetLatestIceUrl(bool ForceUpdate = false)
        {
            try
            {
                if (GetIceUrlStatus() != IceUrlStateEnum.Updated || ForceUpdate)
                {
                    GetIceUrlFromGithub();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                console.log(e);
                //throw;
            }

            return false;
        }
        private void updateStatus( string status)
        {
            console.log("Status change: " + status);
            this.status.Add(status);
        }

        // private updateListView(l:any) {
        //     this.lvEpisodes.Items.Add(l);
        //    this.lvEpisodes.Refresh();
        // }

        private void updateProgress(int progress , int total )
        {
            currentProgressTotal = total;
            currentProgressValue = progress;
            console.log("Progress change _-_ Total: " + total + " Done: " + progress);
        }
        internal void GetIceUrlFromGithub()
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

        public async Task<IIceEpisode[]> GetEpisodesFromUrl(string url)
        {
            await grabData(url);
            return await Task.FromResult(this.Results.ToArray());
        }
        
        private async  Task grabData(object urlo)
        {
            int doneEpisodes = 0, totalEpisodes = 0;
            string url = urlo.ToString();
            bool megaUpOnly = false; 
         
         
            this.Invoke(statusDelegate, new object[] { "Grabbing IceFilms page..." });
            String showPage = await httpGet(url, "");
            if (showPage == "")
            {
                //textOutput.Text = "Error: Couldn't grab page. Icefilms down?";
                this.Invoke(progressDelegate, new object[] { (int)0, (int)100 });
                this.Invoke(statusDelegate, new object[] { "Error: Couldn't grab page. Is icefilms down?" });
                throw new Exception(statusStream);
                return;
            }

            // Find the number of episodes
            MatchCollection eMatches = Regex.Matches(showPage, "<a href=(\\\")?/ip.php");
            totalEpisodes = eMatches.Count;
            this.Invoke(progressDelegate, new object[] { doneEpisodes, totalEpisodes });
            this.Invoke(statusDelegate, new object[] { "Found " + totalEpisodes + " episodes." });
            //<a name=[0-9]+></a>(?<title>Season (?<season>[0-9]+) )?[\(]?(?<year>[0-9]{4})[\\)]?.*?</h3>(?<episodes>.*?)(<h3>|</span>)
            //MatchCollection matches = Regex.Matches(showPage, "<a name=[0-9]+></a>(Season (?<season>[0-9]+) )?[\\(]?(?<year>[0-9\\?]{4})[\\)]?.*?</h3>(?<episodes>.*?)(?:<h3>|</span>)");
            //if (matches.Count < 1)
            //{
            //    //textOutput.Text = "Error: No seasons found.";
            //    //MessageBox.Show("Error: No seasons found.");
            //    this.Invoke(progressDelegate, new object[] { (int)0, (int)100 });
            //    this.Invoke(statusDelegate, new object[] { "Error: No seasons found." });
            //    return;
            //}

                //textOutput.Text += "Season: " + match.Groups["season"] + " -- Year: " + match.Groups["year"] ;
                MatchCollection episodeMatches = Regex.Matches(
                    //match.Groups["episodes"].ToString()
                    showPage
                    , "ip.php\\?v=(?<vid>[0-9]+)&[\"']?>(?<season>[0-9]+)x(?<episode>[0-9]+) (?<title>.*?)</a>");
                if (episodeMatches.Count < 1)
                {
                    //textOutput.Text = "Error: No episodes found for season #" + match.Groups["season"];
                    //MessageBox.Show("Error: No episodes found for season #" + match.Groups["season"] + ".");
                    this.Invoke(progressDelegate, new object[] { (int)0, (int)100 });
                    this.Invoke(statusDelegate, new object[] { "Error: No episodes found for season #" +
                        1 +
                        //match.Groups["season"] + 
                        "." });
                    return;
                }
                foreach (Match episode in episodeMatches)
                {
                    String sourcesPage;
                    String sec;
                    await getSecretSourcesPage(episode, out sourcesPage, out sec);
                    if (sec == "")
                    {
                        // Oh well... let's just skip this episode.
                        //textOutput.Text += "Error: Couldn't find sec " + episode.Groups["season"] + "x" + episode.Groups["episode"] ;
                        Console.WriteLine("Error: Couldn't find sec " + episode.Groups["season"] + "x" + episode.Groups["episode"]);
                        continue;
                    }

                    //textOutput.Text += "  Season " + episode.Groups["season"] + ", Episode " + episode.Groups["episode"] + ": " + WebUtility.HtmlDecode(episode.Groups["title"].ToString()) + " -- ID: " + episode.Groups["vid"] ;                    

                    MatchCollection sourceMatches = Regex.Matches(sourcesPage, "go\\(([0-9]+)\\)['\"]?>Source #[0-9]+:.*?</a>");
                    bool breakOut = false;
                    bool retrylogic = false;
                    var myArr = Array.Empty<Match>();
                    
                    for (int i = (sourceMatches.Count - 1); i >= 0 && !breakOut; i--)
                    {
                        var filehost = sourceMatches[i].Groups[0].ToString();
                        filehost = filehost.Substring(filehost.IndexOf("<span", StringComparison.OrdinalIgnoreCase));
                    int fsizePos = filehost.IndexOf("<span class=\"fsize", StringComparison.OrdinalIgnoreCase);
                    var fsize =GetCleanHtml( filehost.Substring(fsizePos) );
                    filehost = filehost.Substring(0, fsizePos);
                        filehost = GetCleanHtml(filehost);
                        if (IsInHostIgnoreList(filehost))
                        {
                            if (sourceMatches.Count - i - 1 > 1)
                            {continue;}
                            else
                            {Console.WriteLine("no alternative sources left, using host anyway");}
                        }
                        
                        int retries = 0;
                        String sourceResponse = await getSourceDetails(episode, sec, sourceMatches, i);
                        if (sourceResponse == "" && retrylogic)
                        {
                            tryagain:
                            Console.Write("Error: Retrying source #" + i + " for " + episode.Groups["season"] + "x" + episode.Groups["episode"] + ".");
                            String tsourcesPage;
                            //textOutput.Text += "Error: Couldn't grab source for " + episode.Groups["season"] + "x" + episode.Groups["episode"] + "." ;

                            // Get fresh "sec" code from episode page;
                            await getSecretSourcesPage(episode, out tsourcesPage, out sec);
                            sourceResponse = await getSourceDetails(episode, sec, sourceMatches, i);
                            retries++;
                            if (sourceResponse != "") { Console.Write("Success!" + Environment.NewLine); }
                            else
                            {
                                Console.Write("\t Re-Retrying source #" + i + " for " + episode.Groups["season"] + "x" + episode.Groups["episode"] + ".");
                                await getSecretSourcesPage(episode, out tsourcesPage, out sec);
                                sourceResponse = await getSourceDetails(episode, sec, sourceMatches, i);
                                retries++;
                                if (sourceResponse != "") { Console.Write("Success!" + Environment.NewLine); }
                                else
                                {
                                    Console.Write("Failed: Source #" + i + " S" + episode.Groups["season"] + "E" + episode.Groups["episode"] + Environment.NewLine);
                                    continue;
                                }
                            }
                            if (retries < 5) goto tryagain;
                        }
                        //if (IsNumeric(sourceResponse))
                        if (!sourceResponse.Contains("GMorBMlet"))
                        {
                            //textOutput.Text += "Error: "+sourceResponse+" -- Couldn't grab source for " + episode.Groups["season"] + "x" + episode.Groups["episode"] + "." ;
                            continue;
                        }
                        String[] urlSplit = Regex.Split(sourceResponse, "GMorBMlet\\.php\\?url=");
                        String sourceURL = urlSplit[1];
                        //textOutput.Text += episode.Groups["season"] + "x" + episode.Groups["episode"] + " -- " + Uri.UnescapeDataString(sourceURL) ;
                        String[] cols = { episode.Groups["season"].ToString(), episode.Groups["episode"].ToString(), WebUtility.HtmlDecode(episode.Groups["title"].ToString()) + String.Format(" ({0}",fsize), Uri.UnescapeDataString(sourceURL) };
                        ListViewItem lvEpisode = new ListViewItem(cols);
                        lvEpisode.Checked = i == 0 ? true : false;
                        //lvEpisodes.Items.Add(lvEpisode);
                        this.Invoke(listViewDelegate, new object[] { lvEpisode });
                        // break;

                        continue;

                    }
                    doneEpisodes++;
                    this.Invoke(progressDelegate, new object[] { doneEpisodes, totalEpisodes });
                
            }
            this.Invoke(progressDelegate, new object[] { (int)100, (int)100 });
            this.Invoke(statusDelegate, new object[] { "Done!" });
        }

        internal void Invoke(Delegate d, object[] p)
        {
            try
            {

            d.DynamicInvoke(p);
            }
            catch (global::System.Exception)
            {
                console.log("failed to invoke deleggate");
               // throw;
            }
        }
        
        internal async Task<String> httpGet(string strPage, string strVars)
        {
            return await httpGet(strPage, strVars, useStandardReferrer ? standardIceFilmsUrl : IceUrlWithSlash());
        }

        internal async Task<String> httpGet(string strPage, string strVars, string referer)
        {
            //Initialization
            HttpWebRequest WebReq = (HttpWebRequest) WebRequest.Create(string.Format("{0}{1}", strPage, strVars));
            //This time, our method is GET.
            WebReq.Method = "GET";
            WebReq.Headers.AddOrUpdate("Referer", referer);
            WebReq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            WebReq.Headers.AddOrUpdate("Accept-Language", "en-US,en;q=0.8");
            WebReq.Headers.AddOrUpdate("Upgrade-Insecure-Requests", "1");
            WebReq.Headers.AddOrUpdate("User-Agent",
                useUSERAGENT
                    ? standardUSERAGENT
                    : "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
            );
            // Timeout
            WebReq.ContinueTimeout = 60000; // 10 second timeout. Should be long enough.

            WebReq.CookieContainer = cookieContainer;
            //From here on, it's all the same as above.
            var WebResp = await WebReq.GetResponseAsync();
            //Let's show some information about the response
            //Console.WriteLine(WebResp.StatusCode);
            //Console.WriteLine(WebResp.Server);

            //Now, we read the response (the string), and output it.
            Stream Answer = WebResp.GetResponseStream();
            StreamReader _Answer = new StreamReader(Answer);
            return _Answer.ReadToEnd();
        }


        private bool IsInHostIgnoreList(string filehost)
        {
            return IgnoreHostList.Contains(filehost.ToLower());
        }

        private async Task<string> getSourceDetails(Match episode, String sec, MatchCollection sourceMatches, int i)
        {
            //this.Invoke(statusDelegate, new object[] { "Trying source #" + i + " for " + episode.Groups["season"] + "x" + episode.Groups["episode"] + "..." });
            // Craft our POST data
            String postData = "id=" + sourceMatches[i].Groups[1] + "&s=" + (10000 + rand.Next(2, 18)) +
                              "&iqs=&url=&m=" + (10000 + rand.Next(80, 200)) + "&cap= &sec=" + sec + "&t=" +
                              episode.Groups["vid"];

            String sourceResponse = await httpPost(
                IceUrlWithSlash() + "membersonly/components/com_iceplayer/video.phpAjaxResp.php?s=" +
                sourceMatches[i].Groups[1] + "&t=" + episode.Groups["vid"], postData,
                IceUrlWithSlash() + "membersonly/components/com_iceplayer/video.php?h=374&w=631&vid=" +
                episode.Groups["vid"] + "&img=");
            return sourceResponse;
        }

        private Task getSecretSourcesPage(Match episode, out String sourcesPage, out String sec)
        {
            //this.Invoke(statusDelegate, new object[] { "Grabbing sources for " + episode.Groups["season"] + "x" + episode.Groups["episode"] + "..." });
            var t = httpGet(
                IceUrlWithSlash() + "membersonly/components/com_iceplayer/video.php?h=374&w=631&vid=" +
                episode.Groups["vid"] + "&img=", "", IceUrlWithSlash() + "ip.php?v=" + episode.Groups["vid"]);
            Task.WaitAll(t);
            sourcesPage = t.Result;

            //            sourcesPage = httpGet("https://ipv6.icefilms.info/membersonly/components/com_iceplayer/video.php?h=374&w=631&vid=" + episode.Groups["vid"] + "&img=", "", "https://ipv6.icefilms.info/ip.php?v=" + episode.Groups["vid"]);
            // # video.php might (should) give us a cookie. We need to remember it!
            // Need to extract sec. This is the same for any source.
            Match secMatch = Regex.Match(sourcesPage, "f\\.lastChild\\.value=\"(?<sec>.*?)\"");
            sec = secMatch.Groups["sec"].ToString();
            return Task.FromResult(sourcesPage);
        }

        private String httpPost(string strPage, string strBuffer)
        {
            var t = httpPost(strPage, strBuffer, useStandardReferrer ? standardIceFilmsUrl : IceUrlWithSlash());
            Task.WaitAll(t);
            return t.Result;
        }

        private async Task<String> httpPost(string strPage, string strBuffer, string referer)
        {
            try
            {
                //Our postvars
                byte[] buffer = Encoding.ASCII.GetBytes(strBuffer);
                //Initialization
                //   System.Net.ServicePointManager.Expect100Continue = false;
                HttpWebRequest WebReq = (HttpWebRequest) WebRequest.Create(strPage);
                //Our method is post, otherwise the buffer (postvars) would be useless

                WebReq.Accept = "application/json"; 
                WebReq.Headers.AddOrUpdate("Referer", referer);
                WebReq.Headers.AddOrUpdate("Accept-Language", "en-US,en;q=0.8");
                WebReq.Headers.AddOrUpdate("Upgrade-Insecure-Requests", "1");
                WebReq.Headers.AddOrUpdate("User-Agent",
                    useUSERAGENT
                        ? standardUSERAGENT
                        : "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
                );
                // Timeout
                WebReq.ContinueTimeout = 60000; // 10 second timeout. Should be long enough.

                WebReq.CookieContainer = cookieContainer;
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
                    useUSERAGENT
                        ? standardUSERAGENT
                        : "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36"
                );
                WebReq.Headers.AddOrUpdate("Content-Length", buffer.Length.ToString());
                WebReq.CookieContainer = cookieContainer;
                Console.WriteLine(WebReq.RequestUri);
                //We open a stream for writing the postvars
                Stream PostData = await WebReq.GetRequestStreamAsync();
                //Now we write, and afterwards, we close. Closing is always important!

                await PostData.WriteAsync(buffer, 0, buffer.Length);
             //   await PostData.FlushAsync();
                //Get the response handle, we have no true response yet!
                HttpWebResponse WebResp = (HttpWebResponse) await WebReq.GetResponseAsync();
                //Let's show some information about the response
                //Console.WriteLine(WebResp.StatusCode);
                //Console.WriteLine(WebResp.Server);

                //Now, we read the response (the string), and output it.
                Stream Answer = WebResp.GetResponseStream();
                StreamReader _Answer = new StreamReader(Answer);
                return await _Answer.ReadToEndAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(((WebException) e.InnerException).Status);
                Console.WriteLine(((WebException) e.InnerException).Response);
                throw;
            }
        }


        /// <summary>
        ///  Resolve Domain via listings site or similar example
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
            {
                this.CurrentDomain = match.Groups[1].Value;
            }

            return; // x[1];

            //this.currentDomain = x;
        }

        public string IceUrlWithoutSlash()
        {
            try
            {
                CurrentDomainOrSetDefault();
                return WithOrWithoutTrailingChar(CurrentDomain, '/', trailingDesired: false);
            }
            catch (Exception e)
            {
                console.log("Fatal error returning iceurl withOUT a slash, returned CurrentDomain anyway");
                console.log(e);
                return this.CurrentDomain;
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
                return WithOrWithoutTrailingChar(CurrentDomain, '/', trailingDesired: true);
            }
            catch (Exception e)
            {
                console.log("Fatal error returning iceurl with a slash, returned CurrentDomain anyway");
                console.log(e);
                return this.CurrentDomain;
            }
        }
        
    }
}