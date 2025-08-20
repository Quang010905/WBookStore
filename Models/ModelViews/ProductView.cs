using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBookStore.Models.ModelViews
{
    public class ProductView
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public int Active { get; set; } = 0;
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public decimal Price { get; set; } = 0;
        public int Quantity {  get; set; } = 0;
        public int Type { get; set; } = 0;  
        public int category_id { get; set; } = 0;
    }
}