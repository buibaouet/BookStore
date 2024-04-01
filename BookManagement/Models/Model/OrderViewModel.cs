using BookManagement.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models.Model
{
    public class OrderViewModel : Order
    {
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
