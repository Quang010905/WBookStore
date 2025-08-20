using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net.Http.Headers;
using WBookStore.Models.ModelViews;
using Newtonsoft.Json;
namespace WBookStore.Areas.User.Controllers
{
    public class HomeController : Controller
    {
        // GET: User/Home
        public async Task<ActionResult> Index()
        {
            var ls = new List<ProductView>();
            try
            {
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync("myapi/products/getNewPro");
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        ls = JsonConvert.DeserializeObject<List<ProductView>>(data);
                    }
                    else
                    {
                        Console.WriteLine("Internal server error");
                    }
                }
                ViewBag.listPro = ls;
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
    }
}