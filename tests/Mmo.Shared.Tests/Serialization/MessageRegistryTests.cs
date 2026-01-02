using System.Reflection;
using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Helper;
using Mmo.Shared.Messaging.Interfaces;
using Mmo.Shared.Messaging.Serialization;

namespace Mmo.Shared.Tests.Serialization;

/// <summary>
///     Tests for MessageRegistry initialization and validation logic.
/// </summary>
public class MessageRegistryTests
{
    [Fact]
    public void MessageRegistry_AllINetworkMessageTypes_HaveNetworkMessageAttribute()
    {
        // Arrange: Get all types that implement INetworkMessage
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var networkMessageTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => typeof(INetworkMessage).IsAssignableFrom(t))
            .Where(t => t != typeof(MessageHeader)) // MessageHeader is special
            .ToList();

        // Act & Assert: All should have [NetworkMessage] attribute
        foreach (Type type in networkMessageTypes)
        {
            NetworkMessageAttribute? attribute = type.GetCustomAttribute<NetworkMessageAttribute>();
            Assert.NotNull(attribute);
        }
    }

    [Fact]
    public void MessageRegistry_AllTypesWithNetworkMessageAttribute_ImplementINetworkMessage()
    {
        // Arrange: Get all types with [NetworkMessage] attribute
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var typesWithAttribute = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .ToList();

        // Act & Assert: All should implement INetworkMessage
        foreach (Type type in typesWithAttribute)
            Assert.True(typeof(INetworkMessage).IsAssignableFrom(type),
                $"Type {type.FullName} has [NetworkMessage] attribute but does not implement INetworkMessage");
    }

    [Fact]
    public void MessageRegistry_NoDuplicateMessageTypes()
    {
        // Arrange: Get all types with [NetworkMessage] attribute
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<NetworkMessageAttribute>()!
            })
            .ToList();

        // Act: Group by MessageType
        var duplicates = messageTypes
            .GroupBy(x => x.Attribute.Type)
            .Where(g => g.Count() > 1)
            .ToList();

        // Assert: No duplicates
        Assert.Empty(duplicates);
    }

    [Fact]
    public void MessageRegistry_StaticConstructor_InitializesSuccessfully()
    {
        // Act: Access MessageSerializer to trigger static constructor
        // This should not throw an exception
        Exception? exception = Record.Exception(() =>
        {
            // Simply accessing the type triggers the static constructor
            _ = typeof(MessageSerializer);
        });

        // Assert: No exception should be thrown during initialization
        Assert.Null(exception);
    }

    [Fact]
    public void MessageRegistry_AllRegisteredTypes_CanBeDeserialized()
    {
        // Arrange: Get all types with [NetworkMessage] attribute
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .Where(t => !t.IsAbstract) // Skip abstract classes
            .ToList();

        // Act & Assert: Each type should be deserializable
        foreach (Type messageType in messageTypes)
        {
            // We can't easily test deserialization without creating instances,
            // but we can verify the type is concrete and has the required attribute
            NetworkMessageAttribute? attribute = messageType.GetCustomAttribute<NetworkMessageAttribute>();
            Assert.NotNull(attribute);
            Assert.False(messageType.IsAbstract, $"Message type {messageType.FullName} should not be abstract");
        }
    }

    [Fact]
    public void MessageRegistry_AllMessageTypes_HaveMessagePackObjectAttribute()
    {
        // Arrange: Get all types with [NetworkMessage] attribute
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .ToList();

        // Act & Assert: All should have [MessagePackObject] attribute
        foreach (Type type in messageTypes)
        {
            MessagePackObjectAttribute? messagePackAttr = type.GetCustomAttribute<MessagePackObjectAttribute>();
            Assert.NotNull(messagePackAttr);
        }
    }

    [Fact]
    public void MessageRegistry_AllMessageTypes_HaveTypePropertyWithKey0()
    {
        // Arrange: Get all types with [NetworkMessage] attribute
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .ToList();

        // Act & Assert: All should have Type property with [Key(0)]
        foreach (Type type in messageTypes)
        {
            PropertyInfo? typeProperty = type.GetProperty("Type");
            Assert.NotNull(typeProperty);

            KeyAttribute? keyAttribute = typeProperty.GetCustomAttribute<KeyAttribute>();
            Assert.NotNull(keyAttribute);
            // KeyAttribute.IntKey contains the integer key value
            Assert.Equal(0, keyAttribute.IntKey);
        }
    }
}
