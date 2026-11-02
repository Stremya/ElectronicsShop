using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using ElectronicsShop.Data;

namespace ElectronicsShop.Services;

public class DisconnectedService
{
    public void RunDemo(int categoryId)
    {
        DbConnection conn = null;
        try
        {
            conn = DbHelper.CreateConnection();
            conn.Open();

            // Загрузка Products в DataTable через DataAdapter
            var adapter = DbHelper.Factory.CreateDataAdapter();
            adapter.SelectCommand = DbHelper.CreateCommand(conn,
                "SELECT Id, Name, CategoryId, Price, Stock FROM Products");
            var dt = new DataTable("Products");
            adapter.Fill(dt);
            dt.PrimaryKey = new[] { dt.Columns["Id"] }; // ключ для корректного трекинга
            var idCol = dt.Columns["Id"];
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = -1;
            idCol.AutoIncrementStep = -1;

            // Увеличение Price на 10% у товаров заданной категории
            int changed = 0;
            foreach (DataRow r in dt.Rows)
                if ((int)r["CategoryId"] == categoryId)
                {
                    r["Price"] = ((decimal)r["Price"]) * 1.10m;
                    changed++;
                }

            // Программная новая строка 
            var nr = dt.NewRow();
            nr["Name"] = "НОВЫЙ товар (demo)";
            nr["CategoryId"] = categoryId;
            nr["Price"] = 1999.99m;
            nr["Stock"] = 7;
            dt.Rows.Add(nr);

            // Удаление одной несвязанной строки той же категории
            var toDel = dt.Rows.Cast<DataRow>()
                .FirstOrDefault(r => r.RowState == DataRowState.Unchanged && (int)r["CategoryId"] == categoryId);
            toDel?.Delete();

            Console.WriteLine($"Изменено цен: {changed}, добавлено строк: 1, удалено строк: {(toDel != null ? 1 : 0)}");
            PrintRowStates(dt, "ДО синхронизации");

            // Ручная настройка команд 
            adapter.InsertCommand = BuildInsert(conn);
            adapter.UpdateCommand = BuildUpdate(conn);
            adapter.DeleteCommand = BuildDelete(conn);

            int affected = adapter.Update(dt); // синхронизация с БД
            Console.WriteLine($"Сохранено в БД строк: {affected}");
            PrintRowStates(dt, "ПОСЛЕ синхронизации");
        }
        catch (Exception ex) { Console.WriteLine("Ошибка отсоединённого режима: " + ex.Message); }
        finally { conn?.Dispose(); }
    }

    // --- создание параметра через фабрику и добавление одним аргументом ---
    private static DbParameter MakeParam(DbCommand cmd, string name, DbType type,
                                         string sourceColumn, DataRowVersion version,
                                         ParameterDirection dir = ParameterDirection.Input)
    {
        var p = DbHelper.Factory.CreateParameter();   // фабрика провайдеров
        p.ParameterName = name;
        p.DbType = type;
        p.Direction = dir;
        p.SourceColumn = sourceColumn;
        p.SourceVersion = version;
        cmd.Parameters.Add(p);                         
        return p;
    }

    private DbCommand BuildInsert(DbConnection conn)
    {
        var cmd = DbHelper.CreateCommand(conn,
            "INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES (@Name, @CategoryId, @Price, @Stock); SET @Id = SCOPE_IDENTITY();");
        MakeParam(cmd, "@Name", DbType.String, "Name", DataRowVersion.Current);
        MakeParam(cmd, "@CategoryId", DbType.Int32, "CategoryId", DataRowVersion.Current);
        MakeParam(cmd, "@Price", DbType.Decimal, "Price", DataRowVersion.Current);
        MakeParam(cmd, "@Stock", DbType.Int32, "Stock", DataRowVersion.Current);
        // возврат identity в столбец Id добавленной строки
        MakeParam(cmd, "@Id", DbType.Int32, "Id", DataRowVersion.Proposed, ParameterDirection.Output);
        return cmd;
    }

    private DbCommand BuildUpdate(DbConnection conn)
    {
        var cmd = DbHelper.CreateCommand(conn,
            "UPDATE Products SET Name=@Name, CategoryId=@CategoryId, Price=@Price, Stock=@Stock WHERE Id=@Original_Id");
        MakeParam(cmd, "@Name", DbType.String, "Name", DataRowVersion.Current);
        MakeParam(cmd, "@CategoryId", DbType.Int32, "CategoryId", DataRowVersion.Current);
        MakeParam(cmd, "@Price", DbType.Decimal, "Price", DataRowVersion.Current);
        MakeParam(cmd, "@Stock", DbType.Int32, "Stock", DataRowVersion.Current);
        MakeParam(cmd, "@Original_Id", DbType.Int32, "Id", DataRowVersion.Original);
        return cmd;
    }

    private DbCommand BuildDelete(DbConnection conn)
    {
        var cmd = DbHelper.CreateCommand(conn, "DELETE FROM Products WHERE Id=@Original_Id");
        MakeParam(cmd, "@Original_Id", DbType.Int32, "Id", DataRowVersion.Original);
        return cmd;
    }

    private static void PrintRowStates(DataTable dt, string title)
    {
        Console.WriteLine($"  [{title}]");
        foreach (DataRow r in dt.Rows)
            Console.WriteLine($"    Id={r["Id"]} | {r["Name"]} | состояние={r.RowState}");
    }
}