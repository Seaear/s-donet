using System;
using System.Data;
using System.Data.Common;
using DoNet.Utility.Web;

namespace DoNet.Utility.Database
{
    /// <summary>
    ///     ���и������ݿ���ز��������ݿ������
    /// </summary>
    public class DbHelper
    {
        #region ���캯��

        public DbHelper(string dbName = "", bool checkStoredProcedurePara = true)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                ConnectionString = ConfigHelper.GetDbString(1);
                if (string.IsNullOrEmpty(ConnectionString))
                    throw new Exception("��δ�������ݿ������ַ�����");

                DbName = ConfigHelper.GetDbName(1);
                ProviderName = ConfigHelper.GetDbProviderName(1);
                ProviderFactory = DbProviderFactories.GetFactory(ProviderName);
            }
            else
            {
                ConnectionString = ConfigHelper.GetDbString(dbName);
                if (string.IsNullOrEmpty(ConnectionString))
                    throw new Exception("��δ�������ݿ������ַ�����");

                DbName = ConfigHelper.GetDbName(dbName);
                ProviderName = ConfigHelper.GetDbProviderName(dbName);
                ProviderFactory = DbProviderFactories.GetFactory(ProviderName);
            }

            if (Encryption.IsEncrypted(ConnectionString))
                ConnectionString = Encryption.Decrypt(ConnectionString);

            //�Ƿ���洢���̵Ĳ���
            CheckStoredProcedurePara = checkStoredProcedurePara;
        }

        #endregion

        #region ����

        /// <summary>
        ///     �Ƿ��Ƿ��ڵ��ô洢����ʱ������
        /// </summary>
        public bool CheckStoredProcedurePara { get; set; }

        public string DbName { get; private set; }
        public string ConnectionString { get; }
        public string ProviderName { get; }
        public DbProviderFactory ProviderFactory { get; }
        public DbTransaction DbTransactions { get; set; }

        #endregion

        #region ȫ�ֱ���

        private const string StrSetIsoLevelReadCommited = " SET TRANSACTION ISOLATION LEVEL READ COMMITTED; ";
        private const string StrSetIsoLevelReadUnCommited = " SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; ";

        #endregion

        #region �������

        public DbParameter AddInParameter(DbCommand dm, string name, DbType dbType, object value)
        {
            return AddParameter(dm, name, dbType, 0, ParameterDirection.Input, false, 0, 0, string.Empty,
                DataRowVersion.Default, value);
        }

        public virtual DbParameter AddParameter(DbCommand dm, string name, DbType dbType, int size,
            ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn,
            DataRowVersion sourceVersion, object value)
        {
            ////if (dbType == DbType.String)
            ////    throw new Exception("�벻Ҫʹ��DbType.String�������ݿ��ѯ��");

            if (CheckInjectAttackForSp(dm, value))
                throw new Exception("����Ĳ������ݿ��ܶ�ϵͳ�ȶ������Ӱ�죬������ֹͣ��[" + value + "]");

            var param = ProviderFactory.CreateParameter();
            if (param != null)
            {
                param.ParameterName = name;
                param.DbType = dbType;
                param.Size = size;
                param.Value = value ?? DBNull.Value;
                param.Direction = direction;
                param.IsNullable = nullable;
                param.SourceColumn = sourceColumn;
                param.SourceVersion = sourceVersion;
                dm.Parameters.Add(param);
            }
            return param;
        }

        #endregion

        #region ��ȡ����

        public virtual DbCommand GetSqlStringCommand(string commandText)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("����Ϊ��", "commandText");
            }
            return CreateCommand(CommandType.Text, commandText);
        }

        public virtual DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentException("�洢��������Ϊ��", "storedProcedureName");
            }
            return CreateCommand(CommandType.StoredProcedure, storedProcedureName);
        }

        public virtual DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
            {
                throw new ArgumentException("�洢��������Ϊ��", "storedProcedureName");
            }
            var dm = CreateCommand(CommandType.StoredProcedure, storedProcedureName);
            for (var i = 0; i < parameterValues.Length; i++)
            {
                IDataParameter parameter = dm.Parameters[i];
                if (CheckInjectAttackForSp(dm, parameterValues[i]))
                {
                    throw new Exception("����Ĳ������ݿ��ܶ�ϵͳ�ȶ������Ӱ�죬������ֹͣ��[" + parameterValues[i] + "]");
                }
                dm.Parameters[parameter.ParameterName].Value = parameterValues[i] ?? DBNull.Value;
            }
            return dm;
        }

        private DbCommand CreateCommand(CommandType commandType, string commandText = "")
        {
            var dm = ProviderFactory.CreateCommand();
            {
                if (dm != null)
                {
                    dm.CommandType = commandType;
                    dm.CommandText = commandText;
                }
                return dm;
            }
        }

        #endregion

        #region �������Ӻ�����

        public virtual DbTransaction CreateTransaction()
        {
            return CreateTransaction(IsolationLevel.ReadCommitted);
        }

        public virtual DbTransaction CreateTransaction(IsolationLevel iso)
        {
            var dc = CreateConnection();
            var dt = dc.BeginTransaction(iso);
            return dt;
        }

        public virtual DbConnection CreateConnection()
        {
            var dc = ProviderFactory.CreateConnection();
            if (dc != null && dc.State != ConnectionState.Open)
            {
                dc.ConnectionString = ConnectionString;
                dc.Open();
            }
            return dc;
        }

        #endregion

        #region ִ�����

        public virtual DataTable ExecuteDataTable(string commandText)
        {
            using (var dm = GetSqlStringCommand(commandText))
            {
                return ExecuteDataTable(dm);
            }
        }

        public virtual DataTable ExecuteDataTable(DbCommand dm)
        {
            using (var conn = CreateConnection())
            {
                dm.Connection = conn;
                var myDataTable = new DataTable();
                using (var myDataAdapter = ProviderFactory.CreateDataAdapter())
                {
                    if (myDataAdapter != null)
                    {
                        myDataAdapter.SelectCommand = dm;
                        myDataAdapter.Fill(myDataTable);
                    }
                    myDataTable.RemotingFormat = SerializationFormat.Binary;
                    return myDataTable;
                }
            }
        }

        public virtual DataSet ExecuteDataSet(string commandText)
        {
            using (var dm = GetSqlStringCommand(commandText))
            {
                return ExecuteDataSet(dm);
            }
        }

        public virtual DataSet ExecuteDataSet(DbCommand dm)
        {
            using (var conn = CreateConnection())
            {
                dm.Connection = conn;
                using (var myDataAdapter = ProviderFactory.CreateDataAdapter())
                {
                    var myDataSet = new DataSet();
                    if (myDataAdapter != null)
                    {
                        myDataAdapter.SelectCommand = dm;
                        myDataAdapter.Fill(myDataSet);
                    }
                    myDataSet.RemotingFormat = SerializationFormat.Binary;
                    return myDataSet;
                }
            }
        }

        public virtual int ExecuteNonQuery(string commandText)
        {
            using (var dm = GetSqlStringCommand(commandText))
            {
                return ExecuteNonQuery(dm);
            }
        }

        public virtual int ExecuteNonQuery(DbCommand dm)
        {
            using (var conn = CreateConnection())
            {
                dm.Connection = conn;
                var result = dm.ExecuteNonQuery();
                return result;
            }
        }

        public virtual int ExecuteNonQuery(string commandText, DbTransaction dt)
        {
            using (var dm = GetSqlStringCommand(commandText))
            {
                if (dm == null) return 0;
                return ExecuteNonQuery(dm, dt);
            }
        }

        public virtual int ExecuteNonQuery(DbCommand dm, DbTransaction dt)
        {
            dm.Transaction = dt;
            dm.Connection = dt.Connection;
            var result = dm.ExecuteNonQuery();
            return result;
        }

        public virtual IDataReader ExecuteReader(string commandText)
        {
            using (var dm = GetSqlStringCommand(commandText))
            {
                return ExecuteReader(dm);
            }
        }

        public virtual IDataReader ExecuteReader(DbCommand dm)
        {
            dm.Connection = CreateConnection();
            if (dm.CommandType == CommandType.Text)
                dm.CommandText = StrSetIsoLevelReadUnCommited + dm.CommandText;

            //IDataReader reader;
            //if (Transaction.Current != null)
            //    reader = dm.ExecuteReader(CommandBehavior.Default);
            //else
            //    reader = dm.ExecuteReader(CommandBehavior.CloseConnection);

            return dm.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public virtual IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues)
        {
            using (var dm = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteReader(dm);
            }
        }

        public virtual object ExecuteScalar(string commandText)
        {
            using (var dm = GetSqlStringCommand(commandText))
            {
                dm.CommandText = StrSetIsoLevelReadCommited + commandText;
                return ExecuteScalar(dm);
            }
        }

        public virtual object ExecuteScalar(DbCommand dm)
        {
            using (var conn = CreateConnection())
            {
                dm.Connection = conn;
                if (dm.CommandType == CommandType.Text)
                    dm.CommandText = StrSetIsoLevelReadUnCommited + dm.CommandText;
                return dm.ExecuteScalar();
            }
        }

        public virtual object ExecuteScalar(string storedProcedureName, params object[] parameterValues)
        {
            using (var dm = GetStoredProcCommand(storedProcedureName, parameterValues))
            {
                return ExecuteScalar(dm);
            }
        }

        #endregion

        #region ���洢����SQLע��

        /// <summary>
        ///     �����ô洢����ʱ��صĲ����Ƿ���ע��ķ���
        /// </summary>
        private bool CheckInjectAttackForSp(DbCommand dm)
        {
            if (!CheckStoredProcedurePara)
                return false;

            if (dm == null)
                return false;

            if (dm.CommandType != CommandType.StoredProcedure)
                return false;

            if (dm.Parameters.Count == 0)
                return false;

            for (var i = 0; i < dm.Parameters.Count; i++)
            {
                if (dm.Parameters[i].Value == null || dm.Parameters[i].Value is DBNull)
                    continue;
                if (!(dm.Parameters[i].Value is string))
                    continue;

                if (!SqlInjectionReject.CheckMssqlParameter(dm.Parameters[i].Value.ToString()))
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     �����ô洢����ʱ��صĲ����Ƿ���ע��ķ���
        /// </summary>
        /// <param name="dm"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool CheckInjectAttackForSp(DbCommand dm, object val)
        {
            if (!CheckStoredProcedurePara)
                return false;

            if (dm == null)
                return false;

            if (dm.CommandType != CommandType.StoredProcedure)
                return false;

            if (val == null || val is DBNull)
                return false;

            if (!(val is string))
                return false;

            if (!SqlInjectionReject.CheckMssqlParameter(val.ToString()))
                return true;

            return false;
        }

        #endregion
    }

    //SQLע���ж�
}