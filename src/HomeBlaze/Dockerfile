#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:7.0 AS base-arm64
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "HomeBlaze/HomeBlaze.csproj"
COPY . .
WORKDIR "/src/HomeBlaze"
RUN dotnet build "HomeBlaze.csproj" -c Release -o /app/build

FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/sdk:7.0 AS build-arm64
WORKDIR /src
COPY . .
RUN dotnet restore "HomeBlaze/HomeBlaze.csproj"
COPY . .
WORKDIR "/src/HomeBlaze"
RUN dotnet build "HomeBlaze.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "HomeBlaze.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM build-arm64 AS publish-arm64
RUN dotnet publish "HomeBlaze.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/build .
COPY --from=publish-arm64 /app/build .
ENTRYPOINT ["dotnet", "HomeBlaze.dll"]