using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
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

            var sb = new StringBuilder();

            sb.AppendLine("FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim");
            sb.AppendLine("COPY . .");
            sb.AppendLine("ENTRYPOINT [\"dotnet\", \"Playground.Docker.Hello.dll\"]");

            Directory.Delete("contents", true);

            var contentsDir = Directory.CreateDirectory("contents");
            
            var dockerfilePath = Path.Combine("contents", "Dockerfile");

            File.WriteAllText(dockerfilePath, sb.ToString());

            File.Copy("Playground.Docker.Hello.dll", Path.Combine("contents", "Playground.Docker.Hello.dll"));
            File.Copy("Playground.Docker.Hello.runtimeconfig.json", Path.Combine("contents", "Playground.Docker.Hello.runtimeconfig.json"));

            var filesInDirectory = contentsDir.GetFiles();
            var tarArchiveName = @"contents.tar.gz";

            using var targetStream = new GZipOutputStream(File.Create(tarArchiveName));
            {
                using var tarArchive = TarArchive.CreateOutputTarArchive(targetStream);
                {
                    foreach (var fileToBeTarred in filesInDirectory)
                    {
                        var entry = TarEntry.CreateEntryFromFile(fileToBeTarred.FullName);
                        entry.Name = fileToBeTarred.Name;
                        tarArchive.WriteEntry(entry, true);
                    }
                }
            }

            using var contentsStream = File.OpenRead(tarArchiveName);

            var logStream = await client.Images.BuildImageFromDockerfileAsync(contentsStream, new ImageBuildParameters
            {
                Tags = new[] { "playground-hello" }
            });


            var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = "playground-hello"
            });

            var started = await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters
            {
            });
        }
    }
}
