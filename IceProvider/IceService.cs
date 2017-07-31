using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace IceProvider
{
    
    public class IceService
    {
        
        
        public Random rand;
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
        public string standardIceFilmsUrl = "https://www.icefilms.info/";
        public bool useStandardReferrer = true;
        public bool useUSERAGENT = true;
        public List<string> IgnoreHostList = new List<string>();// { "fileweed" };
        public string standardUSERAGENT =
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36";
        public string currentDomain = "";


//TODO: implement currentDomain default
        //TODO: take Episode implementation and recreate as API
        //TODO: Add support for IgnoreHostList
        //TODO: expand to support searching and series listing
        
        
        public string IceUrlWithoutSlash() {
            try {
                return this.currentDomain[this.currentDomain.Length - 1] == '/'
                    ?
                    this.currentDomain.Substring(0, this.currentDomain.Length - 1)
                    :
                    this.currentDomain;

            }
            catch (Exception e) {
                console.log("Fatal error returning iceurl withOUT a slash, returned currentDomain anyway");
                console.log(e);
                return this.currentDomain;
            }
        }

        public string IceUrlWithSlash() {
            try {
                return this.currentDomain[this.currentDomain.Length - 1] == '/'
                    ?
                    this.currentDomain
                    :
                    this.currentDomain + '/';

            }
            catch (Exception e) {
                console.log("Fatal error returning iceurl with a slash, returned currentDomain anyway");
                console.log(e);
                return this.currentDomain;
            }
        }
        
       
    }
}
