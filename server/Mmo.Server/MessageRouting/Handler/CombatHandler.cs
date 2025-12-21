using Mmo.Shared.Core.Interfaces;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.MessageRouting.Handler;

/// <summary>
///     Handler for Combat (MessageCategory.Combat, range 300-399).
///     Handles combat-related messages like attacks, skills, damage.
/// </summary>
public class CombatHandler : BaseCategoryHandler
{
    public CombatHandler(ILog log) : base(log)
    {
    }

    public override MessageCategory Category => MessageCategory.Combat;

    protected override void RegisterHandlers()
    {
        // TODO: Register handlers for combat messages
        // Example:
        // Register(MessageType.CombatAttack, HandleAttack);
        // Register(MessageType.CombatSkillUse, HandleSkillUse);
    }

    // Example handler method structure:
    // private void HandleAttack(MessageContext ctx, AttackMessage msg)
    // {
    //     if (!RequireInGame(ctx)) return;
    //     
    //     var combatService = ctx.GetService<ICombatService>();
    //     // Handler logic here - delegate to service
    // }
}
