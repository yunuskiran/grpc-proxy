using Google.Protobuf;
using System;

namespace GrpcProxy;

public static class Serializer<T>
{
    public static byte[] Serialize(T message) =>
        message is IMessage protobufMessage
            ? protobufMessage.ToByteArray()
            : throw new ArgumentException($"Message must implement the IMessage interface: {typeof(T).FullName}");

    public static T Deserialize(byte[] bytes)
    {
        if (Activator.CreateInstance(typeof(T)) is not IMessage protobufMessage)
            throw new ArgumentException($"Message must implement the IMessage interface: {typeof(T).FullName}");
        protobufMessage.MergeFrom(bytes);
        return (T)protobufMessage;
    }
}