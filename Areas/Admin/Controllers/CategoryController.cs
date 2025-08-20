using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Controllers
{
    public class CategoryController : Controller
    {
        private readonly string apiBaseUrl = "https://localhost:44326/"; // Đổi url API thật

        private HttpClient GetClient()
        {
            var client = new HttpClient();
            return client;
        }
        public async Task<ActionResult> Index(int page = 1, string search = "")
        {
            var client = GetClient();

            // Gọi API mới cho search
            var apiUrl = string.IsNullOrWhiteSpace(search)
                ? apiBaseUrl
                : $"{apiBaseUrl}/search?keyword={Uri.EscapeDataString(search)}";

            var response = await client.GetAsync(apiUrl);
            var categories = new List<CategoryView>();

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                categories = JsonConvert.DeserializeObject<List<CategoryView>>(jsonString);
            }

            // Phân trang
            int pageSize = 2;
            int totalItem = categories.Count;
            int totalPage = (int)Math.Ceiling((double)totalItem / pageSize);

            var pageList = categories.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Categories = pageList;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPage;
            ViewBag.TotalItems = totalItem;
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.StartIndex = (page - 1) * pageSize + 1;

            return View();
        }


        // GET: Admin/Category/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryView model, HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống";
                return View(model);
            }

            // Kiểm tra trùng tên qua API
            var clientCheck = GetClient();
            var checkResponse = await clientCheck.GetAsync($"{apiBaseUrl}/CheckName?name={Uri.EscapeDataString(model.Name)}");
            if (checkResponse.IsSuccessStatusCode)
            {
                var jsonCheck = await checkResponse.Content.ReadAsStringAsync();
                bool isDuplicate = JsonConvert.DeserializeObject<bool>(jsonCheck);
                if (isDuplicate)
                {
                    TempData["ErrorMessage"] = "Tên danh mục đã tồn tại";
                    return View(model);
                }
            }

            var client = GetClient();
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Name), "C_name");
                content.Add(new StringContent(model.Active.ToString()), "C_active");

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var streamContent = new StreamContent(imageFile.InputStream);
                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "C_image",
                        FileName = imageFile.FileName
                    };
                    content.Add(streamContent, "C_image", imageFile.FileName);
                }

                var response = await client.PostAsync(apiBaseUrl, content);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                TempData["ErrorMessage"] = "Lỗi khi tạo category";
                return View(model);
            }
        }

        // GET: Admin/Category/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var client = GetClient();
            var response = await client.GetAsync($"{apiBaseUrl}/{id}");
            if (!response.IsSuccessStatusCode) return HttpNotFound();

            var jsonString = await response.Content.ReadAsStringAsync();
            var category = JsonConvert.DeserializeObject<CategoryView>(jsonString);
            return View(category);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CategoryView model, HttpPostedFileBase imageFile)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống";
                return View(model);
            }

            // Kiểm tra trùng tên (bỏ qua chính nó)
            var clientCheck = GetClient();
            var checkResponse = await clientCheck.GetAsync($"{apiBaseUrl}/CheckName?id={id}&name={Uri.EscapeDataString(model.Name)}");
            if (checkResponse.IsSuccessStatusCode)
            {
                var jsonCheck = await checkResponse.Content.ReadAsStringAsync();
                bool isDuplicate = JsonConvert.DeserializeObject<bool>(jsonCheck);
                if (isDuplicate)
                {
                    TempData["ErrorMessage"] = "Tên danh mục đã tồn tại";
                    return RedirectToAction("Edit", new { id = id });
                }
            }

            var client = GetClient();
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent(model.Name), "C_name");
                content.Add(new StringContent(model.Active.ToString()), "C_active");

                // Nếu không upload ảnh mới -> gửi lại ảnh cũ
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var streamContent = new StreamContent(imageFile.InputStream);
                    streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "C_image",
                        FileName = imageFile.FileName
                    };
                    content.Add(streamContent, "C_image", imageFile.FileName);
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.Image))
                        content.Add(new StringContent(model.Image), "C_image");
                }

                var response = await client.PutAsync($"{apiBaseUrl}/{id}", content);
                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                TempData["ErrorMessage"] = "Lỗi khi cập nhật category";
                return View(model);
            }
        }



        // POST: Admin/Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var client = GetClient();
            var response = await client.DeleteAsync($"{apiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Xóa thành công";
                return RedirectToAction("Index");
            }
            else
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}