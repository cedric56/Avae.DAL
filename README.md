# Avae.DAL

Lightweight .NET data-access building blocks around **Dommel/Dapper**, with optional database and transport feature modules.

> **Status:** preview (1.0.0-preview.1). APIs may change.

## Current core API

The repository currently contains these core building blocks:

| Area | Description |
|---|---|
| IDBFactory / DBFactory<T> | Creates database connections and stores monitor/session registrations |
| IEntityMapper | Dommel-backed entity selection and paging helpers |
| DommelEntityMapper | Default IEntityMapper implementation |
| IDbTransaction<T> | Contract for async save/remove operations |
| EntityValidator | DataAnnotations validation helpers |
| Feature modules | SQLite, PostgreSQL, SQL table dependency, SignalR, and MagicOnion |

The repository does **not** currently contain the older IDBLayer / DBLayer / DBBase / DBTransactional API referenced by older documentation.

## Target framework

The core project targets **.NET 10**.

## Install

    <PackageReference Include="Avae.DAL" Version="1.0.0-preview.1" />

Optional features are enabled through the AvaeFeatures MSBuild property:

    <PropertyGroup>
      <AvaeFeatures>;Sqlite;SignalR;</AvaeFeatures>
    </PropertyGroup>

Available feature flags:

| Feature flag | Adds |
|---|---|
| Sqlite | SQLite helpers and Microsoft.Data.Sqlite |
| PostgreSQL | PostgreSQL helpers and Npgsql |
| SqlTableDependency | SQL Server table-dependency support |
| SignalR | SignalR change notifications |
| MagicOnion | MagicOnion abstractions |
| MagicClient | MagicOnion client / gRPC Web / WebSocket bridge |
| MagicServer | MagicOnion server / ASP.NET Core gRPC |

Feature source files are injected into consuming projects by build/Avae.DAL.targets.

## Basic connection factory

    using Avae.DAL;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.DependencyInjection;

    var services = new ServiceCollection();

    services.UseFactory<SqliteConnection>("Data Source=app.db");

    var provider = services.BuildServiceProvider();
    var factory = provider.GetRequiredService<IDBFactory>();

    using var connection = factory.CreateConnection();
    connection.Open();

## Entity mapper

    using Avae.DAL;
    using Microsoft.Extensions.DependencyInjection;

    services.AddSingleton<IEntityMapper>(
        sp => new DommelEntityMapper(
            sp.GetRequiredService<IDBFactory>()));

    var provider = services.BuildServiceProvider();
    var mapper = provider.GetRequiredService<IEntityMapper>();

    using var connection = mapper.CreateConnection();
    connection.Open();

    var people = await mapper.SelectAsync<Person>(
        connection,
        person => person.FirstName == "Ada");

DommelEntityMapper also exposes synchronous/asynchronous first-item selection and paged queries.

## Architecture notes

- DBFactory<TDbConnection> creates a new provider connection for each request.
- DommelEntityMapper delegates entity selection to Dommel.
- DBFactory also owns mutable monitor/session collections used by the optional real-time integrations.
- MagicOnion exposes remote CRUD/query operations and raw SQL execution.
- SignalR and MagicOnion streaming hubs can propagate database change notifications.

## Project layout

    Avae.DAL.csproj
    Extensions.cs
    EntityValidator.cs

    Implementations/      DB factory, logging connection/command, monitor, records
    Interfaces/           DB factory, monitor, identity and entity-mapper contracts

    Sqlite/               SQLite feature source
    PostgreSQL/           PostgreSQL feature source
    SqlDependency/        SQL table-dependency feature source
    SignalR/              SignalR feature source
    MagicOnion/           MagicOnion client-independent feature source
    MagicOnionClient/     MagicOnion client feature source
    MagicOnionServer/     MagicOnion server feature source
    build/                MSBuild feature-selection targets

## Development

    dotnet restore
    dotnet build
    dotnet pack

The repository currently has no dedicated test project. A local clone/build could not be completed during this audit because the execution environment could not resolve github.com; the findings below are therefore source-level findings.

## Audit findings

The following issues were identified during the maintainer review:

1. [TLS certificate callback accepts any certificate](https://github.com/cedric56/Avae.DAL/issues/1)
2. [SQLite commit hook replays old records and retains them indefinitely](https://github.com/cedric56/Avae.DAL/issues/2)
3. [MagicOnion client uses a static disconnect delegate shared by all instances](https://github.com/cedric56/Avae.DAL/issues/3)
4. [EntityHandler ignores commandTimeout for Dommel operations](https://github.com/cedric56/Avae.DAL/issues/4)
5. [DBFactory Sessions and Monitors are unsynchronized mutable collections](https://github.com/cedric56/Avae.DAL/issues/5)
6. [EntityHandler.Handlers is a global mutable static registry](https://github.com/cedric56/Avae.DAL/issues/6)
7. [ConnectionTracker uses async void for notification dispatch](https://github.com/cedric56/Avae.DAL/issues/7)
8. [MagicOnionLayer ignores buffered and transaction parameters on remote query APIs](https://github.com/cedric56/Avae.DAL/issues/8)
9. [README documents APIs that are not present in the current repository tree](https://github.com/cedric56/Avae.DAL/issues/9)

## License

See [LICENSE.txt](LICENSE.txt).
