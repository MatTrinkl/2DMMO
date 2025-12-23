using System.Linq.Expressions;
using System.Reflection;
using MessagePack;
using Mmo.Shared.Messaging.Attributes;
using Mmo.Shared.Messaging.Enums;
using Mmo.Shared.Messaging.Exceptions;
using Mmo.Shared.Messaging.Helper;
using Mmo.Shared.Messaging.Interfaces;

namespace Mmo.Shared.Messaging.Serialization;

/// <summary>
///     This class Serializes and deserializes a message based on its <see cref="MessageType" />.
///     Uses an attribute-based registry system for automatic message type registration.
/// </summary>
public static class MessageSerializer
{
    /// <summary>
    ///     Registry mapping MessageType to deserialization function.
    ///     O(1) lookup for message deserialization with compiled expression delegates.
    /// </summary>
    private static readonly Dictionary<MessageType, Func<ReadOnlyMemory<byte>, INetworkMessage>> MessageRegistry = new();

    /// <summary>
    ///     Static constructor that automatically registers all message types with the [NetworkMessage] attribute.
    ///     Note: Only scans the Mmo.Shared assembly. Message types in other assemblies will not be registered.
    /// </summary>
    static MessageSerializer()
    {
        // Find all types in the Mmo.Shared assembly that have the NetworkMessageAttribute
        var assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .ToList();

        // Register each message type with a compiled expression delegate
        foreach (var messageType in messageTypes)
        {
            // Validate that the type implements INetworkMessage
            if (!typeof(INetworkMessage).IsAssignableFrom(messageType))
            {
                throw new InvalidOperationException(
                    $"Type {messageType.FullName} has [NetworkMessage] attribute but does not implement INetworkMessage interface.");
            }

            var attribute = messageType.GetCustomAttribute<NetworkMessageAttribute>()!;
            
            // Check for duplicate registrations
            if (MessageRegistry.ContainsKey(attribute.Type))
            {
                var existingType = MessageRegistry[attribute.Type].Method.DeclaringType;
                throw new InvalidOperationException(
                    $"Duplicate MessageType registration detected: {attribute.Type} is registered for both {existingType?.FullName} and {messageType.FullName}");
            }

            // Create an optimized compiled expression delegate for deserialization
            // This avoids reflection overhead at runtime
            var deserializer = CreateDeserializer(messageType);
            MessageRegistry[attribute.Type] = deserializer;
        }
    }

    /// <summary>
    ///     Creates a compiled expression delegate for deserializing a specific message type.
    ///     This provides near-native performance without runtime reflection overhead.
    /// </summary>
    private static Func<ReadOnlyMemory<byte>, INetworkMessage> CreateDeserializer(Type messageType)
    {
        // Find the MessagePackSerializer.Deserialize<T> method
        var deserializeMethod = typeof(MessagePackSerializer)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(MessagePackSerializer.Deserialize))
            .Where(m => m.IsGenericMethodDefinition)
            .Where(m => m.GetParameters().Length == 3)
            .Where(m => m.GetParameters()[0].ParameterType == typeof(ReadOnlyMemory<byte>))
            .FirstOrDefault();

        if (deserializeMethod == null)
        {
            throw new InvalidOperationException(
                "Could not find MessagePackSerializer.Deserialize<T>(ReadOnlyMemory<byte>, MessagePackSerializerOptions, CancellationToken) method.");
        }

        // Create the generic method for this specific message type
        var genericMethod = deserializeMethod.MakeGenericMethod(messageType);

        // Build an expression tree: (data) => (INetworkMessage)MessagePackSerializer.Deserialize<T>(data, null, default)
        var dataParam = Expression.Parameter(typeof(ReadOnlyMemory<byte>), "data");
        var nullOptions = Expression.Constant(null, typeof(MessagePackSerializerOptions));
        var defaultToken = Expression.Default(typeof(CancellationToken));
        
        var methodCall = Expression.Call(genericMethod, dataParam, nullOptions, defaultToken);
        var castToInterface = Expression.Convert(methodCall, typeof(INetworkMessage));
        
        var lambda = Expression.Lambda<Func<ReadOnlyMemory<byte>, INetworkMessage>>(castToInterface, dataParam);
        
        // Compile the expression into a delegate - this happens once at startup
        return lambda.Compile();
    }

    /// <summary>
    ///     Serializes a message to bytes with type prefix
    ///     {MessageTye} needs to be Key(0).
    /// </summary>
    public static byte[] Serialize<T>(T message) where T : INetworkMessage => MessagePackSerializer.Serialize(message);

    /// <summary>
    ///     Deserializes a message based on its type using the registered message types.
    /// </summary>
    public static INetworkMessage Deserialize(ReadOnlyMemory<byte> data)
    {
        MessageHeader header = MessagePackSerializer.Deserialize<MessageHeader>(data);

        if (!MessageRegistry.TryGetValue(header.Type, out var deserializer))
        {
            throw new UnknownMessageTypeException(header.Type);
        }

        return deserializer(data);
    }
}
