namespace ShopVanPhongPham.Models.Interfaces
{
    public interface IOrderRepository
    {
        Order PlaceOrder(Order order);
        List<Order> GetAllOrders();
        List<Order> GetOrdersByEmail(string email);
        Order? GetOrderById(int id);
    }

}
