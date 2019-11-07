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

            var contentsDir = Directory.CreateDirectory("contents");

            var dockerfilePath = Path.Combine("contents", "Dockerfile");

            File.WriteAllText(dockerfilePath, sb.ToString());

            if (!File.Exists("contents/Playground.Docker.Hello.dll"))
            {
                File.Copy("Playground.Docker.Hello.dll", Path.Combine("contents", "Playground.Docker.Hello.dll"));
            }

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

            var extractedDir = Directory.CreateDirectory("extracted");

            using (var sourceStream = new GZipInputStream(File.OpenRead(tarArchiveName)))
            {
                using (var t = TarArchive.CreateInputTarArchive(sourceStream, TarBuffer.DefaultBlockFactor))
                {
                    t.ExtractContents(extractedDir.FullName);
                }
            }

            var files = extractedDir.GetFiles();

            using var contentsStream = File.OpenRead(tarArchiveName);

            var image = await client.Images.BuildImageFromDockerfileAsync(contentsStream, new ImageBuildParameters
            {
            });

            var read = -1;

            using var imageStream = File.OpenWrite("image.tar.gz");
            {
                do
                {
                    var b = new byte[1024];

                    read = image.Read(b, 0, 1024);

                    imageStream.Write(b, 0, b.Length);
                } while (read > 0);
                imageStream.Close();
            }

            using var imageReadStream = File.OpenRead("image.tar.gz");
            {
                await client.Images.CreateImageAsync(new ImagesCreateParameters
                {
                }, imageReadStream, null, null);
            }

            var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                
            });
        }
    }
}
