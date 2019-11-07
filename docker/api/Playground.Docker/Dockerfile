FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Playground.Docker.csproj", ""]
RUN dotnet restore "./Playground.Docker.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Playground.Docker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Playground.Docker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
VOLUME .:/app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Playground.Docker.dll"]