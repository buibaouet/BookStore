using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class OrderViewModel : Order
    {
        public string UserName { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
