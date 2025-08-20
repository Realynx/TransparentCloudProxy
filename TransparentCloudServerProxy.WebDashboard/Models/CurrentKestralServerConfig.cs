using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace TransparentCloudServerProxy.WebDashboard.Models
{
    public class CurrentKestralServerConfig : IHostedService
    {
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _lifetime;

        public int HttpsPort { get; private set; }
        public int HttpPort { get; private set; }

        public CurrentKestralServerConfig(IServer server, IHostApplicationLifetime lifetime)
        {
            _server = server;
            _lifetime = lifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(GetPorts);
            return Task.CompletedTask;
        }

        private void GetPorts()
        {
            var addresses = _server.Features.Get<IServerAddressesFeature>()?.Addresses;
            if (addresses == null) return;

            foreach (var address in addresses)
            {
                if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
                {
                    if (uri.Scheme == Uri.UriSchemeHttp)
                        HttpPort = uri.Port;
                    else if (uri.Scheme == Uri.UriSchemeHttps)
                        HttpsPort = uri.Port;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
