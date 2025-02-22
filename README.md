# Natech Test

This is a simple aps.net core using .net 8.

Follow the instructions below on how to run and execute the tests.

## Project Structure

| Project | Description|
|-----|-----------|
| Natech.Caas.API  | Contains everything related to the Web API, i.e. validators, requests and Main(). This package is combines all the other projects and it's the starting point  |
| Natech.Caas.Core  | Contains services that hold the main functionality of the project. It uses the `Natech.Caas.Database` project |
| Natech.Caas.Dtos  | Data Transfer Objects for `Natech.Caas.API` |
| Natech.Caas.Shared  | Shared code between all projects |
| Natech.Caas.TheCatApi  | All code related to integrating to thecatapi.com, it includes dtos, a client and config definition |
| Natech.Caas.API.IntegrationTests  | A few integration tests that verify the functionality of the API, using an In-Memory database |
| Natech.Caas.Core.UnitTests  | A few unit tests for the code that has most of the logic i.e. `Natech.Caas.Core` |

# Run

```shell
docker-compose up -d # sets the database
dotnet run --project src/Natech.Caas.API
```

# Test

Runs all tests in the project

```shell
dotnet test
```

# Database

## Generate migration script

```shell
dotnet ef migrations add InitialCreate --project ../Natech.Caas.Database --startup-project .
```

## Apply migrations

```shell
dotnet ef migrations update --project ../Natech.Caas.Database --startup-project .
```

# TODO

- [ ] Use IOptions in CatService
