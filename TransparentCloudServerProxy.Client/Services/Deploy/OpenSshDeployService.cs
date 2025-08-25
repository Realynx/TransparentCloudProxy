using Renci.SshNet;

using System;

namespace TransparentCloudServerProxy.Client.Services.Deploy {
    public class OpenSshDeployService {
        public OpenSshDeployService() {

        }

        public void DeployServer() {
            using var client = new SshClient("sftp.foo.com", "guest", new PrivateKeyFile("path/to/my/key"));
            client.Connect();

            using var cmd = client.RunCommand("echo 'Hello World!'");
            Console.WriteLine(cmd.Result);
        }
    }
}
