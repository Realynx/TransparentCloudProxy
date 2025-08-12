using Microsoft.AspNetCore.Mvc;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class AdminApiController : ControllerBase {
        private readonly ILogger<AdminApiController> _logger;

        public AdminApiController(ILogger<AdminApiController> logger) {
            _logger = logger;
        }
    }
}
