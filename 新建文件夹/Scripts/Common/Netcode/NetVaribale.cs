using Unity.Netcode;

/// <summary>
/// �������
/// </summary>
/// <typeparam name="T"></typeparam>
public class NetVaribale<T> : NetworkVariable<T>
{
    /// <summary>
    /// Constructor for <see cref="NetworkVariable{T}"/>
    /// </summary>
    /// <param name="value">initial value set that is of type T</param>
    /// <param name="readPerm">the <see cref="NetworkVariableReadPermission"/> for this <see cref="NetworkVariable{T}"/></param>
    /// <param name="writePerm">the <see cref="NetworkVariableWritePermission"/> for this <see cref="NetworkVariable{T}"/></param>
    public NetVaribale(T value = default,
        NetworkVariableReadPermission readPerm = DefaultReadPerm,
        NetworkVariableWritePermission writePerm = DefaultWritePerm)
        : base(value, readPerm, writePerm)
    {

    }

    public override bool IsDirty()
    {
        //����Ŀ�пͻ���û���޸����������Ȩ��������ֱ�ӹ����ұ���NetworkVariableSerialization<T>.AreEqualΪnull�����
        if (NetworkVariableSerialization<T>.AreEqual == null)
        {
            return false;
        }
        return base.IsDirty();
    }

}
