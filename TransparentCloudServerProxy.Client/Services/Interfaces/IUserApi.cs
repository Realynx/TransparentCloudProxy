using System;
using System.Threading.Tasks;

using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.Client.Services.Interfaces {
    public interface IUserApi {
        Task<ProxyUser?> Login(Uri endpoint, string credential);
    }
}