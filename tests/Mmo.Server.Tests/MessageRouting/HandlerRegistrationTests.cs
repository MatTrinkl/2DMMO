using Mmo.Server.MessageRouting.Handler;
using Mmo.Server.Tests.Helpers;
using Mmo.Shared.Messaging.Enums;

namespace Mmo.Server.Tests.MessageRouting;

public class HandlerRegistrationTests
{
    private readonly MockLog _log = new();

    [Fact]
    public void ZoneEventHandler_CanBeInstantiated()
    {
        var handler = new ZoneEventHandler(_log);

        Assert.NotNull(handler);
        Assert.Equal(MessageCategory.Zone, handler.Category);
    }

    [Fact]
    public void MovementHandler_CanBeInstantiated()
    {
        var handler = new MovementHandler(_log);

        Assert.NotNull(handler);
        Assert.Equal(MessageCategory.Movement, handler.Category);
    }

    [Fact]
    public void CombatHandler_CanBeInstantiated()
    {
        var handler = new CombatHandler(_log);

        Assert.NotNull(handler);
        Assert.Equal(MessageCategory.Combat, handler.Category);
    }

    [Fact]
    public void ChatHandler_CanBeInstantiated()
    {
        var handler = new ChatHandler(_log);

        Assert.NotNull(handler);
        Assert.Equal(MessageCategory.Chat, handler.Category);
    }

    [Fact]
    public void PingHandler_CanBeInstantiated()
    {
        var handler = new PingHandler(_log);

        Assert.NotNull(handler);
        Assert.Equal(MessageCategory.Ping, handler.Category);
    }

    [Fact]
    public void AllHandlers_HaveCorrectCategory()
    {
        var zoneEventHandler = new ZoneEventHandler(_log);
        var movementHandler = new MovementHandler(_log);
        var combatHandler = new CombatHandler(_log);
        var chatHandler = new ChatHandler(_log);
        var pingHandler = new PingHandler(_log);

        // Verify categories match expected values
        Assert.Equal((int)MessageCategory.Zone, (int)zoneEventHandler.Category);
        Assert.Equal((int)MessageCategory.Movement, (int)movementHandler.Category);
        Assert.Equal((int)MessageCategory.Combat, (int)combatHandler.Category);
        Assert.Equal((int)MessageCategory.Chat, (int)chatHandler.Category);
        Assert.Equal((int)MessageCategory.Ping, (int)pingHandler.Category);

        // Verify numeric ranges
        Assert.Equal(1, (int)zoneEventHandler.Category); // 100-199 range
        Assert.Equal(2, (int)movementHandler.Category); // 200-299 range
        Assert.Equal(3, (int)combatHandler.Category); // 300-399 range
        Assert.Equal(4, (int)chatHandler.Category); // 400-499 range
        Assert.Equal(9, (int)pingHandler.Category); // 900-999 range
    }

    [Fact]
    public void Handlers_DoNotHandleUnregisteredMessages()
    {
        var handler = new ZoneEventHandler(_log);

        // No handlers registered yet (empty implementation)
        // So CanHandle should return false for any message type
        Assert.False(handler.CanHandle(MessageType.ZoneTransferRequest));
    }
}
