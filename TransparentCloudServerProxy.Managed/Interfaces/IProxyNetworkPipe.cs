namespace TransparentCloudServerProxy.Managed.Interfaces {
    public interface IProxyNetworkPipe {
        TimeSpan Latency { get; }
        void Dispose();
        void Start();
        void Stop();
        string ToString();
    }
}