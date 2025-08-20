using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBookStore.Models.ModelViews
{
    public class CartView
    {
        public int Id { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public int Quantity { get; set; } = 0;
        public int MemberId { get; set; } = 0;
        public int ProductId {  get; set; } = 0;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}