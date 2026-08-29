namespace Shopizy.Api.Common.LoggerMessages;

public static partial class LoggerMessages
{
    [LoggerMessage(
        EventId = 1070,
        Level = LogLevel.Error,
        Message = "An error occurred while sending email notification."
    )]
    public static partial void EmailDispatchError(this ILogger logger, Exception ex);
}
