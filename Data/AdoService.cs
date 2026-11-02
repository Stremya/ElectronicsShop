using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using ElectronicsShop.Data;

namespace ElectronicsShop.Services;

public class AdoService
{
    // синхронное добавление товара через ExecuteNonQuery
    public int AddProduct(string name, int categoryId, decimal price, int stock)
    {
        DbConnection conn = null;
        try
        {
            conn = DbHelper.CreateConnection();
            conn.Open();
            const string sql = "INSERT INTO Products (Name, CategoryId, Price, Stock) VALUES (@Name, @CategoryId, @Price, @Stock)";
            using (var cmd = DbHelper.CreateCommand(conn, sql,
                DbHelper.Param("@Name", name),
                DbHelper.Param("@CategoryId", categoryId, DbType.Int32),
                DbHelper.Param("@Price", price, DbType.Decimal),
                DbHelper.Param("@Stock", stock, DbType.Int32)))
            {
                return cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка добавления товара: " + ex.Message);
            return -1;
        }
        finally { conn?.Dispose(); }
    }

    // JOIN-запрос + чтение через DbDataReader
    public void PrintAllProductsJoin()
    {
        DbConnection conn = null;
        try
        {
            conn = DbHelper.CreateConnection();
            conn.Open();
            const string sql = @"SELECT cat.Name AS CategoryName, p.Name AS ProductName, p.Price, p.Stock
                                 FROM Products p JOIN Categories cat ON p.CategoryId = cat.Id
                                 ORDER BY cat.Name, p.Name";
            using (var cmd = DbHelper.CreateCommand(conn, sql))
            using (var reader = cmd.ExecuteReader())
            {
                Console.WriteLine("Список товаров [Категория] Название - Цена (Остаток):");
                while (reader.Read())
                {
                    string cat = (string)reader["CategoryName"];
                    string nm = (string)reader["ProductName"];
                    decimal pr = (decimal)reader["Price"];
                    int st = (int)reader["Stock"];
                    Console.WriteLine($"  [{cat}] {nm} - {pr:0.00} (Остаток: {st} шт.)");
                }
            }
        }
        catch (Exception ex) { Console.WriteLine("Ошибка чтения товаров: " + ex.Message); }
        finally { conn?.Dispose(); }
    }

    // асинхронный метод через ExecuteScalarAsync
    public async Task<int> GetStockAsync(int productId)
    {
        DbConnection conn = null;
        try
        {
            conn = DbHelper.CreateConnection();
            await conn.OpenAsync();
            using (var cmd = DbHelper.CreateCommand(conn, "SELECT Stock FROM Products WHERE Id = @Id",
                DbHelper.Param("@Id", productId, DbType.Int32)))
            {
                object res = await cmd.ExecuteScalarAsync();
                return (res == null || res == DBNull.Value) ? 0 : Convert.ToInt32(res);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка GetStockAsync: " + ex.Message);
            return -1;
        }
        finally { conn?.Dispose(); }
    }
}