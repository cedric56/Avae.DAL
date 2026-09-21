# Avae.DAL

Lightweight data-access layer for .NET, built around **Dapper** / **Dapper.Contrib**, with optional backends selected via MSBuild feature flags.
> **Status:** preview (`1.0.0-preview.1`). APIs may change.

---

## What it provides

| Area | Description |
|------|-------------|
| **IDBLayer** | CRUD helpers (`Get`, `GetAll`, `Where`, `FindByAny`, `Execute`, multi-map `Query`) |
| **IDBFactory** | Connection factory abstraction |
| **DBTransactional** | Save / remove unit-of-work style operations |
| **DBBase** | Optional static accessor after `Initialize` |
| **Feature modules** | Sqlite, PostgreSQL, SqlTableDependency, SignalR, MagicOnion (client/server) |

Core package depends on: `Dapper.Contrib`, `MessagePack`, `Microsoft.Extensions.DependencyInjection.Abstractions`, logging abstractions.

---

## Install

```xml
<PackageReference Include="Avae.DAL" Version="1.0.0-preview.1" />

<PropertyGroup>
  <!-- opt into only what you need -->
  <AvaeFeatures>;Sqlite;</AvaeFeatures>
  <!-- examples: ;PostgreSQL; ;SignalR; ;MagicClient; ;MagicServer; ;SqlTableDependency; -->
</PropertyGroup>
```

Features are activated by `build/Avae.DAL.targets` (package path `buildtransitive`): matching sources under `lib/net10.0/<Feature>/` are compiled into the consuming project and the related NuGet dependencies are added.

| Feature flag | Adds |
|--------------|------|
| `Sqlite` | `Microsoft.Data.Sqlite` + Sqlite helpers |
| `PostgreSQL` | Npgsql EF Core package + Postgres helpers |
| `SqlTableDependency` | SqlTableDependencyCore |
| `SignalR` | SignalR client/core + MessagePack protocol |
| `MagicOnion` | MagicOnion.Abstractions |
| `MagicClient` | MagicOnion.Client, gRPC Web / WebSocket bridge |
| `MagicServer` | MagicOnion.Server, Grpc.AspNetCore |

Target framework: **net10.0**.

---

## Quick start (Sqlite)

```csharp
using Avae.DAL;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.UseFactory<SqliteConnection>("Data Source=app.db");
services.UseLayer(
    sp => new DBLayer(sp.GetRequiredService<IDBFactory>()),
    getDBCreateCommand: () => """
        CREATE TABLE IF NOT EXISTS Person (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            FirstName TEXT,
            LastName TEXT
        );
        """);

var provider = services.BuildServiceProvider();
DBBase.Initialize(provider.GetRequiredService<IDBLayer>());

var layer = DBBase.Instance;
var people = await layer.GetAllAsync<Person>();
```

### Filtering

```csharp
// AND
var rows = await layer.WhereAsync<Person>(("FirstName", "Ada"), ("LastName", "Lovelace"));

// OR
var any = await layer.FindByAnyAsync<Person>(("FirstName", "Ada"), ("FirstName", "Grace"));
```

Column names in filters must match entity property / table column names. Values are parameterized; **column names are not** (see known issues).

### Raw SQL / multi-map

```csharp
var result = layer.Query<Contact, Person, Contact>(
    """
    SELECT C.Id AS ContactId, C.IdPerson, P.Id, P.FirstName, P.LastName
    FROM Contact C
    INNER JOIN Person P ON C.IdPerson = P.Id
    WHERE C.Id = @Id
    """,
    (c, p) => { c.Person = p; return c; },
    new { Id = id },
    aliases: [new DBAlias("ContactId", "Id")]);
```

---

## Architecture notes

- **DBLayer** opens a connection per call via `IDBFactory.CreateConnection()`.
- **Dapper.Contrib** is used for `Get` / `GetAll` (table name = type name by default).
- **DBBase** is a process-wide singleton holder; prefer injecting `IDBLayer` in new code.
- Optional real-time paths: SignalR hubs, MagicOnion record hubs, SQL table dependency.

---

## Project layout

```
Implementations/     DBLayer, DBFactory, DBTransactional, logging wrappers
Interfaces/          IDBLayer, IDBFactory, IDBMonitor, …
Sqlite/              feature sources
PostgreSQL/
SqlDependency/
SignalR/
MagicOnion/ MagicOnionClient/ MagicOnionServer/
MessagePack/         transactional formatters
build/Avae.DAL.targets
```

---

## License

See [LICENSE.txt](LICENSE.txt).

---

## Known limitations

See the companion notes in the repository issues / maintainer review: async connection disposal, ignored `aliases` / `commandTimeout` on some paths, filter SQL construction, and static `DBBase` / `Sessions` state.
