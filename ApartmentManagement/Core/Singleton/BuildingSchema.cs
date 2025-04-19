using System.Windows;

namespace ApartmentManagement.Core.Singleton
{
    public class BuildingSchema
    {
        private static BuildingSchema _instance;
        private string _currentBuildingSchema;

        // Private constructor to prevent instantiation from outside
        private BuildingSchema() { }

        // Singleton instance
        public static BuildingSchema Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuildingSchema();
                }
                return _instance;
            }
        }

        // Get and set the current building schema
        public string CurrentBuildingSchema
        {
            get => string.IsNullOrEmpty(_currentBuildingSchema) ? "mien_dong" : _currentBuildingSchema;
            private set => _currentBuildingSchema = value;
        }

        // Change the current building and its schema if it's different
        public void SetBuilding(string buildingSchema)
        {
            MessageBox.Show("ê bị gọi nè ở singleton");
            // Only update schema if it's different
            if (_currentBuildingSchema != buildingSchema)
            {
                _currentBuildingSchema = buildingSchema ?? _currentBuildingSchema;
            }
            
        }
    }
}
