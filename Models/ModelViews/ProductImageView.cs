using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBookStore.Models.ModelViews
{
    public class ProductImageView
    {
        public int Id { get; set; } = 0;
        public string Image { get; set; } = "";
        public int Index { get; set; } = 0;
        public int proId {  get; set; } = 0;
    }
}