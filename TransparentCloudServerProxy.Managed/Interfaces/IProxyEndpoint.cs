using TransparentCloudServerProxy.Managed.Models;

namespace TransparentCloudServerProxy.Managed.Interfaces {
    public interface IProxyEndpoint {
        ManagedProxyEntry ManagedProxyEntry { get; }

        void Dispose();
        double GetAverageDelayNanoSecond();
        void Start();
        void Stop();
        string ToString();
    }
}