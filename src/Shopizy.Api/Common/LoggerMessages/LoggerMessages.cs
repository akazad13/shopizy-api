namespace Shopizy.Api.Common.LoggerMessages;

/// <summary>
/// Defines logger messages for the application.
/// </summary>
public static partial class LoggerMessages
{
    /// <summary>
    /// Logs an error when fetching a category fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching category."
    )]
    public static partial void CategoryFetchError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when creating a category fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "An error occurred while creating category."
    )]
    public static partial void CategoryCreationError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when updating a category fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "An error occur while updating category."
    )]
    public static partial void CategoryUpdateError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when deleting a category fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting category."
    )]
    public static partial void CategoryDeleteError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when fetching a cart fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching cart."
    )]
    public static partial void CartFetchError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when creating a cart fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "An error occurred while creating cart."
    )]
    public static partial void CartCreationError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when updating a cart fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Error,
        Message = "An error occur while updating cart."
    )]
    public static partial void CartUpdateError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when removing an item from the cart fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Error,
        Message = "An error occurred while removing item from cart."
    )]
    public static partial void RemoveItemFromCartError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when fetching an order fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching order."
    )]
    public static partial void OrderFetchError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when creating an order fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1009,
        Level = LogLevel.Error,
        Message = "An error occurred while creating order."
    )]
    public static partial void OrderCreationError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when cancelling an order fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Error,
        Message = "An error occur while cancelling cart."
    )]
    public static partial void CancelOrderError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when fetching a product fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching product."
    )]
    public static partial void ProductFetchError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when creating a product fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1012,
        Level = LogLevel.Error,
        Message = "An error occurred while creating product."
    )]
    public static partial void ProductCreationError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when updating a product fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1013,
        Level = LogLevel.Error,
        Message = "An error occur while updating product."
    )]
    public static partial void ProductUpdateError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when deleting a product fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1014,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting product."
    )]
    public static partial void ProductDeleteError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when adding a product image fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1015,
        Level = LogLevel.Error,
        Message = "An error occurred while adding product image."
    )]
    public static partial void ProductImageAdditionError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when deleting a product image fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1016,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting product image."
    )]
    public static partial void ProductImageDeleteError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when fetching a user fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1017,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching user."
    )]
    public static partial void UserFetchError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when updating a user fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1018,
        Level = LogLevel.Error,
        Message = "An error occurred while updating user."
    )]
    public static partial void UserUpdateError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when updating a user's address fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1019,
        Level = LogLevel.Error,
        Message = "An error occur while updating user address."
    )]
    public static partial void UserAddressUpdateError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when updating a user's password fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1020,
        Level = LogLevel.Error,
        Message = "An error occur while updating user password."
    )]
    public static partial void UserPasswordUpdateError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when a payment fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1021,
        Level = LogLevel.Error,
        Message = "An error occur while payment."
    )]
    public static partial void PaymentError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when user registration fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 1022,
        Level = LogLevel.Error,
        Message = "An error occur while registering user."
    )]
    public static partial void UserRegisterError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an error when user login fails.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(EventId = 1023, Level = LogLevel.Error, Message = "An error occur while login.")]
    public static partial void UserLoginError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs an unhandled exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="message">The exception message.</param>
    [LoggerMessage(
        EventId = 1024,
        Level = LogLevel.Error,
        Message = "An unhandled exception has occurred: {Message}"
    )]
    public static partial void UnhandledExceptionError(
        this ILogger logger,
        Exception ex,
        string message
    );

    [LoggerMessage(
        EventId = 1025,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching wishlist."
    )]
    public static partial void WishlistFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1026,
        Level = LogLevel.Error,
        Message = "An error occurred while creating wishlist."
    )]
    public static partial void WishlistCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1027,
        Level = LogLevel.Error,
        Message = "An error occurred while updating wishlist."
    )]
    public static partial void WishlistUpdateError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1045,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting wishlist."
    )]
    public static partial void WishlistDeleteError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1028,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching dashboard metrics."
    )]
    public static partial void DashboardMetricsFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1029,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching admin orders list."
    )]
    public static partial void AdminOrdersListFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1030,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching admin order detail."
    )]
    public static partial void AdminOrderDetailFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1031,
        Level = LogLevel.Error,
        Message = "An error occurred while updating order status."
    )]
    public static partial void OrderStatusUpdateError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1032,
        Level = LogLevel.Error,
        Message = "An error occurred while updating user role."
    )]
    public static partial void UserRoleUpdateError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1033,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching users list."
    )]
    public static partial void UsersListFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1034,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching brands."
    )]
    public static partial void BrandsFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1035,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching product reviews."
    )]
    public static partial void ProductReviewFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1036,
        Level = LogLevel.Error,
        Message = "An error occurred while creating product review."
    )]
    public static partial void ProductReviewCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1037,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting product review."
    )]
    public static partial void ProductReviewDeleteError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1038,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching promo codes."
    )]
    public static partial void PromoCodeFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1039,
        Level = LogLevel.Error,
        Message = "An error occurred while creating promo code."
    )]
    public static partial void PromoCodeCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1040,
        Level = LogLevel.Error,
        Message = "An error occurred while updating promo code."
    )]
    public static partial void PromoCodeUpdateError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1041,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting promo code."
    )]
    public static partial void PromoCodeDeleteError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1051,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching brand."
    )]
    public static partial void BrandFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1052,
        Level = LogLevel.Error,
        Message = "An error occurred while creating brand."
    )]
    public static partial void BrandCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1053,
        Level = LogLevel.Error,
        Message = "An error occurred while updating brand."
    )]
    public static partial void BrandUpdateError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1054,
        Level = LogLevel.Error,
        Message = "An error occurred while deleting brand."
    )]
    public static partial void BrandDeleteError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1055,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching payment."
    )]
    public static partial void PaymentFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1056,
        Level = LogLevel.Error,
        Message = "An error occurred while processing Stripe webhook."
    )]
    public static partial void StripeWebhookError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1057,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching return request(s)."
    )]
    public static partial void ReturnFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1058,
        Level = LogLevel.Error,
        Message = "An error occurred while creating return request."
    )]
    public static partial void ReturnCreationError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1059,
        Level = LogLevel.Error,
        Message = "An error occurred while approving return request."
    )]
    public static partial void ReturnApprovalError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1060,
        Level = LogLevel.Error,
        Message = "An error occurred while rejecting return request."
    )]
    public static partial void ReturnRejectionError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1061,
        Level = LogLevel.Error,
        Message = "An error occurred while upvoting product review."
    )]
    public static partial void ProductReviewUpvoteError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1062,
        Level = LogLevel.Error,
        Message = "An error occurred while fetching user notification preferences."
    )]
    public static partial void NotificationPreferencesFetchError(this ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1063,
        Level = LogLevel.Error,
        Message = "An error occurred while updating user notification preferences."
    )]
    public static partial void NotificationPreferencesUpdateError(
        this ILogger logger,
        Exception ex
    );
}
