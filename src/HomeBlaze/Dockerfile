FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "HomeBlaze/HomeBlaze.csproj"
COPY . .
WORKDIR "/src/HomeBlaze"
RUN dotnet build "HomeBlaze.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HomeBlaze.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update -qq && apt-get install ffmpeg -y
ENTRYPOINT ["dotnet", "HomeBlaze.dll"]