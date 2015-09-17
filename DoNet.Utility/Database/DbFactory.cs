namespace DoNet.Utility.Database
{
    public class DbFactory
    {
        public static DbHelper CreateDatabase()
        {
            return new DbHelper();
        }

        public static DbHelper CreateDatabase(string dbName)
        {
            return new DbHelper(dbName);
        }
    }
}