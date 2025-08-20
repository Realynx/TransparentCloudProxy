using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Controllers.Cluster {
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "ClusterToken")]
    public class ClusterController : ControllerBase {
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;

        public ClusterController(IDbContextFactory<WebDashboardDbContext> dbContextFactory) {
            _dbContextFactory = dbContextFactory;
        }
    }
}
