using System.Collections.Generic;
using System.Threading.Tasks;
using IceProvider;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCoreApplication.Controllers
{
    public class EpisodeController : Controller
    {
        private readonly IceService _ice;

        public EpisodeController(IceService ice)
        {
            _ice = ice; // Allowed to set private / readonly only in constructor
            if (_ice.GetIceUrlStatus() != IceUrlStateEnum.Updated) _ice.GetLatestIceUrl(true);
        }
        
        [HttpGet("Episode/Get")]
        public async Task<IActionResult> Get([FromQuery] string id)
        {
            await _ice.GetEpisode(id);
            return Json(_ice.Results);
        }

        [HttpGet("Episode/GetSeries")]
        public async Task<IActionResult> GetSeries([FromQuery] string id)
        {
            await _ice.GetEpisodesFromSeriesUrl(_ice.IceUrlWithSlash() + "tv/series/" + id);
            return Json(_ice.Results);
        }

        [HttpGet("Episode/Multiple")]
        public async Task<IActionResult> Multiple([FromQuery] string[] id)
        {   
            var ret = new List<IIceEpisode>();
            foreach (var item in id)
            {
                await _ice.GetEpisode(item);
                _ice.Results.ForEach(x=> { ret.Add(x); }) ;
            }
            return Json(ret);
        }

    }
}