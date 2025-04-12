using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApartmentManagement.Core.Singleton
{
    public class BuildingManager
    {
        private static BuildingManager _instance;
        private string _currentBuildingSchema;

        // Private constructor to prevent instantiation from outside
        private BuildingManager() { }

        // Singleton instance
        public static BuildingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuildingManager();
                }
                return _instance;
            }
        }

        // Get and set the current building schema (used to set search_path in connection)
        public string CurrentBuildingSchema
        {
            get
            {
                // Check if the schema is null and set default to "PTTK"
                return string.IsNullOrEmpty(_currentBuildingSchema) ? "PTTK" : _currentBuildingSchema;
            }
            private set
            {
                _currentBuildingSchema = value;
                // You could trigger actions related to the new schema, like updating database connections, etc.
                Console.WriteLine($"Schema changed to: {_currentBuildingSchema}"); // Debugging log
            }
        }

        // Change the current building and its schema
        public void SetBuilding(string buildingSchema)
        {
            _currentBuildingSchema = buildingSchema;
            // Print the current schema after setting
        }

        // You can add other methods to handle different behaviors related to the building
    }
}
