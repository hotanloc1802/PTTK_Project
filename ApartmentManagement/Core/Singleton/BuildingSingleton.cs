using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApartmentManagement.Core.Singleton
{
    // In BuildingSingleton.cs
    public class BuildingManager
    {
        private static BuildingManager _instance;
        private string _currentBuildingSchema = "miendong"; // Set default value here

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

        // Get and set the current building schema
        public string CurrentBuildingSchema
        {
            get => string.IsNullOrEmpty(_currentBuildingSchema) ? "miendong" : _currentBuildingSchema;
            private set => _currentBuildingSchema = value;
        }

        // Change the current building and its schema
        public void SetBuilding(string buildingSchema)
        {
            _currentBuildingSchema = buildingSchema ?? "miendong";
        }
    }
}
