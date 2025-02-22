FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /

COPY . ./

RUN dotnet restore
RUN dotnet publish --no-restore -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /App

COPY --from=build /out .
RUN mkdir -p /App/downloads/

HEALTHCHECK CMD curl --fail http://localhost:8080/healthz || exit

ENTRYPOINT ["dotnet", "Natech.Caas.API.dll"]