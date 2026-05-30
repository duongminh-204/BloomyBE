using Bloomy.Models;
using Bloomy.Models.Enums;

namespace Bloomy.Data.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, bool includeDetails = false);
        Task<Order?> GetByIdForCustomerAsync(Guid id, Guid customerId);
        Task<Order?> GetByIdForShopAsync(Guid id, Guid shopId, bool includeDetails = false);
        Task<List<Order>> GetByCustomerIdAsync(Guid customerId);
        Task<List<Order>> GetPendingForShopAsync(Guid shopId);
        Task<List<Order>> GetManagedOrdersAsync(Guid shopId);
        Task<List<Order>> GetUpcomingSetupsAsync(Guid shopId, int days = 14);
        Task<List<Order>> GetCalendarOrdersAsync(Guid shopId, DateTime from, DateTime to);
        Task<int> CountActiveOrdersOnDateAsync(Guid shopId, DateTime date, Guid? excludeOrderId = null);
        Task<List<Order>> GetOrdersOnDateAsync(Guid shopId, DateTime date, Guid? excludeOrderId = null);
        Task<Shop?> GetShopAsync(Guid shopId);
        Task<Concept?> GetConceptAsync(Guid conceptId);
        Task<EventType?> GetEventTypeByIdAsync(int eventTypeId);
        Task<EventType?> GetDefaultEventTypeAsync();
        Task<Concept> CreateConceptAsync(Concept concept);
        Task<List<Concept>> GetConceptsByCustomerAsync(Guid customerId);
        Task<List<Concept>> GetConceptsByShopAsync(Guid shopId);
        Task DeleteConceptAsync(Guid conceptId);
        Task AddOrderAsync(Order order);
        Task AddStatusHistoryAsync(OrderStatusHistory history);
        Task AddPaymentAsync(Payment payment);
        Task<Payment?> GetPaymentAsync(Guid paymentId);
        Task AddReviewAsync(Review review);
        Task<List<Order>> GetOrdersWithPendingPaymentsAsync(Guid shopId);
        Task SaveChangesAsync();
    }
}
