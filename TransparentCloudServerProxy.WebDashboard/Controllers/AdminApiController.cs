using Microsoft.AspNetCore.Mvc;

using TransparentCloudServerProxy.Managed;
using TransparentCloudServerProxy.WebDashboard.Services;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class AdminApiController : ControllerBase {
        private readonly ILogger<AdminApiController> _logger;
        private readonly IProxyService _proxyService;

        public AdminApiController(ILogger<AdminApiController> logger, IProxyService proxyService) {
            _logger = logger;
            _proxyService = proxyService;
        }

        [HttpPost]
        public string CreateOrUpdate(object proxyEndpoint) {
            return "";
        }

        [HttpPost]
        public string StartAllProxies() {
            try {
                _proxyService.StartAllProxies();
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost]
        public string StartProxy(ManagedProxyEntry managedProxyEntry) {
            try {

                _proxyService.StartProxy(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost]
        public string StopProxy(ManagedProxyEntry managedProxyEntry) {
            try {

                _proxyService.StopProxy(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost]
        public string AddProxy(ManagedProxyEntry managedProxyEntry) {
            try {

                _proxyService.AddProxyEntry(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }

        [HttpPost]
        public string RemoveProxy(ManagedProxyEntry managedProxyEntry) {
            try {

                _proxyService.StopProxy(managedProxyEntry);
            }
            catch (Exception e) {
                return e.ToString();
            }

            return string.Empty;
        }
    }
}
