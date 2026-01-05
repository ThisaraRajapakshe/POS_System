# 1. Build Phase
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
# Restore dependencies
RUN dotnet restore

# This finds any .csproj file and builds it.
RUN dotnet publish -c Release -o /app

# 2. Run Phase
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Set the port for Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# START THE APP
ENTRYPOINT ["dotnet", "POS_System.dll"]