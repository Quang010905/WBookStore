using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Controllers
{
    public class ProductController : Controller
    {
        // GET: Admin/Product
        public async Task<ActionResult> Index(int page = 1, string search = "")
        {
            var ls = new List<ProductView>();
            var lsCate = new List<CategoryView>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44326/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("ApplicationException/Json"));
                string apiUrl = string.IsNullOrEmpty(search) ? "myapi/products"
                    : $"myapi/products/search?keyword={Uri.EscapeDataString(search)}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = response.Content.ReadAsStringAsync().Result;
                    ls = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductView>>(data);
                }
                else
                {
                    Console.WriteLine("Internal server Error");
                }
                HttpResponseMessage cateResponse = await client.GetAsync("myapi/categories");
                if (cateResponse.IsSuccessStatusCode)
                {
                    string cateData = await cateResponse.Content.ReadAsStringAsync();
                    lsCate = JsonConvert.DeserializeObject<List<CategoryView>>(cateData);
                }
            }
            int pageSize = 2;
            var totalItem = ls.Count();
            var totalPage = (int)Math.Ceiling((double)totalItem / pageSize);
            var pageList = ls.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.listProduct = pageList;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPage;
            ViewBag.TotalItems = totalItem;
            ViewBag.pageSize = pageSize;
            ViewBag.StartIndex = (page - 1) * pageSize + 1;
            ViewBag.Search = search;
            ViewBag.Categories = lsCate;
            return View();
        }

        public async Task<ActionResult> Add(HttpPostedFileBase upFile, ProductView model)
        {
            string pathSave = Server.MapPath("~/Content/Image");
            string fileName = "";
            try
            {
                if (upFile != null)
                {
                    string originalFileName = Path.GetFileNameWithoutExtension(upFile.FileName);
                    string extension = Path.GetExtension(upFile.FileName);

                    // Làm sạch tên file, chỉ giữ ký tự chữ, số, dấu gạch ngang và gạch dưới
                    string safeFileName = Regex.Replace(originalFileName, @"[^a-zA-Z0-9_-]", "");

                    // Nếu sau khi lọc mà tên rỗng thì đặt mặc định
                    if (string.IsNullOrEmpty(safeFileName))
                    {
                        safeFileName = "img";
                    }

                    // Đặt tên file mới (thêm ticks để tránh trùng)
                    fileName = $"{DateTime.Now.Ticks}_{safeFileName}{extension}";

                    // Lưu file
                    upFile.SaveAs(Path.Combine(pathSave, fileName));
                }
                else
                {
                    fileName = "noimage.png";
                }
                model.Name = Request.Form["name"];
                model.Description = Request.Form["description"];
                model.Active = Request.Form["active"] != null ? 1 : 0;
                model.Price = int.Parse(Request.Form["price"]);
                model.Quantity = int.Parse(Request.Form["quantity"]);
                model.Type = int.Parse(Request.Form["type"]);
                model.category_id = int.Parse(Request.Form["cateId"]);
                model.Image = fileName;
                //goi api
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    var jsonData = JsonConvert.SerializeObject(model);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage responseProName = await client.GetAsync($"myapi/products/checkProName?Name={model.Name}");
                    if (responseProName.IsSuccessStatusCode)
                    {
                        string result = await responseProName.Content.ReadAsStringAsync();
                        bool exists = JsonConvert.DeserializeObject<bool>(result);

                        if (exists)
                        {
                            TempData["ErrorMessage"] = "Product name already exist!";
                            return RedirectToAction("Index", "Product");
                        }
                    }
                    HttpResponseMessage response = await client.PostAsync("myapi/products", content);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Add product success!";
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        ViewBag.Error = $"Không thể thêm sản phẩm! Mã lỗi: {response.StatusCode}";
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Add()
        {
            var ls = new List<CategoryView>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync("myapi/categories");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        ls = JsonConvert.DeserializeObject<List<CategoryView>>(data);
                    }
                }
                ViewBag.Categories = ls;
            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.DeleteAsync($"myapi/products/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Delete product success!";
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        //TempData["ErrorMessage"] = "Please delete image in this product first";
                        string error = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Update product fail! Status: {response.StatusCode}. Error: {error}";
                        return RedirectToAction("Index", "Product");
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var ls = new List<CategoryView>();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync($"myapi/products/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        string data = response.Content.ReadAsStringAsync().Result;
                        var product = JsonConvert.DeserializeObject<ProductView>(data);
                        ViewBag.itemProduct = product;
                        ViewBag.type = product.Type;
                    }
                    HttpResponseMessage responseCate = await client.GetAsync("myapi/categories");
                    if (responseCate.IsSuccessStatusCode)
                    {
                        string data = await responseCate.Content.ReadAsStringAsync();
                        var cate = JsonConvert.DeserializeObject<List<CategoryView>>(data);
                        ViewBag.Categories = cate;
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
        public async Task<ActionResult> Update(int id, ProductView model, HttpPostedFileBase upFile)
        {
            string pathSave = Server.MapPath("~/Content/Image");
            string fileName = "";
            try
            {
                if (upFile != null)
                {
                    string originalFileName = Path.GetFileNameWithoutExtension(upFile.FileName);
                    string extension = Path.GetExtension(upFile.FileName);

                    // Làm sạch tên file, chỉ giữ ký tự chữ, số, dấu gạch ngang và gạch dưới
                    string safeFileName = Regex.Replace(originalFileName, @"[^a-zA-Z0-9_-]", "");

                    // Nếu sau khi lọc mà tên rỗng thì đặt mặc định
                    if (string.IsNullOrEmpty(safeFileName))
                    {
                        safeFileName = "img";
                    }

                    // Đặt tên file mới (thêm ticks để tránh trùng)
                    fileName = $"{DateTime.Now.Ticks}_{safeFileName}{extension}";

                    // Lưu file
                    upFile.SaveAs(Path.Combine(pathSave, fileName));
                }
                else
                {
                    fileName = Request.Form["OldImage"];
                }
                model.Name = Request.Form["name"];
                model.Description = Request.Form["description"];
                model.Active = Request.Form["active"] != null ? 1 : 0;
                model.Price = decimal.Parse(Request.Form["price"]);
                model.Quantity = int.Parse(Request.Form["quantity"]);
                model.Type = int.Parse(Request.Form["type"]);
                model.category_id = int.Parse(Request.Form["cateId"]);
                model.Image = fileName;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44326/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage responseProName = await client.GetAsync($"myapi/products/checkProName?Name={model.Name}&excludeId={model.Id}");
                    if (responseProName.IsSuccessStatusCode)
                    {
                        string result = await responseProName.Content.ReadAsStringAsync();
                        bool exists = JsonConvert.DeserializeObject<bool>(result);

                        if (exists)
                        {
                            TempData["ErrorMessage"] = "Product name already exist!";
                            return RedirectToAction("Edit", "Product", new {id = model.Id});
                        }
                    }
                    var json = JsonConvert.SerializeObject(model);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync($"myapi/products/{id}", httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Update product success!";
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        TempData["ErrorMessage"] = $"Update product fail! Status: {response.StatusCode}. Error: {error}";
                        return RedirectToAction("Index", "Product");
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