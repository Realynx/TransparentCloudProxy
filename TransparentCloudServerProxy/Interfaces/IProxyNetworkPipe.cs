namespace TransparentCloudServerProxy.Interfaces {
    public interface IProxyNetworkPipe {
        TimeSpan Latency { get; }
        void Dispose();
        void Start();
        void Stop();
    }
}