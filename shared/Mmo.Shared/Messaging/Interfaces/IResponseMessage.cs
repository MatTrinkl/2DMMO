using Mmo.Shared.Messaging.Enums;

namespace Mmo.Shared.Messaging.Interfaces;

/// <summary>
///     Base-Interface for all response messages.
/// </summary>
public interface IResponseMessage : IServerMessage
{
    /// <summary>
    ///     Was the operation successful.
    /// </summary>
    bool Success { get; init; }

    /// <summary>
    ///     Global Error - ALWAYS check first!
    ///     If != None, then ignore specific error code.
    ///     Only used when <see cref="Success" /> is false.
    /// </summary>
    GlobalErrorCode GlobalError { get; init; }

    /// <summary>
    ///     Readable Errormessage for UI and logs.
    /// </summary>
    string? ErrorMessage { get; init; }
}

/// <summary>
///     Generic Interface for Responses with specific ErrorCode.
/// </summary>
/// <typeparam name="TErrorCode">The specific ErrorCode-Enum-Type.</typeparam>
public interface IResponseMessage<TErrorCode> : IResponseMessage
    where TErrorCode : struct, Enum
{
    /// <summary>
    ///     Specific Error for this MessageType.
    ///     Only relevant if <see cref="GlobalErrorCode" /> is None and <see cref="IResponseMessage.Success" /> is false.
    /// </summary>
    TErrorCode? ErrorCode { get; init; }
}
