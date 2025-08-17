using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.ProxyBackend;
using TransparentCloudServerProxy.WebDashboard.Services;
using TransparentCloudServerProxy.WebDashboard.Services.Interfaces;
using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Controllers {
    [ApiController]
    [Route("[controller]")]
    [Authorize(AuthenticationSchemes = "KeyToken")]
    public class ProxyApiController : ControllerBase {
        private readonly ILogger<ProxyApiController> _logger;
        private readonly IProxyService _proxyService;
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;

        public ProxyApiController(ILogger<ProxyApiController> logger, IProxyService proxyService, IDbContextFactory<WebDashboardDbContext> dbContextFactory) {
            _logger = logger;
            _proxyService = proxyService;
            _dbContextFactory = dbContextFactory;
        }

        private static IActionResult BadRequestError(Exception e) {
            return new ContentResult() {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentType = "application/json",
                Content = e.Message.ToString()
            };
        }

        private async Task<ProxyUser?> GetCurrentUserAsync(WebDashboardDbContext dbContext) {
            if (User.Identity is not ClaimsIdentity identity) {
                return null;
            }

            var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var currentUserId)) {
                return null;
            }

            var currentUser = await dbContext.Users
                .Include(u => u.UserSavedProxies)
                .SingleOrDefaultAsync(u => u.Id == currentUserId);

            return currentUser;
        }

        [HttpGet(nameof(GetProxies))]
        public async Task<IActionResult> GetProxies() {
            try {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var currentUser = await GetCurrentUserAsync(dbContext);
                if (currentUser is null) {
                    return BadRequest();
                }

                if (currentUser.Admin) {
                    return Ok(_proxyService.GetProxies());
                }
                else {
                    return Ok(currentUser.UserSavedProxies.Select(i => i.GetProxy()).ToArray());
                }
            }
            catch (Exception e) {
                return BadRequestError(e);
            }
        }

        [HttpPost(nameof(StartAllProxies))]
        public async Task<IActionResult> StartAllProxies() {
            try {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var currentUser = await GetCurrentUserAsync(dbContext);
                if (currentUser is null) {
                    return BadRequest();
                }

                foreach (var savedProxy in currentUser.UserSavedProxies) {
                    var proxy = savedProxy.GetProxy();
                    if (proxy is null) {
                        continue;
                    }

                    _proxyService.StartProxy(proxy);
                }
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

        [HttpPost(nameof(AddOrModifyProxy))]
        public async Task<IActionResult> AddOrModifyProxy([FromBody] Proxy proxy) {
            try {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var currentUser = await GetCurrentUserAsync(dbContext);
                if (currentUser is null) {
                    return BadRequest();
                }

                var savedProxy = new SavedProxy(proxy, currentUser.Id);
                var existingProxy = dbContext.Proxies.Find(savedProxy.Id);
                if (existingProxy is not null) {
                    _proxyService.RemoveProxyEntry(existingProxy.GetProxy()!);
                    _proxyService.AddProxyEntry(proxy);

                    existingProxy.ProxyBase64Json = savedProxy.ProxyBase64Json;
                    dbContext.SaveChanges();
                    return Ok(existingProxy);
                }

                dbContext.Proxies.Add(savedProxy);
                dbContext.SaveChanges();

                _proxyService.AddProxyEntry(proxy);
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();
        }

        [HttpPost(nameof(RemoveProxy))]
        public async Task<IActionResult> RemoveProxy([FromBody] Proxy proxy) {
            try {
                using var dbContext = _dbContextFactory.CreateDbContext();
                var currentUser = await GetCurrentUserAsync(dbContext);
                if (currentUser is null) {
                    return BadRequest();
                }

                var savedProxy = new SavedProxy(proxy, currentUser.Id);
                var existingRule = currentUser.UserSavedProxies.Single(i => i.Id == savedProxy.Id);

                _proxyService.RemoveProxyEntry(proxy);
                currentUser.UserSavedProxies.Remove(existingRule);
                dbContext.SaveChanges();
            }
            catch (Exception e) {
                return BadRequestError(e);
            }

            return Ok();
        }
    }
}
