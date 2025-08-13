using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace TransparentCloudServerProxy.Bindings;

[NativeMarshalling(typeof(Marshaller))]
public readonly record struct IpAddress
{
    public static IpAddress Loopback = new([127, 0, 0, 1]);

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

    public static implicit operator ulong(IpAddress address) => address.Address;
    public static implicit operator IpAddress(ulong address) => new(address);

    [CustomMarshaller(typeof(IpAddress), MarshalMode.Default, typeof(Marshaller))]
    private static class Marshaller
    {
        public static ulong ConvertToUnmanaged(IpAddress managed) => managed.Address;

        public static IpAddress ConvertToManaged(ulong unmanaged) => new(unmanaged);
    }
}