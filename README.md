
# Database

## Generate migration script

```shell
dotnet ef migrations add InitialCreate --project ../Natech.Caas.Database --startup-project .
```

## Apply migrations

```shell
dotnet ef migrations update --project ../Natech.Caas.Database --startup-project .
```

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