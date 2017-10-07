using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCoreApplication.Controllers
{
    public class HomeController : Controller
    {
        private object model;
        // GET
        [Route("/")]
        public IActionResult Index(object model)
        {
            try
            {

            return
            View("Index");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}