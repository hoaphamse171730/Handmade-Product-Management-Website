using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Principal;
using System.Threading.Channels;

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
    public const string OrderNotFoundMessage = "We could not find your order.";
    public const string ErrorMessageEmptyId = "Please provide an ID.";
    public const string ErrorMessageOrderNotFound = "We could not find your order.";
    public const string ErrorMessageOrderDetailsNotFound = "No details found for this order.";
    public const string ErrorMessageEmptyCart = "Your cart is empty.";
    public const string ErrorMessageProductItemNotFound = "The product item could not be found.";
    public const string ErrorMessageProductNotFound = "The product associated with this item could not be found.";
    public const string ErrorMessageInsufficientStock = "Insufficient stock for this product.";
    public const string ErrorMessageNoPermission = "You do not have permission to access this resource.";
    public const string ErrorMessageInvalidOrderStatus = "This order is currently being processed and cannot be updated.";
    public const string ErrorMessageInvalidAddressFormat = "Please ensure your address only includes letters, numbers, commas, and periods.";
    public const string ErrorMessageInvalidCustomerNameFormat = "Customer names can only include letters and spaces.";
    public const string ErrorMessageInvalidPhoneFormat = "Phone numbers must be numeric, begin with '0', and be 10 or 11 digits long.";
    public const string ErrorMessageUserNotFound = "We could not find a user matching your details.";
    public const string ErrorMessageCannotTransition = "Unable to change status from {0} to {1}.";
    public const string ErrorMessageNoOrdersFound = "No orders found for this user.";
    public const string ErrorMessageInvalidStatus = "Status cannot be left empty.";
    public const string ErrorMessageInvalidCancelReason = "A cancellation reason is required when setting the status to 'Canceled'.";
    public const string ErrorMessageOrderClosed = "This order has been closed and cannot be updated.";
    public const string ErrorMessageCancelReasonRequired = "Please provide a reason for cancelling the order.";
    public const string ErrorMessageShopNotFound = "We could not find a shop associated with this user.";
    public const string ErrorMessageInvalidAddress = "Please provide a valid address.";
    public const string ErrorMessageInvalidCustomerName = "Please provide a valid customer name.";
    public const string ErrorMessageInvalidPhone = "Please provide a valid phone number.";
    public const string ErrorMessageCategoryNotFound = "We could not find the specified category.";
    public const string ErrorMessageMissingLoginIdentifier = "Please provide at least one of the following: Phone Number, Email, or Username.";
    public const string ErrorMessageInvalidEmailFormat = "The email address format is invalid.";
    public const string ErrorMessageInvalidUsernameFormat = "Usernames cannot contain special characters.";
    public const string ErrorMessageAccountDisabled = "This account has been disabled.";
    public const string ErrorMessageUnauthorized = "The login credentials provided are incorrect.";
    public const string ErrorMessageIncorrectPassword = "The password entered is incorrect.";
    public const string ErrorMessageUsernameTaken = "This username is already taken.";
    public const string ErrorMessageEmailTaken = "This email address is already in use.";
    public const string ErrorMessagePhoneTaken = "This phone number is already in use.";
    public const string ErrorMessageUserCreationFailed = "We encountered an error while creating your user account.";
    public const string ErrorMessageInvalidFullnameFormat = "Full names can only contain letters and spaces.";
    public const string ErrorMessageWeakPassword = "Your password is too weak. Please include at least 8 characters, with an uppercase letter, a lowercase letter, a special character, and a number.";
    public const string ErrorMessageInvalidEmail = "The email address provided is either invalid or unverified.";
    public const string MessagePasswordResetLinkSent = "A password reset link has been sent to your email.";
    public const string MessagePasswordResetSuccess = "Your password has been successfully reset.";
    public const string ErrorMessageResetPasswordError = "We encountered an error while resetting your password.";
    public const string MessageEmailConfirmedSuccess = "Your email address has been successfully confirmed.";
    public const string ErrorMessageEmailConfirmationError = "We encountered an error while confirming your email.";
    public const string ErrorMessageInvalidToken = "The token provided is invalid.";
    public const string ErrorMessageMissingClaims = "The token is missing necessary information.";
    public const string UserNotFoundErrorMessage = "We could not find a user with the provided information.";
    public const string RoleAssignmentFailedErrorMessage = "We encountered an error while assigning a role to this user.";
    public const string ErrorMessageRoleAssignmentFailed = "We encountered an error while assigning the role.";
    public const string ErrorMessageInvalidGuidFormat = "The provided ID is not in the correct format.";
    public const string ErrorMessageCancelReasonNotFound = "We could not find the specified cancellation reason.";
    public const string ErrorMessageCannotAddOwnProduct = "You cannot add your own product to the cart.";
    public const string ErrorMessageInsufficientStockForUpdate = "There is not enough stock to update this cart item.";
    public const string ErrorMessageCartItemNotFound = "We could not find this item in your cart.";
    public const string ErrorMessageCategoryNameExists = "This category name is already in use.";
    public const string ErrorMessagePromotionNotFound = "We could not find the specified promotion.";
    public const string ErrorMessageInvalidDateRange = "The end date must be after the start date.";
    public const string ErrorMessageNoItemsInCart = "There are no items in your cart.";
    public const string ErrorMessagePaymentNotFound = "We could not find the specified payment.";
    public const string ErrorMessageInvalidPaymentStatus = "Unable to create payment details for a payment with status '{0}'.";
    public const string ErrorMessageInvalidPaymentId = "Please provide a valid payment ID.";
    public const string ErrorMessagePleaseInputStatus = "Please provide a status.";
    public const string ErrorMessageInvalidStatusFormat = "Status must be either 'Success' or 'Failed'.";
    public const string ErrorMessagePleaseInputMethod = "Please provide a payment method.";
    public const string ErrorMessageInvalidMethodFormat = "Payment methods cannot contain numbers or special characters.";
    public const string ErrorMessageInvalidExternalTransactionFormat = "External transaction IDs can only contain alphanumeric characters.";
    public const string ErrorMessageUserNotOwner = "You do not own this order.";
    public const string ErrorMessagePaymentAlreadyExists = "A payment already exists for this order.";
    public const string ErrorMessageInvalidStatusTransition = "Unable to change status from {0} to {1}.";
    public const string ErrorMessageValidationFailed = "There was an issue with the provided information. Please check and try again.";
    public const string ErrorMessageInvalidProductItem = "We could not find the specified product item.";
    public const string ErrorMessageInvalidVariationOption = "We could not find the specified variation option.";
    public const string ErrorMessageProductConfigurationNotFound = "We could not find the specified product configuration.";
    public const string ErrorMessageFileNotFound = "We could not find the specified file.";
    public const string ErrorMessageImageNotFound = "We could not find the specified image.";
    public const string ErrorMessageVariationOptionNotFound = "Some variation options could not be found.";
    public const string ErrorMessageDuplicateCombination = "All of the provided variation options are already associated with this product.";
    public const string ErrorMessageInvalidCombination = "A combination cannot include multiple options from the same variation.";
    public const string ErrorMessageIncompleteCombinations = "Some required variation options are missing from the combination.";
    public const string ErrorMessageInvalidProduct = "The provided product information is invalid.";
    public const string ErrorMessageVariationNotFound = "We could not find the specified variation.";
    public const string ErrorMessageVariationOptionNotBelongToVariation = "The Variation Option with ID {0} does not belong to the specified Variation {1}.";
    public const string ErrorMessageInvalidPageNumber = "Page numbers must be greater than zero.";
    public const string ErrorMessageInvalidPageSize = "Page sizes must be greater than zero.";
    public const string ErrorMessageMinRatingOutOfRange = "The minimum rating must be between 0 and 5.";
    public const string ErrorMessageProductAlreadyHasStatus = "The product is already in the {0} status.";
    public const string ErrorMessageNameInUse = "This name is already in use.";
    public const string ErrorMessageNullReplyModel = "Please provide a reply model.";
    public const string ErrorMessageInvalidReviewIdFormat = "The provided review ID format is invalid.";
    public const string ErrorMessageReviewNotFound = "We could not find the specified review.";
    public const string ErrorMessageUnauthorizedShopAccess = "You do not own this shop.";
    public const string ErrorMessageShopCannotReply = "This shop cannot respond to this review.";
    public const string ErrorMessageReplyAlreadyExists = "This review already has a reply.";
    public const string ErrorMessageInvalidReplyIdFormat = "The provided reply ID format is invalid.";
    public const string ErrorMessageReplyNotFound = "We could not find the specified reply.";
    public const string ErrorMessageUnauthorizedUpdate = "You do not have permission to update this reply.";
    public const string ErrorMessageSoftDeletedReply = "You cannot update a reply that has been soft-deleted.";
    public const string ErrorMessageProductSoftDeleted = "This product has been soft-deleted and cannot be found.";
    public const string ErrorMessageUnauthorizedOrderAccess = "You do not own this order.";
    public const string ErrorMessageOrderNotShipped = "You can only create a review if the order has been shipped.";
    public const string ErrorMessageReviewAlreadyExists = "You have already reviewed this product.";
    public const string ErrorMessageInvalidRating = "The rating must be between 1 and 5.";
    public const string ErrorMessageReviewNotFoundSoftDeleted = "You cannot update a review that has been soft-deleted.";
    public const string ErrorMessageUserActiveShop = "You already have an active shop.";
    public const string ErrorMessageShopNotFoundForUser = "We could not find a shop associated with your account.";
    public const string ErrorMessageUnauthorizedShop = "You do not own this shop.";
    public const string ErrorMessageInactiveProduct = "The product you are trying to view is not available.";
    public const string ErrorMessageInactiveCategory = "The category you are trying to view is not available.";
    public const string ErrorMessageInactiveVariationOption = "The variation option you are trying to view is not available.";
    public const string ErrorMessageInactiveShop = "The shop you are trying to view is not active.";
    public const string ErrorMessageCannotTransitionStatus = "Unable to transition from {existingOrder.Status} to {updateStatusOrderDto.Status}.";

    #endregion


}