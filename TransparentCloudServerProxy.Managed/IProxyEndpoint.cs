namespace TransparentCloudServerProxy.Managed {
    internal interface IProxyEndpoint {
        ManagedProxyEntry ManagedProxyEntry { get; }

        void Dispose();
        void Start();
        void Stop();
    }
}