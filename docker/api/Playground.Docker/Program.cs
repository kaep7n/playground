using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Playground.Docker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new DockerClientConfiguration(new Uri("http://host.docker.internal:2375"))
                .CreateClient();
            
            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters(){Limit = 10});
            foreach (var container in containers)
            {
                Console.WriteLine(container.ID);
            }

            var sb = new StringBuilder();

            sb.AppendLine("FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim");
            sb.AppendLine("ARG foo");
            sb.AppendLine("COPY . .");
            sb.AppendLine("RUN dotnet --version");

            var files = Directory.GetFiles(Directory.GetCurrentDirectory());

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));

            var image = await client.Images.BuildImageFromDockerfileAsync(memoryStream, new ImageBuildParameters
            {
            });
        }
    }
}
