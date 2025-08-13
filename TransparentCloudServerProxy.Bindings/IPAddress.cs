using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace TransparentCloudServerProxy.Bindings;

[NativeMarshalling(typeof(Marshaller))]
[CollectionBuilder(typeof(IpAddress), nameof(FromSpan))]
public readonly record struct IpAddress : IEnumerable<byte>
{
    public static IpAddress Loopback = [127, 0, 0, 1];

    public ulong Address { get; init; }

    public IpAddress(ulong address)
    {
        Address = address;
    }

    public IpAddress(ReadOnlySpan<byte> address)
    {
        if (address.Length != sizeof(uint))
        {
            throw new NotSupportedException();
        }

        Address = MemoryMarshal.Read<uint>(address);
    }

    public static IpAddress FromSpan(ReadOnlySpan<byte> address) => new(address);

    public IEnumerator<byte> GetEnumerator()
    {
        var local = Address;
        var span = MemoryMarshal.CreateSpan(ref Unsafe.As<ulong, byte>(ref local), sizeof(ulong));
        span.Reverse();

        for (var i = 0; i < sizeof(ulong); i++)
        {
            yield return (byte)local;
            local >>= 1;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator ulong(IpAddress address) => address.Address;

    public static implicit operator IpAddress(ulong address) => new(address);

    [CustomMarshaller(typeof(IpAddress), MarshalMode.Default, typeof(Marshaller))]
    private static class Marshaller
    {
        public static ulong ConvertToUnmanaged(IpAddress managed) => managed.Address;

        public static IpAddress ConvertToManaged(ulong unmanaged) => new(unmanaged);
    }
}