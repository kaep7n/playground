using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Threading.Tasks;

namespace Playground.Docker.Local
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"))
                   .CreateClient();

            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { Limit = 10 });

            foreach (var container in containers)
            {
                Console.WriteLine(container.ID);
            }
        }
    }
}
