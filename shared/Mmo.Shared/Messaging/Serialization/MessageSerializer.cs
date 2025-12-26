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
    private static readonly Dictionary<MessageType, Func<ReadOnlyMemory<byte>, INetworkMessage>> _messageRegistry =
        new();

    /// <summary>
    ///     Static constructor that automatically registers all message types with the [NetworkMessage] attribute.
    ///     Note: Only scans the Mmo.Shared assembly. Message types in other assemblies will not be registered.
    /// </summary>
    static MessageSerializer()
    {
        // Find all types in the Mmo.Shared assembly that have the NetworkMessageAttribute
        Assembly assembly = typeof(INetworkMessage).Assembly;
        var messageTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() != null)
            .ToList();

        // Validate: Check if there are INetworkMessage implementations without [NetworkMessage] attribute
        var unregisteredTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => typeof(INetworkMessage).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<NetworkMessageAttribute>() == null)
            .Where(t => t != typeof(MessageHeader)) // MessageHeader is special, used only for deserialization routing
            .ToList();

        if (unregisteredTypes.Any())
        {
            string unregisteredNames = string.Join(", ", unregisteredTypes.Select(t => t.FullName));
            throw new InvalidOperationException(
                $"The following types implement INetworkMessage but are missing the [NetworkMessage] attribute: {unregisteredNames}. " +
                $"All message types must have the [NetworkMessage] attribute to be registered for deserialization.");
        }

        // Register each message type with a compiled expression delegate
        foreach (Type messageType in messageTypes)
        {
            // Validate that the type implements INetworkMessage
            if (!typeof(INetworkMessage).IsAssignableFrom(messageType))
                throw new InvalidOperationException(
                    $"Type {messageType.FullName} has [NetworkMessage] attribute but does not implement INetworkMessage interface.");

            NetworkMessageAttribute attribute = messageType.GetCustomAttribute<NetworkMessageAttribute>()!;

            // Check for duplicate registrations
            if (_messageRegistry.TryGetValue(attribute.Type,
                    out Func<ReadOnlyMemory<byte>, INetworkMessage>? existingDeserializer))
            {
                // Find the type that was previously registered for this MessageType
                Type? existingType = messageTypes
                    .FirstOrDefault(t => t != messageType &&
                                         t.GetCustomAttribute<NetworkMessageAttribute>()?.Type == attribute.Type);

                string existingTypeName = existingType?.FullName ?? "<unknown>";

                throw new InvalidOperationException(
                    $"Duplicate MessageType registration detected: {attribute.Type} is registered for both {existingTypeName} and {messageType.FullName}");
            }

            // Create an optimized compiled expression delegate for deserialization
            // This avoids reflection overhead at runtime
            Func<ReadOnlyMemory<byte>, INetworkMessage> deserializer = CreateDeserializer(messageType);
            _messageRegistry[attribute.Type] = deserializer;
        }
    }

    /// <summary>
    ///     Creates a compiled expression delegate for deserializing a specific message type.
    ///     This provides near-native performance without runtime reflection overhead.
    /// </summary>
    private static Func<ReadOnlyMemory<byte>, INetworkMessage> CreateDeserializer(Type messageType)
    {
        // Find the MessagePackSerializer.Deserialize<T> method with signature:
        // T Deserialize<T>(ReadOnlyMemory<byte>, MessagePackSerializerOptions, CancellationToken)
        MethodInfo? deserializeMethod = typeof(MessagePackSerializer)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(MessagePackSerializer.Deserialize))
            .Where(m => m.IsGenericMethodDefinition)
            .Where(m => m.GetParameters().Length == 3)
            .Where(m =>
            {
                ParameterInfo[] parameters = m.GetParameters();
                return parameters[0].ParameterType == typeof(ReadOnlyMemory<byte>) &&
                       parameters[1].ParameterType == typeof(MessagePackSerializerOptions) &&
                       parameters[2].ParameterType == typeof(CancellationToken);
            })
            .FirstOrDefault();

        if (deserializeMethod == null)
            throw new InvalidOperationException(
                "Could not find MessagePackSerializer.Deserialize<T>(ReadOnlyMemory<byte>, MessagePackSerializerOptions, CancellationToken) method. " +
                "This may indicate an incompatible version of MessagePack. Expected signature: T Deserialize<T>(ReadOnlyMemory<byte>, MessagePackSerializerOptions, CancellationToken).");

        // Create the generic method for this specific message type
        MethodInfo genericMethod = deserializeMethod.MakeGenericMethod(messageType);

        // Build an expression tree: (data) => (INetworkMessage)MessagePackSerializer.Deserialize<T>(data, null, default)
        ParameterExpression dataParam = Expression.Parameter(typeof(ReadOnlyMemory<byte>), "data");
        ConstantExpression nullOptions = Expression.Constant(null, typeof(MessagePackSerializerOptions));
        DefaultExpression defaultToken = Expression.Default(typeof(CancellationToken));

        MethodCallExpression methodCall = Expression.Call(genericMethod, dataParam, nullOptions, defaultToken);
        UnaryExpression castToInterface = Expression.Convert(methodCall, typeof(INetworkMessage));

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

        if (!_messageRegistry.TryGetValue(header.Type, out Func<ReadOnlyMemory<byte>, INetworkMessage>? deserializer))
            throw new UnknownMessageTypeException(header.Type);

        return deserializer(data);
    }
}
