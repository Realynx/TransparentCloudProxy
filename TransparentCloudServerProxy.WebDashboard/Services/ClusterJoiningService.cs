using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.WebDashboard.SqlDb;
using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.Services {
    public class ClusterJoiningService {
        private readonly IDbContextFactory<WebDashboardDbContext> _dbContextFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public ClusterJoiningService(IDbContextFactory<WebDashboardDbContext> dbContextFactory, IHttpClientFactory httpClientFactory) {
            _dbContextFactory = dbContextFactory;
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnboardServer(Uri remoteServer, string adminCredential) {
            using var httpClient = _httpClientFactory.CreateClient();

            // httpClient.GetAsync();
        }

        public async Task JoinCluster(string associationKey) {
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.AssociatedServers.RemoveRange(dbContext.AssociatedServers.ToArray());

            /*
             Plan:
                Admin-Client: Get AssociationKey from cluster cluster-host(The authoritive server).
                
                [Must have admin]
                Cluster-Host: Generate a new AssociationKey and send it as response(Keys are only valid for 1 day or 1 use.)

                Admin-Client: Sends via "AdminApi/JoinCluster", The AssociationKey just fetched from cluser-host to a server you wish to join the cluster.

                [Must Have Admin]
                Server: Parse AssociationKey, extract Credential, and ServerAddress (Credential is only valid for "AdminApi/AcceptServer")
                Server: Generate new Credential and ProxyUser called "cluster-host", assign as admin
                Server: Send AssociatedServer to "AdminApi/AcceptServer" on the cluster-host, AssociatedServer:RootCredential will be the credential we generated from The ProxyUser called "cluster-host"

                [Can only be called with the AssociationKey]
                Cluster-Host: Accept a server, add it to the AssociatedServer database table
                Cluster-Host: RemoveAssociationKet after it's use
                Cluster-Host: generate AssociatedServer for self and send it as a success response.

                Server: 
                        -On Success Response:
                            Server: Clears existing AssociatedServer entries in DB.
                            Server: Add the AssociatedServer returned in our accept server api response.
                            Server: Save DB Changes.
                            Server: Return Ok Response for the original AdminApi/JoinCluster Call

                        -On Fail Response:
                            Server: Discord changes pending for the database, this will cause "cluster-host" ProxyUser to not be created.
                            Server: Return ServerError Response for the original AdminApi/JoinCluster Call
             */

            var associatedServer = new AssociatedServer() {
                RootCredential = associationKey
            };


            dbContext.AssociatedServers.Add(associatedServer);
        }
    }
}
