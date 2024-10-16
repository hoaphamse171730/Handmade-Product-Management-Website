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
    public const string OrderStatusPaymentFailed = "Payment Failed";
    public const string OrderStatusCanceled = "Canceled";
    public const string OrderStatusClosed = "Closed";

    #endregion

    #region Promotion Status

    public const string PromotionStatusActive = "Active";
    public const string PromotionStatusInactive = "Inactive";

    #endregion

    #region Category Status

    public const string CategoryStatusActive = "Active";
    public const string CategoryStatusInactive = "Inactive";

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
    public const string RoleCustomer = "Customer";

    #endregion

    #region Error Messages

    public const string ErrorMessageForbidden = "You have no permission to access this order.";
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
    public const string ErrorMessageCannotTransition = "Cannot transition from {0} to {1}.";
    public const string ErrorMessageNoOrdersFound = "There are no orders for this user.";
    public const string ErrorMessageInvalidStatus = "Status cannot be null or empty.";
    public const string ErrorMessageInvalidCancelReason = "CancelReasonId must be null when status is not Canceled.";
    public const string ErrorMessageOrderClosed = "Order was closed.";
    public const string ErrorMessageCancelReasonRequired = "CancelReasonId is required when updating status to Canceled.";
    public const string ErrorMessageShopNotFound = "Shop not found for the provided user.";
    public const string ErrorMessageInvalidAddress = "Address cannot be null or empty.";
    public const string ErrorMessageInvalidCustomerName = "Customer name cannot be null or empty.";
    public const string ErrorMessageInvalidPhone = "Phone number cannot be null or empty.";
    public const string ErrorMessageCategoryNotFound = "Category not found.";
    public const string ErrorMessageMissingLoginIdentifier = "At least one of Phone Number, Email, or Username is required for login.";
    public const string ErrorMessageInvalidEmailFormat = "Invalid Email format.";
    public const string ErrorMessageInvalidUsernameFormat = "Invalid Username format. Special characters are not allowed.";
    public const string ErrorMessageAccountDisabled = "This account has been disabled.";
    public const string ErrorMessageUnauthorized = "Incorrect user login credentials";
    public const string ErrorMessageIncorrectPassword = "Incorrect password";
    public const string ErrorMessageUsernameTaken = "Username is already taken.";
    public const string ErrorMessageEmailTaken = "Email is already taken.";
    public const string ErrorMessagePhoneTaken = "Phone number is already taken.";
    public const string ErrorMessageUserCreationFailed = "User creation failed: ";
    public const string ErrorMessageInvalidFullnameFormat = "Full Name contains invalid characters.";
    public const string ErrorMessageWeakPassword = "Password is too weak. It must be at least 8 characters long, contain uppercase, lowercase, a special character, and a digit.";
    public const string ErrorMessageInvalidEmail = "Email is invalid or not confirmed.";
    public const string MessagePasswordResetLinkSent = "Password reset link has been sent to your email.";
    public const string MessagePasswordResetSuccess = "Password has been reset successfully.";
    public const string ErrorMessageResetPasswordError = "Error resetting the password.";
    public const string MessageEmailConfirmedSuccess = "Email confirmed successfully.";
    public const string ErrorMessageEmailConfirmationError = "Error confirming the email";
    public const string ErrorMessageInvalidToken = "Invalid token.";
    public const string ErrorMessageMissingClaims = "Token is missing necessary claims.";
    public const string UserNotFoundErrorMessage = "User not found.";
    public const string RoleAssignmentFailedErrorMessage = "Failed to add role to user";
    public const string ErrorMessageRoleAssignmentFailed = "Failed to assign role to the user";
    public const string ErrorMessageValidationFailed = "Validation failed.";
    public const string ErrorMessageInvalidGuidFormat = "ID is not in a valid GUID format.";
    public const string ErrorMessageCancelReasonNotFound = "Cancel Reason not found.";
    public const string ErrorMessageCannotAddOwnProduct = "You cannot add your own product to the cart.";
    public const string ErrorMessageInsufficientStockForUpdate = "Not enough quantity in stock for the updated cart item.";
    public const string ErrorMessageCartItemNotFound = "Cart item not found.";
    public const string ErrorMessageCategoryNameExists = "Category name already exists";
    public const string ErrorMessageInvalidIdFormat = "The provided ID is not in a valid GUID format.";
    public const string ErrorMessagePromotionNotFound = "Promotion not found.";
    public const string ErrorMessageInvalidShopId = "Invalid shopId";
    public const string ErrorMessageInvalidDateRange = "toDate must be after fromDate";
    public const string ErrorMessageNoItemsInCart = "No items found in the cart.";
    public const string ErrorMessageInvalidUserId = "The provided user ID is invalid.";


    #endregion


}