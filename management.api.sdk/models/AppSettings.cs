namespace agility.models
{
    public class AppSettings
    {

        public static string Section = "AppSettings";
        public AppSettings()
        {
            this.storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            this.tableName = Environment.GetEnvironmentVariable("TableName");
        }
        public string? storageConnectionString { get; set; }
        public string? tableName { get; set; }
    }
}
