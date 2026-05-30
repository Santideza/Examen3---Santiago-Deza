FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TareasAPI/TareasAPI.csproj TareasAPI/
RUN dotnet restore TareasAPI/TareasAPI.csproj

COPY TareasAPI/ TareasAPI/
RUN dotnet publish TareasAPI/TareasAPI.csproj -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN apt-get update && apt-get install -y libgomp1 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TareasAPI.dll"]
