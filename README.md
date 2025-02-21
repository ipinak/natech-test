
# Database

## Generate migration script

```
dotnet ef migrations add InitialCreate --project ../Natech.Caas.Database --startup-project .
```

## Apply migrations

```
dotnet ef migrations update --project ../Natech.Caas.Database --startup-project .
```