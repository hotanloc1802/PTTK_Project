using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentManagement.Data;

// Create a new file: DbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ApartmentManagement.Core.Factory
{
    public static class DbContextFactory
    {
        public static ApartmentDbContext CreateDbContext()
        {
            var dbConnection = new DbConnection();
            var optionsBuilder = new DbContextOptionsBuilder<ApartmentDbContext>();

            // Get the schema from BuildingManager, default to "MienDong" if null
            var buildingSchema = Core.Singleton.BuildingManager.Instance.CurrentBuildingSchema ?? "mien_dong";

            // Add the schema to connection string if needed
            var connectionString = dbConnection.ConnectionString;
            if (!connectionString.Contains("SearchPath"))
            {
                connectionString += $";SearchPath={buildingSchema}";
            }

            // IMPORTANT: Add a unique identifier to force model recreation
            optionsBuilder.UseNpgsql(connectionString)
                         .EnableSensitiveDataLogging()
                         // This forces EF to create a new model instead of using the cached one
                         .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();

            // Create context with the correct schema
            return new ApartmentDbContext(dbConnection, optionsBuilder.Options, buildingSchema);
        }
    }

    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            // If context is our ApartmentDbContext, include schema in cache key
            if (context is ApartmentDbContext apartmentContext)
            {
                // Get the schema name from the context
                return (context.GetType(), apartmentContext._schema, designTime);
            }

            // Default implementation for other contexts
            return (context.GetType(), designTime);
        }
    }
}