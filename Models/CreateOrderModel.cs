using static FastFoodAPI.Controllers.AccountController;

namespace FastFoodAPI.Models
{
    public class CreateOrderModel
    {
        public List<OrderItemModel> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
