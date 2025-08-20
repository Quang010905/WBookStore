using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Controllers
{
    public class ProductApiController : ApiController
    {
        [System.Web.Http.Route("myapi/products")]
        [System.Web.Http.HttpGet]
        public List<ProductView> GetAll()
        {
            var ls = new List<ProductView>();
            try
            {
                var db = new WebBookStoreEntities();
                ls = db.tbl_product.Select(
                    x => new ProductView
                    {
                        Id = x.C_id,
                        Name = x.C_name,
                        Active = x.C_active ?? 0,
                        Description = x.C_description,
                        Image = x.C_image,
                        Price = x.C_price ?? 0,
                        Quantity = x.C_quantity ?? 0,
                        Type = x.C_type ?? 0,
                        category_id = x.C_category_id ?? 0
                    }).ToList();
                return ls;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [System.Web.Http.Route("myapi/products/search")]
        [System.Web.Http.HttpGet]
        public List<ProductView> SearchProduct(string keyword)
        {
            var db = new WebBookStoreEntities();
            var normalizedSearch = NormalizeSearch(keyword);
            var products = db.tbl_product.Select(
                x => new ProductView
                {
                    Id = x.C_id,
                    Name = x.C_name,
                    Active = x.C_active ?? 0,
                    Description = x.C_description,
                    Image = x.C_image,
                    Price = x.C_price ?? 0,
                    Quantity = x.C_quantity ?? 0,
                    Type = x.C_type ?? 0,
                    category_id = x.C_category_id ?? 0
                }
            ).ToList();
            return products.Where(p => NormalizeSearch(p.Name).Contains(normalizedSearch)).ToList();
        }

        private string NormalizeSearch(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "";
            }
            string lower = input.ToLowerInvariant();
            string normalized = lower.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in normalized)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return new string(sb.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
        [Route("myapi/products")]
        [HttpPost]
        public IHttpActionResult Add(ProductView value)
        {
            var db = new WebBookStoreEntities();
            db.tbl_product.Add(new tbl_product
            {
                C_name = value.Name,
                C_active = value.Active,
                C_description = value.Description,
                C_image = value.Image,
                C_price = value.Price,
                C_quantity = value.Quantity,
                C_type = value.Type,
                C_category_id = value.category_id
            });
            db.SaveChanges();
            return Ok();
        }

        [Route("myapi/products/{id}")]
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var db = new WebBookStoreEntities();
            tbl_product info = db.tbl_product.Find(id);
            if (info == null)
            {
                return NotFound();
            }
            try
            {
                db.tbl_product.Remove(info);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {

                return BadRequest("error");
            }
        }
        [Route("myapi/products/{id}")]
        [HttpPost]
        public IHttpActionResult Update(int id, ProductView value)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var db = new WebBookStoreEntities();
            try
            {
                var item = db.tbl_product.Find(id);
                if (item == null)
                {
                    return NotFound();
                }
                item.C_name = value.Name;
                item.C_active = value.Active;
                item.C_description = value.Description;
                item.C_image = value.Image;
                item.C_price = value.Price;
                item.C_quantity = value.Quantity;
                item.C_type = value.Type;
                item.C_category_id = value.category_id;
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }
        // GET api/products/{id}
        [HttpGet]
        [Route("myapi/products/{id}")]
        public IHttpActionResult GetById(int id)
        {
            var db = new WebBookStoreEntities();
            var category = db.tbl_product
                .Where(c => c.C_id == id)
                .Select(c => new ProductView
                {
                    Id = c.C_id,
                    Name = c.C_name,
                    Active = c.C_active ?? 0,
                    Image = c.C_image,
                    Description = c.C_description,
                    Price = c.C_price ?? 0,
                    Quantity = c.C_quantity ?? 0,
                    Type = c.C_type ?? 0,
                    category_id = c.C_category_id ?? 0
                })
                .FirstOrDefault();

            if (category == null)
                return NotFound();

            return Ok(category);
        }
        [Route("myapi/products/checkProName")]
        [HttpGet]
        public bool checkProName(string Name, int? excludeId = null)
        {
            try
            {
                using (var db = new WebBookStoreEntities())
                {
                    string normalizedInput = Helper.StringHelper.NormalizeName(Name);
                    var allProNames = db.tbl_product.Where(c => !excludeId.HasValue || c.C_id != excludeId.Value)
                                              .Select(c => c.C_name)
                                              .ToList();
                    // 2. So sánh sau khi normalize từng cái
                    return allProNames.Any(dbName => Helper.StringHelper.NormalizeName(dbName) == normalizedInput);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [Route("myapi/products/getNewPro")]
        [HttpGet]
        public List<ProductView> getNewPro()
        {
            var ls = new List<ProductView>();
            var db = new WebBookStoreEntities();
            try
            {
                ls = db.tbl_product.Where(x => x.C_active == 1).Select(x => new ProductView
                {
                    Id = x.C_id,
                    Name = x.C_name,
                    Active = x.C_active ?? 0,
                    Description = x.C_description,
                    Image = x.C_image,
                    Price = x.C_price ?? 0,
                    Quantity = x.C_quantity ?? 0,
                    Type = x.C_type ?? 0,
                    category_id = x.C_category_id ?? 0

                }).Take(8).ToList();
            }
            catch (Exception)
            {

                throw;
            }
            return ls;
        }
    }
}
