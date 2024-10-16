namespace HandmadeProductManagement.Core.Common;

public static class Constants
{
    #region User Status

    public const string UserActiveStatus = "Active";
    public const string UserInactiveStatus = "Inactive";

    #endregion

    #region Order Status

    public const string OrderStatusPending = "Pending";
    public const string OrderStatusAwaitingPayment = "Awaiting Payment";
    public const string OrderStatusProcessing = "Processing";
    public const string OrderStatusDelivering = "Delivering";
    public const string OrderStatusShipped = "Shipped";
    public const string OrderStatusDeliveryFailed = "Delivery Failed";
    public const string OrderStatusOnHold = "On Hold";
    public const string OrderStatusDeliveringRetry = "Delivering Retry";
    public const string OrderStatusRefundRequested = "Refund Requested";
    public const string OrderStatusRefundDenied = "Refund Denied";
    public const string OrderStatusRefundApprove = "Refund Approve";
    public const string OrderStatusReturning = "Returning";
    public const string OrderStatusReturnFailed = "Return Failed";
    public const string OrderStatusReturned = "Returned";
    public const string OrderStatusRefunded = "Refunded";
    public const string OrderStatusCanceled = "Canceled";
    public const string OrderStatusClosed = "Closed";

    #endregion

    #region Promotion Status

    public const string PromotionStatusActive = "Active";
    public const string PromotionStatusInactive = "Inactive";

    #endregion

    #region Payment Status

    public const string PaymentStatusPending = "Pending";
    public const string PaymentStatusCompleted = "Completed";
    public const string PaymentStatusExpired = "Expired";
    public const string PaymentStatusRefunded = "Refunded";

    #endregion

    #region Product Status

    public const string ProductStatusAvailable = "Available";
    public const string ProductStatusUnavailable = "Unavailable";

    #endregion

    #region Role Constants

    public const string RoleAdmin = "Admin";
    public const string RoleSeller = "Seller";

    #endregion

    #region Error Codes

    public const string ErrorCodeEmptyOrderId = "empty_order_id";
    public const string ErrorCodeInvalidOrderIdFormat = "invalid_order_id_format";
    public const string ErrorCodeOrderNotFound = "order_not_found";
    public const string ErrorCodeForbiddenAccess = "forbidden";
    public const string ErrorCodeOrderDetailsNotFound = "order_details_not_found";
    public const string ErrorCodeEmptyCart = "empty_cart";
    public const string ErrorCodeProductItemNotFound = "product_item_not_found";
    public const string ErrorCodeProductNotFound = "product_not_found";
    public const string ErrorCodeInsufficientStock = "insufficient_stock";
    public const string ErrorCodeInvalidOrderId = "invalid_order_id";
    public const string ErrorCodeForbidden = "forbidden";
    public const string ErrorCodeInvalidOrderStatus = "invalid_order_status";
    public const string ErrorCodeInvalidAddressFormat = "invalid_address_format";
    public const string ErrorCodeInvalidCustomerNameFormat = "invalid_customer_name_format";
    public const string ErrorCodeInvalidPhoneFormat = "invalid_phone_format";
    public const string ErrorCodeUserNotFound = "user_not_found";
    public const string ErrorCodeNoOrdersFound = "no_orders_found";
    public const string ErrorCodeInvalidStatus = "invalid_status";
    public const string ErrorCodeInvalidInput = "invalid_input";
    public const string ErrorCodeOrderClosed = "order_closed";
    public const string ErrorCodeInvalidStatusTransition = "invalid_status_transition";
    public const string ErrorCodeValidationFailed = "validation_failed";
    public const string ErrorCodeCancelReasonNotFound = "cancel_reason_not_found";
    public const string ErrorCodeShopNotFound = "shop_not_found";
    public const string ErrorCodeInvalidAddress = "invalid_address";
    public const string ErrorCodeInvalidCustomerName = "invalid_customer_name";
    public const string ErrorCodeInvalidPhone = "invalid_phone";
    public const string ErrorCodeCategoryNotFound = "category_not_found";

    #endregion

    #region Error Messages

    public const string ForbiddenAccessMessage = "You have no permission to access this order.";
    public const string OrderNotFoundMessage = "Order not found.";
    public const string ErrorMessageEmptyOrderId = "Order ID is required.";
    public const string ErrorMessageInvalidOrderIdFormat = "Order ID format is invalid.";
    public const string ErrorMessageOrderNotFound = "Order not found.";
    public const string ErrorMessageForbiddenAccess = "You have no permission to access this order.";
    public const string ErrorMessageOrderDetailsNotFound = "No order details found for this order.";
    public const string ErrorMessageEmptyCart = "Cart is empty.";
    public const string ErrorMessageProductItemNotFound = "Product Item not found.";
    public const string ErrorMessageProductNotFound = "Product for Item not found.";
    public const string ErrorMessageInsufficientStock = "Product has insufficient stock.";
    public const string ErrorMessageNoPermission = "You have no permission to access this resource.";
    public const string ErrorMessageInvalidOrderStatus = "Order is processing, cannot update.";
    public const string ErrorMessageInvalidAddressFormat = "Address cannot contain special characters except commas and periods.";
    public const string ErrorMessageInvalidCustomerNameFormat = "Customer name can only contain letters and spaces.";
    public const string ErrorMessageInvalidPhoneFormat = "Phone number must be numeric, start with 0, and be 10 or 11 digits long.";
    public const string ErrorMessageUserNotFound = "User not found.";
    public const string ErrorMessageNoOrdersFound = "There are no orders for this user.";
    public const string ErrorMessageInvalidStatus = "Status cannot be null or empty.";
    public const string ErrorMessageInvalidCancelReason = "CancelReasonId must be null when status is not Canceled.";
    public const string ErrorMessageOrderClosed = "Order was closed.";
    public const string ErrorMessageInvalidStatusTransition = "Cannot transition from {0} to {1}.";
    public const string ErrorMessageCancelReasonRequired = "CancelReasonId is required when updating status to Canceled.";
    public const string ErrorMessageShopNotFound = "Shop not found for the provided user.";
    public const string ErrorMessageInvalidAddress = "Address cannot be null or empty.";
    public const string ErrorMessageInvalidCustomerName = "Customer name cannot be null or empty.";
    public const string ErrorMessageInvalidPhone = "Phone number cannot be null or empty.";
    public const string ErrorMessageCategoryNotFound = "Category not found.";

    #endregion


}