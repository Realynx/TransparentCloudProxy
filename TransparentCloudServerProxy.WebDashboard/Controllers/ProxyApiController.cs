using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "KeyToken")]
    public class ProxyApiController : ControllerBase {
        private readonly ILogger<ProxyApiController> _logger;
        private readonly IProxyService _proxyService;

        public ProxyApiController(ILogger<ProxyApiController> logger, IProxyService proxyService) {
            _logger = logger;
            _proxyService = proxyService;
        }

        private static IActionResult BadRequestError(Exception e) {
            return new ContentResult() {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentType = "application/json",
                Content = e.Message.ToString()
            };
        }

        [HttpGet(nameof(GetProxies))]
        public IActionResult GetProxies() {
            try {
                return Ok(_proxyService.GetProxies());
            }
            catch (Exception e) {
                return BadRequestError(e);
            }
        }

        [HttpPost(nameof(StartAllProxies))]
        public IActionResult StartAllProxies() {
            try {
                _proxyService.StartAllProxies();
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();
        }

        [HttpPost(nameof(StartProxy))]
        public IActionResult StartProxy([FromBody] Proxy proxy) {
            try {
                _proxyService.StartProxy(proxy);
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();
        }

        [HttpPost(nameof(StopProxy))]
        public IActionResult StopProxy([FromBody] Proxy proxy) {
            Console.WriteLine(proxy);

            try {
                _proxyService.StopProxy(proxy);
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();

        }

        [HttpPost(nameof(AddProxy))]
        public IActionResult AddProxy([FromBody] Proxy proxy) {
            try {
                _proxyService.AddProxyEntry(proxy);
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();
        }

        [HttpPost(nameof(RemoveProxy))]
        public IActionResult RemoveProxy([FromBody] Proxy proxy) {
            try {
                _proxyService.RemoveProxyEntry(proxy);
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();
        }
    }
}
