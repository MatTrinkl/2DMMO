using MessagePack;
using Mmo.Shared.Enums.Messages;
using Mmo.Shared.Interfaces;

namespace Mmo.Shared.Messages.System;

/// <summary>
///     Allgemeine Fehlermeldung vom Server an den Client.
/// </summary>
[MessagePackObject]
public class ErrorMessage : INetworkMessage
{
    /// <summary>
    ///     The constructor used bei <see cref="MessagePackSerializer" />.
    /// </summary>
    [SerializationConstructor]
    public ErrorMessage()
    {
    }

    /// <summary>
    ///     Creates a new Ping Message. Client->Server
    /// </summary>
    /// <param name="code"> Fehler-Code (maschinenlesbar). Z.B. "AUTH_FAILED", "INVALID_INPUT", "NOT_FOUND", "RATE_LIMITED".</param>
    /// <param name="message">Benutzerfreundliche Fehlermeldung (lokalisierbar).</param>
    /// <param name="details">Optionale zusätzliche Details (für Debugging).</param>
    /// <param name="field">Optionaler Verweis auf das fehlerhafte Feld (für Formulare).</param>
    public ErrorMessage(string code, string message, string? details, string? field)
    {
        Code = code;
        Message = message;
        Details = details;
        Field = field;
    }

    [Key(0)] public MessageType Type => MessageType.ErrorMessage;

    /// <summary>
    ///     Fehler-Code (maschinenlesbar).
    ///     Z.B. "AUTH_FAILED", "INVALID_INPUT", "NOT_FOUND", "RATE_LIMITED".
    /// </summary>
    [Key(1)]
    public string Code { get; set; } = "";

    /// <summary>
    ///     Benutzerfreundliche Fehlermeldung (lokalisierbar).
    /// </summary>
    [Key(2)]
    public string Message { get; set; } = "";

    /// <summary>
    ///     Optionale zusätzliche Details (für Debugging).
    /// </summary>
    [Key(3)]
    public string? Details { get; set; }

    /// <summary>
    ///     Optionaler Verweis auf das fehlerhafte Feld (für Formulare).
    /// </summary>
    [Key(4)]
    public string? Field { get; set; }
}
