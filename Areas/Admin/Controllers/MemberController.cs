using System.Linq;
using System.Web.Mvc;
using WBookStore.Models.Entities;
using WBookStore.Models.ModelViews;

namespace WBookStore.Areas.Admin.Controllers
{
    public class MemberController : Controller
    {
        private WebBookStoreEntities db = new WebBookStoreEntities();

        // GET: Admin/Member
        public ActionResult Index(string search = "", int page = 1, int pageSize = 2)
        {
            var query = db.tbl_member.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.C_name.Contains(search) || m.C_email.Contains(search));
            }

            var total = query.Count();
            var members = query
                .OrderByDescending(m => m.C_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)total / pageSize);
            ViewBag.Search = search;
            ViewBag.Members = members;
            ViewBag.StartIndex = (page - 1) * pageSize + 1;

            return View();
        }
    }
}
