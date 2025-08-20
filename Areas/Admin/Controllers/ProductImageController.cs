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
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Ajax.Utilities;
using System.Security.Cryptography;

namespace WBookStore.Areas.Admin.Controllers
{
    public class ProductImageController : Controller
    {
        // GET: Admin/ProductImage
        public async Task<ActionResult> Index(int id, int page = 1)
        {
            try
            {
                var ls = new List<ProductImageView>();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync($"myapi/productImages/byProId/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        string data = response.Content.ReadAsStringAsync().Result;
                        ls = JsonConvert.DeserializeObject<List<ProductImageView>>(data);
                    }
                    else
                    {
                        Console.WriteLine("Error");
                    }
                    var responsePro = await client.GetAsync($"myapi/products/{id}");
                    if (responsePro.IsSuccessStatusCode)
                    {
                        string data = responsePro.Content.ReadAsStringAsync().Result;
                        var pro = JsonConvert.DeserializeObject<ProductView>(data);
                        ViewBag.proName = pro.Name;
                        ViewBag.proId = pro.Id;
                    }
                    else
                    {
                        Console.WriteLine("Error");
                    }
                    var pageSize = 2;
                    var totalItem = ls.Count();
                    var totalPage = (int)Math.Ceiling((double)totalItem/ pageSize);
                    var pageList = ls.Skip((page -1) *pageSize).Take(pageSize).ToList();
                    ViewBag.listProImage = pageList;
                    ViewBag.CurrentPage = page;
                    ViewBag.TotalPages = totalPage;
                    ViewBag.TotalItems = totalItem;
                    ViewBag.pageSize = pageSize;
                    ViewBag.StartIndex = (page - 1) * pageSize + 1;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
        public async Task<ActionResult> Add(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync($"myapi/products/{id}");
                    if (response.IsSuccessStatusCode) 
                    {
                        string data = response.Content?.ReadAsStringAsync().Result;
                        var pro = JsonConvert.DeserializeObject<ProductView>(data);
                        ViewBag.proId = pro.Id;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> AddImg(HttpPostedFileBase upFile, ProductImageView model)
        {
            string pathSave = Server.MapPath("~/Content/Image");
            string fileName = "";
            try
            {
                if (upFile != null)
                {
                    string originalFileName = Path.GetFileNameWithoutExtension(upFile.FileName);
                    string extension = Path.GetExtension(upFile.FileName);
                    string safeFileName = Regex.Replace(originalFileName, @"[^a-zA-Z0-9_-]", "");
                    if (string.IsNullOrEmpty(safeFileName))
                    {
                        safeFileName = "img";
                    }
                    fileName = $"{DateTime.Now.Ticks}_{safeFileName}{extension}";
                    upFile.SaveAs(Path.Combine(pathSave, fileName));
                }
                else
                {
                    fileName = "noimage.png";
                }
                model.proId = int.Parse(Request.Form["proId"]);
                model.Index = int.Parse(Request.Form["Index"]);
                model.Image = fileName;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var jsonData = JsonConvert.SerializeObject(model);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //check index
                    HttpResponseMessage responseProImgIndex = await client.GetAsync(
                        $"myapi/productImages/checkProImgIndex?proId={model.proId}&index={model.Index}&excludeImgId={model.Id}"
                    );

                    if (responseProImgIndex.IsSuccessStatusCode)
                    {
                        string result = await responseProImgIndex.Content.ReadAsStringAsync();
                        bool exists = JsonConvert.DeserializeObject<bool>(result);

                        if (exists)
                        {
                            TempData["ErrorMessage"] = "Index already exist!";
                            return RedirectToAction("Add", "ProductImage", new { id = model.proId });
                        }
                    }
                    HttpResponseMessage response = await client.PostAsync("myapi/productImages/Add", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Add product image success!";
                        return RedirectToAction("Index", "ProductImage", new { id = model.proId});
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Update product fail! Status: {response.StatusCode}. Error: {error}";
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Add", "ProductImage", new {id = model.proId});
        }
        [HttpGet]
        public async Task<ActionResult> Delete(int id, int proId)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.DeleteAsync($"myapi/productImages/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Delete Image Sucess";
                        return RedirectToAction("Index", "ProductImage", new { id = proId});
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Delete Fail";
                        return RedirectToAction("Index","ProductImage", new { id = proId});
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(int id, int proId, ProductImageView model, HttpPostedFileBase upFile)
        {
            string pathSave = Server.MapPath("~/Content/Image");
            string fileName = "";
            try
            {
                if(upFile != null)
                {
                    string originalFileName = Path.GetFileNameWithoutExtension(upFile.FileName);
                    string extension = Path.GetExtension(upFile.FileName);
                    string safeFileName = Regex.Replace(originalFileName, @"[^a-zA-Z0-9_-]", "");
                    if (string.IsNullOrEmpty(safeFileName))
                    {
                        safeFileName = "img";
                    }
                    fileName = $"{DateTime.Now.Ticks}_{safeFileName}{extension}";
                    upFile.SaveAs(Path.Combine(pathSave, fileName));
                }
                else
                {
                    fileName = Request.Form["OldImage"];
                }
                model.Index = int.Parse(Request.Form["Index"]);
                model.Image = fileName;
                using(var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var jsonData = JsonConvert.SerializeObject(model);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    //check index
                    HttpResponseMessage responseProImgIndex = await client.GetAsync(
                        $"myapi/productImages/checkProImgIndex?proId={proId}&index={model.Index}&excludeImgId={id}"
                    );

                    if (responseProImgIndex.IsSuccessStatusCode)
                    {
                        string result = await responseProImgIndex.Content.ReadAsStringAsync();
                        bool exists = JsonConvert.DeserializeObject<bool>(result);

                        if (exists)
                        {
                            TempData["ErrorMessage"] = "Index already exist!";
                            return RedirectToAction("Index", "ProductImage", new { id = proId });
                        }
                    }

                    HttpResponseMessage response = await client.PostAsync($"myapi/productImages/{id}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Update Success !";
                        return RedirectToAction("Index", "ProductImage", new {id = proId});
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Update product fail! Status: {response.StatusCode}. Error: {error}";
                        return RedirectToAction("Index", "ProductImage", new { id = proId });
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}