﻿using System;
using System.Net.Http;
using HtmlAgilityPack;
namespace IceProvider
{
    public static class IceUtils
    {
        public static bool IsValidIceUrl(string avalue)
        {
            try
            {
                string value = avalue.ToLowerInvariant();
                return (value.Substring(0, 4) == "http" && value.Contains("ice"));
            }
            catch
            {
                return false;
            }
        }

        public static string GetCleanHtml(string html )
        {
             var doc = new HtmlAgilityPack.HtmlDocument();
             doc.LoadHtml(html);
            // Use if you want to convert HTML entities to their literal view
              return HtmlAgilityPack.HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);

            //  if you want to keep HTML entities
            return doc.DocumentNode.InnerText;
        }


        public static string StripToDomainAndPort(string str) {
           
            var st = str;
            var pos = st.IndexOf("//",StringComparison.OrdinalIgnoreCase);
            if (pos >= 0)
            {
                st = st.Substring(pos + 2);
            }
            else throw new ArgumentException(string.Format("Unfortunately the url {0} could not be stripped.", str));
            pos = -1;
            pos = st.IndexOf("/",StringComparison.OrdinalIgnoreCase);
            if (pos >= 0) {  st = st.Substring(0, pos);  }
            return st;
        }
        
        
        public static string WithOrWithoutTrailingChar(string input, char totest, bool trailingDesired = true)
        {
            try
            {
                switch (trailingDesired)
                {
                    case true:
                        return input[input.Length - 1] == totest
                            ? input
                            : input + totest;
                        break;

                    case false:
                        return input[input.Length - 1] == totest
                            ? input.Substring(0, input.Length - 1)
                            : input;

                        break;


                    default:
                        throw new Exception("fatal");
                        break;
                }

            }
            catch (ArgumentOutOfRangeException ae)
            {
                console.log(ae + " aint long enough");
                return input;
            }
            catch (Exception e)
            {
                console.log("Fatal error returning input with" + (trailingDesired ? "" : "out") + " a " + totest + ", returned input anyway");
                console.log(e);
                return input;
            }
        }
    }
}