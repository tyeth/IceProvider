using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IceProvider;

namespace WebApiCoreApplication.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly IceService _ice;

        public EpisodeController(IceService ice)
            :
            base()
        {
            _ice = ice; // Allowed to set private / readonly only in constructor
            if (_ice.GetIceUrlStatus() != IceUrlStateEnum.Updated) _ice.GetLatestIceUrl(ForceUpdate: true);
        }


        [HttpGet("Episode/Get")]
        public async Task<IActionResult> Get([FromQuery]string id)
        {
            await _ice.GetEpisodesFromUrl(_ice.IceUrlWithSlash() + "tv/series/" + id);
            return  (Json(_ice.Results));
        }

//        // POST api/values
//        [HttpPost]
//        public void Post([FromBody] string value)
//        {
//            if(!IsValidIceUrl(value)) throw new ArgumentException(string.Format("Unfortunately the url {0} could not be set.",value));
//            _ice.CurrentDomain = value;
//        }
    }
}