﻿namespace Revo.Infrastructure.DataAccess.Migrations.Providers
{
    public class SqliteMigrationScripter : GenericSqlDatabaseMigrationScripter
    {
        public SqliteMigrationScripter() : base("sqlite")
        {
            DatabaseMigrationRecordTable = "rev_database_migration_record";
            RecordIdColumn = "rev_dmr_database_migration_record_id";
            TimeAppliedColumn = "rev_dmr_time_applied";
            ModuleNameColumn = "rev_dmr_module_name";
            VersionColumn = "rev_dmr_version";
            ChecksumColumn = "rev_dmr_checksum";
            FileNameColumn = "rev_dmr_file_name";
        }

        public override string SelectMigrationSchemaExistsSql => $@"
SELECT COUNT(*)
FROM sqlite_master
WHERE type = 'table' AND name = '{DatabaseMigrationRecordTable}';";

        public override string CreateMigrationSchemaSql => $@"
CREATE TABLE {DatabaseMigrationRecordTable} (
	{RecordIdColumn} uuid PRIMARY KEY,
	{TimeAppliedColumn} timestamp NOT NULL,
	{ModuleNameColumn} text NOT NULL,
	{VersionColumn} text,
    {ChecksumColumn} text NOT NULL,
    {FileNameColumn} text
);";
    }
}