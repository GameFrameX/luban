// fork 的 cs-bin / cs-simple-json / cs-litjson 系列模板生成代码引用 LuBan.Runtime 命名空间
// （ByteBuf / BeanBase）；RuntimeSupport 的实现在 Luban 命名空间，此处以派生/转发类型适配。

namespace LuBan.Runtime;

public interface ITypeId
{
    int GetTypeId();
}

public abstract class BeanBase : ITypeId
{
    public abstract int GetTypeId();
}

public class SerializationException : global::Luban.SerializationException
{
    public SerializationException() { }
    public SerializationException(string message) : base(message)
    {
    }
}

public sealed class ByteBuf : global::Luban.ByteBuf
{
    public ByteBuf() : base() { }

    public ByteBuf(int capacity) : base(capacity) { }

    public ByteBuf(byte[] bytes) : base(bytes) { }

    public ByteBuf(byte[] bytes, int readIndex, int writeIndex) : base(bytes, readIndex, writeIndex) { }
}
