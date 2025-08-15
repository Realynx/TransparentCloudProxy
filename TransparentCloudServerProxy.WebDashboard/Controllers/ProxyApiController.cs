using Microsoft.AspNetCore.Mvc;

using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.ProxyBackend.Interfaces;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class ProxyApiController : ControllerBase {
        private readonly ILogger<ProxyApiController> _logger;
        private readonly IProxyService _proxyService;

        public ProxyApiController(ILogger<ProxyApiController> logger, IProxyService proxyService) {
            _logger = logger;
            _proxyService = proxyService;
        }

        [HttpGet(nameof(GetProxies))]
        public IProxy[] GetProxies() {
            try {
                return _proxyService.GetProxies();
            }
            catch (Exception e) {
            }

            return null;
        }

        [HttpPost(nameof(StartAllProxies))]
        public string StartAllProxies() {
            try {
                _proxyService.StartAllProxies();
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost(nameof(StartProxy))]
        public string StartProxy([FromBody] Proxy managedProxyEntry) {
            try {
                _proxyService.StartProxy(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost(nameof(StopProxy))]
        public string StopProxy([FromBody] Proxy managedProxyEntry) {
            Console.WriteLine(managedProxyEntry);

            try {
                _proxyService.StopProxy(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost(nameof(AddProxy))]
        public string AddProxy([FromBody] Proxy managedProxyEntry) {
            try {
                _proxyService.AddProxyEntry(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost(nameof(RemoveProxy))]
        public string RemoveProxy([FromBody] Proxy managedProxyEntry) {
            try {
                _proxyService.RemoveProxyEntry(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }
    }
}
