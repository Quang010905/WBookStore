using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Controllers
{
    public class CategoryApiController : ApiController
    {
        [System.Web.Http.Route("myapi/categories")]
        [System.Web.Http.HttpGet]
        public List<CategoryView> GetActiveCate()
        {
            var ls = new List<CategoryView>();
            try
            {
                var db = new WebBookStoreEntities();
                ls = db.tbl_category.Where(x => x.C_active == 1).OrderByDescending(x => x.C_id).Select(x => new CategoryView
                {
                    Id = x.C_id,
                    Name = x.C_name,
                    Active = x.C_active ?? 0,
                    Image = x.C_image,
                }).ToList();
                return ls;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private WebBookStoreEntities db = new WebBookStoreEntities();

        // GET api/category
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var categories = db.tbl_category.OrderByDescending(C => C.C_id).Select(c => new CategoryView
            {
                Id = c.C_id,
                Name = c.C_name,
                Active = c.C_active ?? 0,
                Image = c.C_image
            }).ToList();


            return Ok(categories);
        }


        // GET api/category/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var category = db.tbl_category
                .Where(c => c.C_id == id)
                .Select(c => new CategoryView
                {
                    Id = c.C_id,
                    Name = c.C_name,
                    Active = c.C_active ?? 0,
                    Image = c.C_image
                })
                .FirstOrDefault();

            if (category == null)
                return NotFound();

            return Ok(category);
        }



        // POST api/category
        // Thêm mới category với upload ảnh
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> Create()
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type");

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            string name = null;
            int active = 1;
            string savedFileName = null;

            foreach (var content in provider.Contents)
            {
                var contentDisposition = content.Headers.ContentDisposition;
                if (contentDisposition != null)
                {
                    var fieldName = contentDisposition.Name?.Trim('\"');

                    if (fieldName == "C_name")
                    {
                        name = (await content.ReadAsStringAsync())?.Trim();
                    }
                    else if (fieldName == "C_active")
                    {
                        var activeStr = await content.ReadAsStringAsync();
                        int.TryParse(activeStr, out active);
                    }
                    else if (fieldName == "C_image")
                    {
                        var fileName = contentDisposition.FileName?.Trim('\"');
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var buffer = await content.ReadAsByteArrayAsync();
                            var folder = HttpContext.Current.Server.MapPath("~/Content/Images/");
                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);

                            var extension = Path.GetExtension(fileName);
                            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                            var filePath = Path.Combine(folder, uniqueFileName);
                            File.WriteAllBytes(filePath, buffer);

                            savedFileName = uniqueFileName;
                        }
                    }
                }
            }

            // Nếu không upload ảnh → dùng ảnh mặc định
            if (string.IsNullOrEmpty(savedFileName))
            {
                savedFileName = "noimage.png";
            }

            // Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Tên danh mục không được để trống");

            // Kiểm tra trùng tên
            var exists = db.tbl_category.Any(c => c.C_name.ToLower() == name.ToLower());
            if (exists)
                return BadRequest("Tên danh mục đã tồn tại");

            var category = new tbl_category
            {
                C_name = name,
                C_active = active,
                C_image = savedFileName
            };

            db.tbl_category.Add(category);
            await db.SaveChangesAsync();

            return Ok(new { message = "Thêm danh mục thành công", categoryId = category.C_id });
        }


        // PUT api/category/{id}
        [HttpPut]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> Update(int id)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return BadRequest("Unsupported media type");

            var category = db.tbl_category.Find(id);
            if (category == null) return NotFound();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            string oldName = category.C_name; // tên trước khi sửa
            string newName = category.C_name;
            bool hasNewImage = false;

            foreach (var content in provider.Contents)
            {
                var contentDisposition = content.Headers.ContentDisposition;
                if (contentDisposition != null)
                {
                    var fieldName = contentDisposition.Name?.Trim('\"');

                    if (fieldName == "C_name")
                    {
                        newName = (await content.ReadAsStringAsync())?.Trim();
                    }
                    else if (fieldName == "C_active")
                    {
                        var activeStr = await content.ReadAsStringAsync();
                        if (int.TryParse(activeStr, out int active))
                            category.C_active = active;
                    }
                    else if (fieldName == "C_image")
                    {
                        var fileName = contentDisposition.FileName?.Trim('\"');
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var buffer = await content.ReadAsByteArrayAsync();
                            var folder = HttpContext.Current.Server.MapPath("~/Content/Images/");
                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);

                            var extension = Path.GetExtension(fileName);
                            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                            var filePath = Path.Combine(folder, uniqueFileName);
                            File.WriteAllBytes(filePath, buffer);

                            // Xóa ảnh cũ nếu không phải ảnh mặc định
                            if (!string.IsNullOrEmpty(category.C_image) && category.C_image != "noimage.png")
                            {
                                var oldPath = HttpContext.Current.Server.MapPath("~/Content/Images/" + category.C_image);
                                if (File.Exists(oldPath))
                                    File.Delete(oldPath);
                            }

                            category.C_image = uniqueFileName;
                            hasNewImage = true;
                        }
                    }
                }
            }

            // Nếu không upload ảnh mới và ảnh cũ đang null/rỗng → set ảnh mặc định
            if (!hasNewImage && string.IsNullOrEmpty(category.C_image))
            {
                category.C_image = "noimage.png";
            }

            // Nếu tên mới rỗng → giữ tên cũ
            if (string.IsNullOrWhiteSpace(newName))
            {
                newName = oldName;
            }
            else
            {
                // Nếu trùng với tên danh mục khác → trả lại tên cũ
                var exists = db.tbl_category.Any(c => c.C_name.ToLower() == newName.ToLower() && c.C_id != id);
                if (exists)
                {
                    newName = oldName;
                }
            }

            category.C_name = newName;

            await db.SaveChangesAsync();
            return Ok(new
            {
                message = "Cập nhật danh mục thành công",
                currentName = category.C_name // trả về tên hiện tại (có thể là tên cũ nếu trùng)
            });
        }




        [HttpGet]
        [Route("CheckName")]
        public IHttpActionResult CheckName(string name, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Ok(false);

            name = name.Trim().ToLower();

            bool exists;
            if (id.HasValue)
            {
                // Khi edit: bỏ qua chính nó
                exists = db.tbl_category.Any(c => c.C_name.ToLower() == name && c.C_id != id.Value);
            }
            else
            {
                exists = db.tbl_category.Any(c => c.C_name.ToLower() == name);
            }

            return Ok(exists);
        }



        // DELETE api/category/{id}
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var category = db.tbl_category.Find(id);
            if (category == null)
                return NotFound();

            // Xóa ảnh cũ nếu có
            if (!string.IsNullOrEmpty(category.C_image))
            {
                try
                {
                    var oldPath = HttpContext.Current.Server.MapPath(category.C_image);
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }
                catch (Exception ex)
                {
                    // Ghi log nếu cần
                    System.Diagnostics.Debug.WriteLine("Không thể xóa ảnh: " + ex.Message);
                }
            }

            // Xóa record trong DB
            db.tbl_category.Remove(category);
            db.SaveChanges();

            return Ok(new { message = "Category deleted" });
        }



        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("search")]
        public List<CategoryView> SearchCategory(string keyword)
        {
            using (var db = new WebBookStoreEntities())
            {
                var normalizedSearch = NormalizeSearch(keyword);

                var categories = db.tbl_category
                    .Select(x => new CategoryView
                    {
                        Id = x.C_id,
                        Name = x.C_name,
                        Active = x.C_active ?? 0,
                        Image = x.C_image
                    })
                    .ToList();

                if (string.IsNullOrEmpty(normalizedSearch))
                    return categories;

                return categories
                    .Where(c => NormalizeSearch(c.Name).Contains(normalizedSearch))
                    .ToList();
            }
        }

        private string NormalizeSearch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            string lower = input.ToLowerInvariant();
            string normalized = lower.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            // Loại bỏ khoảng trắng
            return new string(sb.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }
}




