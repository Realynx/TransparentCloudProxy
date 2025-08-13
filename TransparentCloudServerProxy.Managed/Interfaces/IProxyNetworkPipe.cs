namespace TransparentCloudServerProxy.Managed.Interfaces {
    public interface IProxyNetworkPipe : IDisposable {
        TimeSpan Latency { get; }

        void Start();
        void Stop();
        string ToString();
    }
}