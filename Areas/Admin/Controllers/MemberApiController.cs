using System;
using System.Linq;
using System.Web.Http;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Api
{
    [RoutePrefix("api/member")]
    public class MemberApiController : ApiController
    {
        private WebBookStoreEntities db = new WebBookStoreEntities();

        // GET api/member
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var members = db.tbl_member
                .OrderByDescending(m => m.C_id)
                .Select(m => new MemberView
                {
                    Id = m.C_id,
                    Name = m.C_name,
                    Email = m.C_email,
                    Address = m.C_address,
                    Phone = m.C_phone,
                    Role = m.C_role ?? 0,
                    Active = m.C_active ?? 0
                }).ToList();

            return Ok(members);
        }

        // GET api/member/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var member = db.tbl_member
                .Where(m => m.C_id == id)
                .Select(m => new MemberView
                {
                    Id = m.C_id,
                    Name = m.C_name,
                    Email = m.C_email,
                    Address = m.C_address,
                    Phone = m.C_phone,
                    Role = m.C_role ?? 0,
                    Active = m.C_active ?? 0
                }).FirstOrDefault();

            if (member == null)
                return NotFound();

            return Ok(member);
        }

        // PUT api/member/toggle/{id}
        [HttpPut]
        [Route("toggle/{id:int}")]
        public IHttpActionResult ToggleActive(int id)
        {
            var member = db.tbl_member.Find(id);
            if (member == null) return NotFound();

            member.C_active = (member.C_active == 1 ? 0 : 1);
            db.SaveChanges();

            return Ok(new
            {
                message = member.C_active == 1 ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản",
                active = member.C_active
            });
        }
    }
}
