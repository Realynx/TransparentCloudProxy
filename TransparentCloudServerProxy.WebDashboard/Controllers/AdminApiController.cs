using Microsoft.AspNetCore.Mvc;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class AdminApiController : ControllerBase {
        private readonly ILogger<AdminApiController> _logger;

        public AdminApiController(ILogger<AdminApiController> logger) {
            _logger = logger;
        }

        [HttpGet(nameof(GetAssociationKey))]
        public IActionResult GetAssociationKey() {
            return Ok("key");
        }

        [HttpPost(nameof(JoinCluster))]
        public IActionResult JoinCluster([FromBody] string associationKey) {
            return Ok(associationKey);
        }
    }
}
