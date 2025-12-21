using Mmo.Server.MessageRouting.Handler;
using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handlers;

/// <summary>
///     Handler for Combat messages (Category 3, range 300-399).
///     Handles damage, healing, combat events.
/// </summary>
public class CombatHandler : BaseCategoryHandler
{
    public CombatHandler(ILog log) : base(log)
    {
    }

    /// <inheritdoc />
    public override MessageCategory Category => MessageCategory.Combat;

    /// <inheritdoc />
    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for specific Combat message types
        // Example:
        // Register(MessageType.ActionRequest, HandleActionRequest);
        // Register(MessageType.DamageEvent, HandleDamageEvent);
        // Register(MessageType.HealEvent, HandleHealEvent);
    }

    // TODO: Add handler methods
    // Example:
    // private void HandleActionRequest(MessageContext ctx, ActionRequestMessage msg) { }
}
