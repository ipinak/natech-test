# Natech Test

This is a simple asp.net core using .net 8.

# Table Of Contents

- [Description](#description)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [How To Run It](#how-to-run-it)
- [How To Run Tests](#how-to-run-tests)
- [Database](#database)
  - [Generate migrations](#generate-migration-script)
  - [Apply migrations](#apply-migrations)

## Description

The web api "steals" 25 images from thecatapi.com and saves them
locally on your drive. Ideally you would use azure blob storage 
(or something similar) to ensure you won't have issues with disc 
limitations and the images would be publibcly available. However, 
for the sake of this test and simplicity it's not using any cloud 
service. The images ares **served as static files** and no specific
controller exists.

## Project Structure

| Project | Description |
|---------|-------------|
| Natech.Caas.API | Contains everything related to the Web API, i.e. validators, requests and Main(). This package is combines all the other projects and it's the starting point |
| Natech.Caas.Core | Contains services that hold the main functionality of the project. It uses the `Natech.Caas.Database` project |
| Natech.Caas.Dtos | Data Transfer Objects for `Natech.Caas.API` |
| Natech.Caas.TheCatApi | All code related to integrating to thecatapi.com, it includes dtos, a client and config definition |
| Natech.Caas.API.IntegrationTests | A few integration tests that verify the functionality of the API, using an In-Memory database |
| Natech.Caas.Core.UnitTests | A few unit tests for the code that has most of the logic i.e. `Natech.Caas.Core` |

Starting the project will execute the already created migrations

> NOTE: you will find in the project 2 docker-compose files, I've worked on this on a mac hence the existence of a second file.

## Requirements

- docker-compose
- .net 8

## How To Run It

```shell
docker-compose up -d
dotnet run --project src/Natech.Caas.API
```

## How To Run Tests

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
