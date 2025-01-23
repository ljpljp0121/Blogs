using System;
using Unity.Netcode;

/// <summary>
/// ����������л��ͷ����л��İ���
/// </summary>
public static class NetworkVaribaleSerializationBinder
{
    public static void Init()
    {
        BindUserNetworkVaribaleSerialization<PlayerState>();

    }

    public static void BindUserNetworkVaribaleSerialization<T>() where T : unmanaged, Enum
    {
        UserNetworkVariableSerialization<T>.WriteValue = (FastBufferWriter writer, in T value) =>
        {
            writer.WriteValueSafe(value);
        };

        UserNetworkVariableSerialization<T>.ReadValue = (FastBufferReader reader, out T value) =>
        {
            reader.ReadValueSafe(out value);
        };

        UserNetworkVariableSerialization<T>.DuplicateValue = (in T value, ref T duplicateValue) =>
        {
            duplicateValue = value;
        };
    }


}
