using System.Data.Common;
using DoNet.Utility.Database;

namespace DoNet.GenerateEntity.SqlServer
{
    internal static class SqlServerTools
    {
        public static SqlServerVersions GetServerVersion(string dbName)
        {
            const string strSql = "SELECT @@version as version";
            string str2 = null;
            DbHelper dbHelper = string.IsNullOrEmpty(dbName)
                ? DbFactory.CreateDatabase()
                : DbFactory.CreateDatabase(dbName);
            DbCommand sqlStringCommand = dbHelper.GetSqlStringCommand(strSql);
            str2 = dbHelper.ExecuteScalar(sqlStringCommand).ToString();
            if (str2.Contains("SQL Server  2000"))
            {
                return SqlServerVersions.SqlServer2000;
            }
            if (str2.Contains("SQL Server 2005"))
            {
                return SqlServerVersions.SqlServer2005;
            }
            if (str2.Contains("SQL Server 2008"))
            {
                return SqlServerVersions.SqlServer2008;
            }
            if (str2.Contains("SQL Server 2012"))
            {
                return SqlServerVersions.SqlServer2012;
            }
            return SqlServerVersions.Other;
        }
    }
}