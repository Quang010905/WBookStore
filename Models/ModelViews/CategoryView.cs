using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBookStore.Models.ModelViews
{
    public class CategoryView
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public int Active { get; set; } = 0;
        public string Image { get; set; } = "";
    }
}