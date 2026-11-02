using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace ElectronicsShop.Data;

public static class DbHelper
{
    // строка подключения читается из App.config при первом обращении
    public static readonly string ConnectionString =
        ConfigurationManager.ConnectionStrings["ShopDb"].ConnectionString;

    // имя провайдера для фабрики
    public const string ProviderName = "System.Data.SqlClient";

    public static DbProviderFactory Factory => DbProviderFactories.GetFactory(ProviderName);

    public static DbConnection CreateConnection()
    {
        var conn = Factory.CreateConnection();
        conn.ConnectionString = ConnectionString;
        return conn;
    }

    public static DbCommand CreateCommand(DbConnection conn, string sql, params DbParameter[] ps)
    {
        var cmd = Factory.CreateCommand();
        cmd.Connection = conn;
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        if (ps != null) foreach (var p in ps) cmd.Parameters.Add(p);
        return cmd;
    }

    // параметризованный параметр (защита от SQL-инъекций)
    public static DbParameter Param(string name, object value, DbType type = DbType.String)
    {
        var p = Factory.CreateParameter();
        p.ParameterName = name;
        p.DbType = type;
        p.Value = value ?? DBNull.Value;
        return p;
    }
}