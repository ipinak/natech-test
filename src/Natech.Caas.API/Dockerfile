# Use the official .NET 8 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application
COPY . ./

# Build the application
RUN dotnet restore --force && \
  dotnet publish -c Release -o /out --no-restore

# Use the .NET 8 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the built application from the previous stage
COPY --from=build /out .

# Expose the API port (default ASP.NET Core port)
EXPOSE 8080

# Set the entry point for the container
ENTRYPOINT ["dotnet", "Natech.Caas.API.dll"]
