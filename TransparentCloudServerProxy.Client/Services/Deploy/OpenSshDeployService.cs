using Renci.SshNet;

using System;

namespace TransparentCloudServerProxy.Client.Services.Deploy {
    public class OpenSshDeployService {
        public OpenSshDeployService() {

        }

        public void DeployServer() {
            using var client = new SshClient("sftp.foo.com", "guest", new PrivateKeyFile("path/to/my/key"));
            client.Connect();

            using var cmd = client.RunCommand("wget https://github.com/Realynx/TransparentCloudProxy/releases/latest/download/Server-linux-x64.zip");
            Console.WriteLine(cmd.Result);
        }
    }
}
