using System.Collections.Generic;
using Newtonsoft.Json;

namespace YQL
{
    public class Url
    {
        [JsonProperty(PropertyName="execution-start-time")]
        public string execution_start_time { get; set; }
        [JsonProperty(PropertyName="execution-stop-time")]
        public string execution_stop_time { get; set; }
        [JsonProperty(PropertyName="execution-time")]
        public string execution_time { get; set; }
        public string content { get; set; }
    }

    public class Javascript
    {
        public string execution_start_time { get; set; }
        [JsonProperty(PropertyName="execution-stop-time")]
        public string execution_stop_time { get; set; }
        [JsonProperty(PropertyName="execution-time")]
        public string execution_time { get; set; }
        [JsonProperty(PropertyName="instructions-used")]
        public string instructions_used { get; set; }
        [JsonProperty(PropertyName="table-name")]
        public string table_name { get; set; }
    }

    public class Diagnostics
    {
        public List<Url> url { get; set; }
        public string publiclyCallable { get; set; }
        public List<Javascript> javascript { get; set; }
        [JsonProperty(PropertyName="user-time")]
        public string user_time { get; set; }
        [JsonProperty(PropertyName="service-time")]
        public string service_time { get; set; }
        [JsonProperty(PropertyName="build-version")]
        public string build_version { get; set; }
    }

    public class Results
    {
        public dynamic result { get; set; }
    }

    public class Query
    {
        public int count { get; set; }
        public string created { get; set; }
        public string lang { get; set; }
        public Diagnostics diagnostics { get; set; }
        public Results results { get; set; }
    }

    public class RootObject
    {
        public Query query { get; set; }
    }
}