using Bloomy.DTOs.Orders;

namespace BloomyBE.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateBookingAsync(Guid customerId, CreateBookingDto dto);
        Task<OrderDto?> GetBookingAsync(Guid orderId, Guid customerId);
        Task<List<OrderListItemDto>> GetMyBookingsAsync(Guid customerId);
        Task<OrderDto> TrackBookingAsync(Guid orderId, Guid customerId);
        Task<PaymentDto> CreatePaymentAsync(Guid orderId, Guid customerId, CreatePaymentDto dto);
        Task<OrderDto> RescheduleBookingAsync(Guid orderId, Guid customerId, RescheduleBookingDto dto);
        Task<OrderDto> CancelBookingAsync(Guid orderId, Guid customerId, CancelBookingDto dto);
        Task<OrderDto> SubmitReviewAsync(Guid orderId, Guid customerId, SubmitReviewDto dto);
        Task<ShopOwnerDashboardDto> GetShopOwnerDashboardAsync(Guid shopId);
        Task<List<OrderListItemDto>> GetPendingBookingsAsync(Guid shopId);
        Task<OrderDto> ConfirmBookingAsync(Guid orderId, Guid shopId, Guid userId, ConfirmBookingDto dto);
        Task<OrderDto> UpdateBookingStatusAsync(Guid orderId, Guid shopId, Guid userId, UpdateBookingStatusDto dto);
        Task<List<OrderListItemDto>> GetUpcomingSetupsAsync(Guid shopId);
        Task ApproveReviewAsync(Guid reviewId, Guid shopId, Guid userId, bool approved);
        Task<object> ApproveConceptQuoteAsync(Guid conceptId, Guid customerId, decimal? quotedAmount);
        Task<OrderDto> GetBookingForShopAsync(Guid orderId, Guid shopId);
        Task<PaymentDto> ConfirmPaymentAsync(Guid orderId, Guid paymentId, Guid shopId, Guid userId);
        Task<List<PendingPaymentOrderDto>> GetPendingPaymentConfirmationsAsync(Guid shopId);
        Task<List<OrderListItemDto>> GetManagedBookingsAsync(Guid shopId);
        Task<List<CalendarEventDto>> GetCalendarEventsAsync(Guid shopId, DateTime from, DateTime to);
        Task<OrderDto> UpdateInternalNotesAsync(Guid orderId, Guid shopId, Guid userId, UpdateInternalNotesDto dto);
        Task<OrderDto> ShopOwnerRescheduleAsync(Guid orderId, Guid shopId, Guid userId, ShopOwnerRescheduleDto dto);
        Task<OrderDto> ResolveRescheduleRequestAsync(Guid orderId, Guid shopId, Guid userId, HandleCustomerRequestDto dto);
        Task<OrderDto> ResolveCancelRequestAsync(Guid orderId, Guid shopId, Guid userId, HandleCustomerRequestDto dto);
    }
}
