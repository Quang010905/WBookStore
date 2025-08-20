using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.User.Controllers
{
    public class CartApiController : ApiController
    {
        [Route("myapi/carts")]
        [HttpGet]
        public List<CartView> GetAll()
        {
            var db = new WebBookStoreEntities();
            var ls = new List<CartView>();
            try
            {
                ls = db.tbl_cart.Select(x => new CartView
                {
                    Id = x.C_id,
                    Price = x.C_price ?? 0,
                    Quantity = x.C_quantity ?? 0,
                    Date = (DateTime)x.C_date,
                    MemberId = x.C_member_id ?? 0,
                    ProductId = x.C_product_id ?? 0
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
            return ls;
        }
        [Route("myapi/carts/add")]
        [HttpPost]
        public IHttpActionResult Add(CartView value)
        {
            var db = new WebBookStoreEntities();
            try
            {
                var item = new tbl_cart
                {
                    C_id = value.Id,
                    C_quantity = value.Quantity,
                    C_price = value.Price,
                    C_date = (DateTime)value.Date,
                    C_member_id = value.MemberId,
                    C_product_id = value.ProductId
                };
                db.tbl_cart.Add(item);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }
        [Route("myapi/carts/{id}")]
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            var db = new WebBookStoreEntities();
            try
            {
                var info = db.tbl_cart.Find(id);
                if(info == null)
                {
                    return NotFound();
                }
                db.tbl_cart.Remove(info);
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest("error");
            }
        }
        [Route("myapi/carts/{id}")]
        [HttpPost]
        public IHttpActionResult UpdateCartQuantity(int id, CartView value)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var db = new WebBookStoreEntities();
            try
            {
                var item = db.tbl_cart.Find(id);
                if(item == null)
                {
                    return NotFound();
                }
                item.C_id = value.Id;
                item.C_quantity = value.Quantity;
                db.SaveChanges();
                return Ok();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
