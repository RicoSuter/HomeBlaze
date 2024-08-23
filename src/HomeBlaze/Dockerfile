# Stage 1: Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build stage
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "HomeBlaze/HomeBlaze.csproj"
COPY . .
WORKDIR "/src/HomeBlaze"
RUN dotnet build "HomeBlaze.csproj" -c Release -o /app/build

# Stage 3: Publish stage
FROM build AS publish
RUN dotnet publish "HomeBlaze.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update -qq && apt-get install ffmpeg -y

ENTRYPOINT ["dotnet", "HomeBlaze.dll"]

# To build for multiple architectures:
# docker buildx build --platform linux/amd64,linux/386,linux/arm64,linux/arm/v7 -t yourusername/yourappname:latest --push .