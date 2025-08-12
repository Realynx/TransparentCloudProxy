using Microsoft.AspNetCore.Mvc;

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
    }
}
