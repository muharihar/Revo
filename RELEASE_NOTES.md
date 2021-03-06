# RELEASE NOTES

## [1.14.0] - 2020-??-??

## [1.13.1] - 2020-01-15

### Added
- now it's possible to specify FileSqlDatabaseMigration version in its headers

## [1.13.0] - 2019-11-29

### Added
- database migrations - new own system for managing database schema version migrations (incl. module dependencies)
- standalone database migration tool Revo.Tools.DatabaseMigrator (also invokable as global .NET Core tool revo-dbmigrate)
- support for SQLite in EF Core infrastructure implementation (event store, async event queues, etc.)

### Fixed
- events are now correctly marked as dispatched even when there are no listeners for them
- fixed missing IExternalEventStore registration for EF6 provider
- not resetting DbContexts after EFCoreCoordinatedTransaction finishes

### Changed
- Entity Framework 6 provider updated to EF6.3 version and is now targeting both net472 and netstandard2.1
- Revo.Extensions.Notifications and Revo.Extensions.History now need to be explicitly registered in Revo configuration using AddHistoryExtension and AddNotificationsExtension
- removed IRepository.SaveChangesAsync to avoid confusion - it doesn't work well in certain situations (e.g. with EF Core provider) and IUnitOfWork.CommitAsync should be used instead
- NLog updated to 4.6.7

## [1.12.0] - 2019-10-22

### Added
- new BaseTypeAttribute for EF Core mapping - overrides the mapped entity base type

### Changed
- upgraded to ASP.NET Core 3.0 and Entity Framework Core 3.0
- ReadModelForEntityAttribute no longer overrides mapped table name (leaves default)

### Removed
- dropped OData out-of-the-box integration on ASP.NET & ASP.NET Core platforms altogether (OData is not supported on ASP.NET Core 3.0 now), you have to integrate it by yourself now

## [1.11.0] - 2019-09-03

### Added
- IRepository.FindManyAsync and GetManyAsync with optimized batch-loading of event sourced aggregates

### Changed
- IRepository.FindAllAsync now returns an array instead of IList<T>
- improved StaticClassifierDatabaseInitializer, now also supporting event sourced aggregates
- Cake script now uses VS2019 build toolset

### Fixed
- EF Core's PrefixConvention now correctly prefixes columns defined in abstract classes annotated with TablePrefixAttribute

## [1.10.0] - 2019-06-03

### Added
- IAsyncQueryableResolver.AnyAsync
- injection support for SignalR hubs in Revo.AspNetCore

## [1.9.1] - 2019-05-15

### Added
- IEventNumberVersioned and EventEntityReadModel for read models with additional arbitrary versioning (concurrency control)

### Changed
- now possible to configure ODataQuerySettings for EFCoreQueryableToODataResultConverter
- removed IEventSourcedAggregateRoot constraints from EF Core and EF6 projectors (now can be any IAggregateRoot)

### Fixed
- CrudAggregateStore now automatically removes entities that have been marked with IsDeleted
- FakeRepository now correctly removes entites that have been flagged as IsDeleted

## [1.9.0] - 2019-03-15

### Added
- new Revo.EasyNetQ module for simple integrations with RabbitMQ

## [1.8.0] - 2019-03-04

### Changed
- default EventSerializer and NotificationSerializer now use the same JSON serializer settings

### Fixed
- EF Core's PrefixConvention with deeper inheritance hierarchies
- EF Core sync projectors are invoked not invoked multiple times with the same events

### Removed
- obsolete Globalization stuff (messages, locales, translatable entities, ASP.NET localization helpers...)

## [1.7.0] - 2019-02-14

### Added

- new default in-memory job scheduler

### Changed

- FakeClock now uses AsyncLocal instead of ThreadLocal
- refactored Revo.Extensions.Notifications for .NET Core compatibility and better configuration

## [1.6.0] - 2019-01-17

### Changed

- projects upgraded to .NET Core 2.2, ASP.NET Core 2.2 and Entity Framework Core 2.2

## [1.5.0] - 2019-01-10

### Added
- automatic event projector discovery and registration (now enabled by default)
- event publishing for non-event sourced entities (not transactionally safe and not saving to event store yet, though, so beware)
- coordinated UoW transaction for EF Core including synchronous projections, publishing events from projections and fast event queue dispatching
- EFCoreInMemoryCrudRepository and EFCoreInMemoryDatabaseAccess
- recurring job scheduling support

### Fixed
- optimized event queue dispatching for big amounts of queues
- ODataAsyncResultFilter.DefaultConverter now returns correct counts
- fixed BasicDomainModelConvention when using inheritance including abstract types
- fixed binding of Hangfire and infrastructure configuration sections, added more options

## [1.4.0] - 2018-11-05

### Added
- [#1](https://github.com/revoframework/Revo/issues/1) **ASP.NET Core support** - platform implementation, i.e. user context, security, DI, OData, etc.
- [#2](https://github.com/revoframework/Revo/issues/2), [#3](https://github.com/revoframework/Revo/issues/3) **EF Core support** - data access & infrastructure (async events, sagas, projections...)
- Throttling async event processing to minimize the amount of running tasks and open DB connections (when running event queue catch-ups during app start-up and pseudo-synchronously processing events after a request; see IAsyncEventPipelineConfiguration)

### Changed
- **flattened and simplified package structure** (now provider-centric) - vendor-specific modules were moved
to Providers directory and some of them were merged/renamed:  
  - _Revo.Platforms.AspNet → Revo.AspNet_
  - _Revo.DataAccess.EF6 and Revo.Infrastructure.EF6 → Revo.EF6_  
  - _Revo.DataAccess.RavenDB → Revo.RavenDB_  
  - _Revo.Integrations.Rebus → Revo.Rebus_
- **improved AsyncEventWorker concurrency** - framework did not prevent multiple workers from parallel processing of one async event queue, causing occasional (e.g. when under heavy load) concurrency exceptions 
(which are eventually handled by an automatic retry); now allowing only one active worker per a queue in an application instance
- **Ninject binding extensions InRequestOrJobScope** is now two separate methods with corresponding fallbacks in order - InTaskScope (mostly preferred) and InRequestScope
  
### Removed
- **IAutoMapperDefinition** - removed as obsolete and replaced with AutoMapper's own profiles (auto-discovered again)
- **removed implicit ASP.NET Web API configuration** - i.e. default OData and serializer settings
- **TokenValidator** - removed as obsolete
- **(Tenant)ContextSequence** - removed (possibly to be reimplemented later)

### Fixed
- **CRUD repositories** now correctly wrap their **concurrency exceptions as OptimisticConcurrencyException**
- **objects from DI container** are now deterministically disposed at the end of tasks run within a context (also reducing amount of unused open DB connection)
- **Hangfire job task context disposal** now correctly awaits the command to finish

## [1.3.0] - 2018-08-31
### Fixed
- **EF6SagaMetadataRepository** bug not including the keys in a query
- **EF6 saga keys** not being reloaded correctly due to JSON serialization changing dictionary key character cases
- **ExecuteCommandJob** is now resolved correctly by Hangfire

### Added
- **(I)UserPermissionAuthorizer** for manual and more fine-grained user action authorization
- **EF6/EntityEventProjector** for more arbitrary projections

### Changed
- framework configuration - framework and its module parameters now need to be programmatically configured and set-up using the newly introduced **RevoConfiguration**
- **PermissionAuthorizer** renamed to PermissionAuthorizationMatcher
- **Default ASP.NET Web API JSON ContractResolver** switched to DefaultContractResolver with CamelCaseNamingStrategy from CamelCasePropertyNamesContractResolver
(matches default ASP.NET Core behavior and does not change dictionary key character cases)
- Hangfire integration for background job processing got its own **Revo.Infrastructure.Hangfire** package

## [1.2.0] - 2018-07-16
- First public version released.