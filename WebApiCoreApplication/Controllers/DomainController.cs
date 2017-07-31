using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IceProvider;
using static IceProvider.IceUtils;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCoreApplication.Controllers
{
    [Route("api/[controller]")]
    public class DomainController : Controller
    {
        private IceService _ice;
        public DomainController(IceService ice)
        :
            base()
        {
            _ice = ice;
        }
        
        
        // GET api/values
        [HttpGet]
        public string Get()
        {
            return _ice.IceUrlWithSlash();//  ;new string[] {"value1", "value2"};
        }

        // GET api/values/tv/series/123
        [HttpGet("{id}")]
        public string Get(string id)
        {
            return _ice.IceUrlWithSlash() + id;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            if(!IsValidIceUrl(value)) throw new ArgumentException(string.Format("Unfortunately the url {0} could not be set.",value));
            _ice.standardIceFilmsUrl = value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}