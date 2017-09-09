using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IceProvider;
using static IceProvider.IceUtils;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCoreApplication.Controllers
{
    public class DomainController : Controller
    {
        private readonly IceService _ice;
        public DomainController(IceService ice)
        :
            base()
        {
            _ice = ice;   // Allowed to set private / readonly only in constructor
        }
        
        
        // GET api/values
        [HttpGet]
        public string Get([FromQuery]bool withSlash=true)
        {
            return withSlash ? _ice.IceUrlWithSlash() : _ice.IceUrlWithoutSlash();//  ;new string[] {"value1", "value2"};
        }
        [Route("GetLatest")]
        public string GetLatest([FromQuery]bool withSlash=true)
        {
            _ice.GetLatestIceUrl(ForceUpdate: true);
            return withSlash ? _ice.IceUrlWithSlash() : _ice.IceUrlWithoutSlash();
        }

        // GET api/values/tv/series/123
        [HttpGet("Domain/{id}")]
        public string Get(string id)
        {
            return _ice.IceUrlWithSlash() + id;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            if(!IsValidIceUrl(value)) throw new ArgumentException(string.Format("Unfortunately the url {0} could not be set.",value));
            _ice.CurrentDomain = value;
        }

    }
}