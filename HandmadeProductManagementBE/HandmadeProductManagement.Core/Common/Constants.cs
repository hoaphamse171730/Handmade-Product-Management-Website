namespace HandmadeProductManagement.Core.Common;

public static class Constants
{
    #region Base

    public const string ApiBaseUrl = "https://localhost:7159";
    public const string AvatarBaseUrl = "/images/avatars";
    public const string VNPAY = "VNPAY";
    public const string TimeZoneSEAsiaStandard = "SE Asia Standard Time";
    public const string DateTimeFormat = "dd/MM/yyyy HH:mm";

    #endregion

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

    #region Payment

    public const string PaymentStatusPending = "Pending";
    public const string PaymentStatusPaid = "Paid";
    public const string PaymentStatusCompleted = "Completed";
    public const string PaymentStatusExpired = "Expired";
    public const string PaymentStatusRefunded = "Refunded";
    public const string PaymentStatusSuccess = "Success";
    public const string PaymentStatusFailed = "Failed";
    public const string PaymentMethodOnline = "Online";
    public const string PaymentMethodOffline = "Offline";
    public const string PaymentDescriptionFailed = "Failed";
    public const string PaymentMethodTransfer = "Transfer";
    public const string PaymentApproveFailed = "Payment approve failed";
    public const string VNPayBanking = "VNPay_banking";
    public const int PaymentExpirationDays = 15;

    #endregion

    #region Product Status

    public const string ProductStatusAvailable = "Available";
    public const string ProductStatusUnavailable = "Unavailable";

    #endregion

    #region Role Constants

    public const string RoleAdmin = "Admin";
    public const string RoleSeller = "Seller";
    public const string RoleCustomer = "Customer";
    public const string RoleSystem = "System";

    #endregion

    #region Notification Tag

    public const string NotificationTagOrder = "Order";
    public const string NotificationTagReview = "Review";
    public const string NotificationTagReply = "Reply";
    public const string NotificationTagStatusChange = "StatusChange";
    public const string NotificationTagPaymentExpiration = "PaymentExpiration";

    #endregion

    #region Error Messages

    public const string ErrorMessageForbidden = "You do not have permission to access this resource.";
    public const string OrderNotFoundMessage = "The order you're looking for could not be found.";
    public const string ErrorMessageEmptyId = "The ID field cannot be empty.";
    public const string ErrorMessageOrderNotFound = "The order you're looking for could not be found.";
    public const string ErrorMessageOrderDetailsNotFound = "We couldn't find any details for this order.";
    public const string ErrorMessageEmptyCart = "Your cart is currently empty.";
    public const string ErrorMessageProductItemNotFound = "The product item could not be found.";
    public const string ErrorMessageProductNotFound = "The product you're looking for could not be found.";
    public const string ErrorMessageInsufficientStock = "There is not enough stock available for this product.";
    public const string ErrorMessageNoPermission = "You do not have permission to access this resource.";
    public const string ErrorMessageInvalidOrderStatus = "The order is being processed and cannot be updated.";
    public const string ErrorMessageInvalidAddressFormat = "The address cannot contain special characters except for commas and periods.";
    public const string ErrorMessageInvalidCustomerNameFormat = "The customer name can only contain letters and spaces.";
    public const string ErrorMessageInvalidPhoneFormat = "The phone number must start with 0 and be 10 or 11 digits long.";
    public const string ErrorMessageUserNotFound = "The user could not be found.";
    public const string ErrorMessageCannotTransition = "Unable to change status from {0} to {1}.";
    public const string ErrorMessageNoOrdersFound = "There are no orders associated with this user.";
    public const string ErrorMessageInvalidStatus = "The status cannot be empty or null.";
    public const string ErrorMessageInvalidCancelReason = "A CancelReasonId must be null if the status is not Canceled.";
    public const string ErrorMessageOrderClosed = "The order has already been closed.";
    public const string ErrorMessageCancelReasonRequired = "A CancelReasonId is required when canceling the order.";
    public const string ErrorMessageShopNotFound = "No shop was found for the given user.";
    public const string ErrorMessageInvalidAddress = "The address cannot be empty.";
    public const string ErrorMessageInvalidCustomerName = "The customer name cannot be empty.";
    public const string ErrorMessageInvalidPhone = "The phone number cannot be empty.";
    public const string ErrorMessageCategoryNotFound = "The specified category could not be found.";
    public const string ErrorMessageMissingLoginIdentifier = "Please provide at least one of Phone Number, Email, or Username for login.";
    public const string ErrorMessageInvalidEmailFormat = "The email address format is invalid.";
    public const string ErrorMessageInvalidUsernameFormat = "The username cannot contain special characters.";
    public const string ErrorMessageAccountDisabled = "This account has been disabled.";
    public const string ErrorMessageUnauthorized = "The login credentials provided are incorrect.";
    public const string ErrorMessageIncorrectPassword = "The password you entered is incorrect.";
    public const string ErrorMessageUsernameTaken = "This username is already taken.";
    public const string ErrorMessageEmailTaken = "This email address is already in use.";
    public const string ErrorMessagePhoneTaken = "This phone number is already in use.";
    public const string ErrorMessageUserCreationFailed = "There was an error while creating the user.";
    public const string ErrorMessageInvalidFullnameFormat = "The full name contains invalid characters.";
    public const string ErrorMessageWeakPassword = "The password is too weak. It must be at least 8 characters long and contain uppercase, lowercase letters, a special character, and a number.";
    public const string ErrorMessageInvalidEmail = "The email is invalid or not confirmed.";
    public const string MessagePasswordResetLinkSent = "A password reset link has been sent to your email.";
    public const string MessagePasswordResetSuccess = "Your password has been reset successfully.";
    public const string ErrorMessageResetPasswordError = "There was an error resetting the password.";
    public const string MessageEmailConfirmedSuccess = "Your email has been successfully confirmed.";
    public const string ErrorMessageEmailConfirmationError = "There was an error confirming your email.";
    public const string ErrorMessageInvalidToken = "The token provided is invalid.";
    public const string ErrorMessageMissingClaims = "The token is missing necessary claims.";
    public const string UserNotFoundErrorMessage = "The user could not be found.";
    public const string RoleAssignmentFailedErrorMessage = "There was an error assigning a role to the user.";
    public const string ErrorMessageRoleAssignmentFailed = "There was an error assigning a role to the user.";
    public const string ErrorMessageInvalidGuidFormat = "The provided ID is not in a valid GUID format.";
    public const string ErrorMessageCancelReasonNotFound = "The cancel reason could not be found.";
    public const string ErrorMessageCannotAddOwnProduct = "You cannot add your own product to the cart.";
    public const string ErrorMessageInsufficientStockForUpdate = "There is insufficient stock for the updated cart item.";
    public const string ErrorMessageCartItemNotFound = "The cart item could not be found.";
    public const string ErrorMessageCategoryNameExists = "This category name is already in use.";
    public const string ErrorMessagePromotionNotFound = "The promotion could not be found.";
    public const string ErrorMessageInvalidDateRange = "The end date must be after the start date.";
    public const string ErrorMessageNoItemsInCart = "No items were found in the cart.";
    public const string ErrorMessagePaymentNotFound = "The payment could not be found.";
    public const string ErrorMessageInvalidPaymentStatus = "Cannot create payment detail for a payment with status '{0}'.";
    public const string ErrorMessageInvalidPaymentId = "Please provide a payment ID.";
    public const string ErrorMessagePleaseInputStatus = "Please provide a status.";
    public const string ErrorMessageInvalidStatusFormat = "The status must be either 'Success' or 'Failed'.";
    public const string ErrorMessagePleaseInputMethod = "Please provide a payment method.";
    public const string ErrorMessageInvalidMethodFormat = "The payment method cannot contain numbers or special characters.";
    public const string ErrorMessageInvalidExternalTransactionFormat = "The external transaction can only contain alphanumeric characters.";
    public const string ErrorMessageUserNotOwner = "You do not have permission to access this order.";
    public const string ErrorMessagePaymentAlreadyExists = "A payment for this order already exists.";
    public const string ErrorMessageInvalidStatusTransition = "Unable to change status from {0} to {1}.";
    public const string ErrorMessageValidationFailed = "Validation failed. Please check your inputs.";
    public const string ErrorMessageInvalidProductItem = "The specified product item could not be found.";
    public const string ErrorMessageInvalidVariationOption = "The specified variation option does not exist.";
    public const string ErrorMessageProductConfigurationNotFound = "The product configuration could not be found.";
    public const string ErrorMessageFileNotFound = "The file could not be found.";
    public const string ErrorMessageImageNotFound = "The image could not be found.";
    public const string ErrorMessageVariationOptionNotFound = "Some variation options could not be found.";
    public const string ErrorMessageDuplicateCombination = "The provided variation options are already associated with this product.";
    public const string ErrorMessageInvalidCombination = "A combination cannot include multiple options from the same variation.";
    public const string ErrorMessageIncompleteCombinations = "The combination is missing options from other variations.";
    public const string ErrorMessageInvalidProduct = "The specified product is invalid.";
    public const string ErrorMessageVariationNotFound = "The variation could not be found.";
    public const string ErrorMessageVariationOptionNotBelongToVariation = "The variation option with ID {0} does not belong to variation {1}.";
    public const string ErrorMessageInvalidPageNumber = "The page number must be greater than zero.";
    public const string ErrorMessageInvalidPageSize = "The page size must be greater than zero.";
    public const string ErrorMessageMinRatingOutOfRange = "The minimum rating must be between 0 and 5.";
    public const string ErrorMessageProductAlreadyHasStatus = "The product already has the status '{0}'.";
    public const string ErrorMessageNameInUse = "This name is already in use.";
    public const string ErrorMessageNullReplyModel = "The reply model cannot be null.";
    public const string ErrorMessageInvalidReviewIdFormat = "The format of the review ID is invalid.";
    public const string ErrorMessageReviewNotFound = "The review could not be found.";
    public const string ErrorMessageUnauthorizedShopAccess = "You do not have permission to access this shop.";
    public const string ErrorMessageShopCannotReply = "The specified shop cannot reply to this review.";
    public const string ErrorMessageReplyAlreadyExists = "This review already has a reply.";
    public const string ErrorMessageInvalidReplyIdFormat = "The format of the reply ID is invalid.";
    public const string ErrorMessageReplyNotFound = "The reply could not be found.";
    public const string ErrorMessageUnauthorizedUpdate = "You do not have permission to update this reply.";
    public const string ErrorMessageSoftDeletedReply = "You cannot update a review that has been soft-deleted.";
    public const string ErrorMessageProductSoftDeleted = "This product has been soft-deleted and cannot be found.";
    public const string ErrorMessageUnauthorizedOrderAccess = "You do not have permission to access this order.";
    public const string ErrorMessageOrderNotShipped = "You can only review an order that has been shipped.";
    public const string ErrorMessageReviewAlreadyExists = "You have already reviewed this product.";
    public const string ErrorMessageInvalidRating = "The rating must be between 1 and 5.";
    public const string ErrorMessageReviewNotFoundSoftDeleted = "The review has been soft-deleted and cannot be updated.";
    public const string ErrorMessageUserActiveShop = "The user already has an active shop.";
    public const string ErrorMessageShopNotFoundForUser = "No shop was found for the specified";
    public const string ErrorMessageUserInfoNotFound = "User info could not be found";
    public const string ErrorMessageOrderPriceNotFound = "Order price could not be found";
    public const string ErrorMessageInvalidShopName = "The shop name cannot be empty.";
    public const string ErrorMessageInvalidShopNameFormat = "The shop name contains invalid characters.";
    public const string ErrorMessageInvalidShopDescription = "The shop description cannot be empty.";
    public const string ErrorMessageInvalidShopDescriptionFormat = "The shop description contains invalid characters.";
    public const string ErrorMessageInvalidFileType = "The file type is not supported.";
    public const string ErrorMessageDuplicateVariationName = "A variation with this name already exists.";

    #endregion


}