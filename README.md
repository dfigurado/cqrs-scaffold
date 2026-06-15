# CQRS-Scaffold

[![NuGet](https://img.shields.io/nuget/v/CqrsScaffold.svg)](https://www.nuget.org/packages/CqrsScaffold)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![C#](https://img.shields.io/badge/C%23-C--Sharp-brightgreen)]

A .NET 8 CLI that generates a Clean Architecture solution with CQRS and MediatR
already wired up. One command gets you a solution file, four layered projects,
a test project, and a working Command/Query pair you can build on.

It's meant to save the half-day of clicking through `dotnet new`, adding the
right packages, and renaming namespaces every time you start a new service.

> **Status:** pre-release. The core generation pipeline is being stabilised
> for `v0.1.0`. See [Roadmap](#roadmap) for what's working and what isn't.

## Install

```bash
dotnet tool install -g CqrsScaffold
```

## Usage

```bash
cqrs-scaffold --name MyApp
```

That's it. You'll get a `MyApp/` folder next to your current directory with a
solution that builds and tests that pass.

## Options

| Flag | Short | Default | What it does |
| ---- | ----- | ------- | ------------ |
| `--name` | `-n` | required | Name used for the solution and root namespace |
| `--output` | `-o` | current dir | Where to put the generated solution |
| `--config` | | none | Load defaults from a JSON file (see below) |
| `--service-bus` | | off | _Roadmap_ — Azure Service Bus publisher/consumer |
| `--auth` | | none | _Roadmap_ — set to `Jwt` to wire up JWT bearer auth |
| `--docker` | | off | _Roadmap_ — Dockerfile + `docker-compose.yml` |
| `--ci` | | none | _Roadmap_ — set to `GitHubActions` for a build workflow |

CLI flags always win over values from `--config`.

## What you get

```text
MyApp/
├── MyApp.sln
├── src/
│   ├── MyApp.Domain/
│   │   └── Entities/Item.cs
│   ├── MyApp.Application/
│   │   ├── Commands/CreateItem/
│   │   │   ├── CreateItemCommand.cs
│   │   │   ├── CreateItemHandler.cs
│   │   │   └── CreateItemValidator.cs
│   │   └── Queries/GetItem/
│   │       ├── GetItemQuery.cs
│   │       └── GetItemHandler.cs
│   ├── MyApp.Infrastructure/
│   │   └── Persistence/ApplicationDbContext.cs
│   └── MyApp.API/
│       └── Program.cs
└── tests/
    └── MyApp.Tests/
        └── CreateItemHandlerTest.cs
```

`Item` is the default entity name. Pass a different one through a config file
(`EntityName`) and every Command, Query, DbSet, and test is renamed to match.

## Config file

If you find yourself typing the same flags over and over, drop them in a JSON
file and pass it with `--config`:

```json
{
  "ProjectName": "MyApp",
  "OutputPath": "./out",
  "EntityName": "Order",
  "IncludeServiceBus": true,
  "AuthType": "Jwt",
  "IncludeDocker": true,
  "CiType": "GitHubActions"
}
```

Anything you also pass on the command line takes priority.

## Roadmap

Tracked on the [project board](https://github.com/dfigurado/cqrs-scaffold/issues).

**`v0.1.0` — Core generation** _(in progress)_
Bug fixes to the template engine and project writer, all nine Scriban templates
producing compilable C#, unit and integration tests covering the pipeline.

**`v0.2.0` — Optional modules** _(not started)_

- `--service-bus` adds `Azure.Messaging.ServiceBus` and a publisher/consumer pair.
- `--auth Jwt` registers JWT bearer auth and an `appsettings.json` placeholder.
- `--docker` produces a multi-stage Dockerfile and Compose file.
- `--ci GitHubActions` adds `.github/workflows/build.yml`.

**`v1.0.0` — Release** _(not started)_
Publish to NuGet.org, expand this README with examples, finalise CI.

## Contributing

- Branch from `main` using `feat/...`, `fix/...`, or `docs/...`.
- Reference the issue in your PR description (`Fixes #16`).
- Make sure `dotnet build` and `dotnet test` are green before asking for review.

## License

MIT. See [LICENSE](LICENSE).
