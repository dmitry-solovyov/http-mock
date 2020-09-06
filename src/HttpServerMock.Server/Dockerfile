#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["HttpServerMock.Server/HttpServerMock.Server.csproj", "HttpServerMock.Server/"]
RUN dotnet restore "HttpServerMock.Server/HttpServerMock.Server.csproj"
COPY . .
WORKDIR "/src/HttpServerMock.Server"
RUN dotnet build "HttpServerMock.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HttpServerMock.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HttpServerMock.Server.dll"]