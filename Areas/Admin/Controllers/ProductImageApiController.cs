using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web.Http;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Controllers
{
    public class ProductImageApiController : ApiController
    {
        [Route("myapi/productImages/byProId/{id}")]
        [HttpGet]
        public List<ProductImageView> GetProImageByProId(int id)
        {
            var db = new WebBookStoreEntities();
            try
            {
                var ls = db.tbl_product_image.OrderBy(x=> x.C_index).Where(x => x.C_product_id == id).Select(x => new ProductImageView
                {
                    Id = x.C_id,
                    Image = x.C_image,
                    Index = x.C_index ?? 0,
                    proId = x.C_product_id ?? 0
                }).ToList();
                return ls;
            }
            catch (Exception)
            {

                throw;
            }
        }
        [Route("myapi/productImages/Add")]
        [HttpPost]
        public IHttpActionResult Add(ProductImageView model)
        {
            var db = new WebBookStoreEntities();
            try
            {
                db.tbl_product_image.Add(new tbl_product_image
                {
                    C_id = model.Id,
                    C_image = model.Image,
                    C_index = model.Index,
                    C_product_id = model.proId
                });
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [Route("myapi/productImages/{id}")]
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var db = new WebBookStoreEntities();
            try
            {
                var info = db.tbl_product_image.Find(id);
                if (info == null)
                {
                    return NotFound();
                }
                db.tbl_product_image.Remove(info);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [Route("myapi/productImages/{id}")]
        [HttpPost]
        public IHttpActionResult Update(int id, ProductImageView model)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var db = new WebBookStoreEntities();
            try
            {
                var item = db.tbl_product_image.Find(id);
                if (item == null)
                {
                    return NotFound();
                }
                item.C_image = model.Image;
                item.C_index = model.Index;
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [Route("myapi/productImages/{id}")]
        [HttpGet]
        public IHttpActionResult GetById(int id)
        {
            var db = new WebBookStoreEntities();
            try
            {
                var proImage = db.tbl_product_image.Where(x => x.C_id == id).Select(x => new ProductImageView
                {
                    Id = x.C_id,
                    Image = x.C_image,
                    proId = x.C_product_id ?? 0,
                    Index = x.C_index ??0
                }).FirstOrDefault();
                if(proImage == null)
                {
                    return NotFound();
                }
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [Route("myapi/productImages/checkProImgIndex")]
        [HttpGet]
        public bool CheckProductImgIndex(int proId, int index, int? excludeImgId = null)
        {
            try
            {
                using (var db = new WebBookStoreEntities())
                {
                    return db.tbl_product_image
                             .Where(img => img.C_product_id == proId)
                             .Where(img => !excludeImgId.HasValue || img.C_id != excludeImgId.Value)
                             .Any(img => img.C_index == index);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
