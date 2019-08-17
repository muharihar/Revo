﻿using Microsoft.EntityFrameworkCore;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class LowerCaseConvention : EFCoreConventionBase
    {
        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToLowerInvariant());
                
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToLowerInvariant());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToLowerInvariant());
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName().ToLowerInvariant());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(index.GetName().ToLowerInvariant());
                }
            }
        }
    }
}
