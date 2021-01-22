#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Risk.SampleClient/Risk.SampleClient.csproj", "Risk.SampleClient/"]
COPY ["Risk.Shared/Risk.Shared.csproj", "Risk.Shared/"]
RUN dotnet restore "Risk.SampleClient/Risk.SampleClient.csproj"
COPY . .
WORKDIR "/src/Risk.SampleClient"
RUN dotnet build "Risk.SampleClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Risk.SampleClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Risk.SampleClient.dll"]
