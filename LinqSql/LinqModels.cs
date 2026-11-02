using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;

namespace ElectronicsShop.LinqSql;

[Table(Name = "Categories")]
public class CategoryLinq
{
    [Column(Name = "Id", IsPrimaryKey = true, IsDbGenerated = true)] public int Id;
    [Column(Name = "Name")] public string Name;
}

[Table(Name = "Products")]
public class ProductLinq
{
    [Column(Name = "Id", IsPrimaryKey = true, IsDbGenerated = true)] public int Id;
    [Column(Name = "Name")] public string Name;
    [Column(Name = "CategoryId")] public int CategoryId;
    [Column(Name = "Price")] public decimal Price;
    [Column(Name = "Stock")] public int Stock;

    private CategoryLinq _category;
    [Association(ThisKey = "CategoryId", OtherKey = "Id", Storage = "_category")]
    public CategoryLinq Category { get => _category; set => _category = value; }
}

[Table(Name = "Orders")]
public class OrderLinq
{
    [Column(Name = "Id", IsPrimaryKey = true, IsDbGenerated = true)] public int Id;
    [Column(Name = "CustomerId")] public int CustomerId;
    [Column(Name = "OrderDate")] public DateTime OrderDate;
    [Column(Name = "TotalAmount")] public decimal TotalAmount;

    private EntitySet<OrderItemLinq> _items = new EntitySet<OrderItemLinq>();
    [Association(ThisKey = "Id", OtherKey = "OrderId", Storage = "_items")]
    public EntitySet<OrderItemLinq> Items { get => _items; set => _items.Assign(value); }
}

[Table(Name = "OrderItems")]
public class OrderItemLinq
{
    [Column(Name = "Id", IsPrimaryKey = true, IsDbGenerated = true)] public int Id;
    [Column(Name = "OrderId")] public int OrderId;
    [Column(Name = "ProductId")] public int ProductId;
    [Column(Name = "Quantity")] public int Quantity;

    private ProductLinq _product;
    [Association(ThisKey = "ProductId", OtherKey = "Id", Storage = "_product")]
    public ProductLinq Product { get => _product; set => _product = value; }
}

public class ShopDataContext : DataContext
{
    public Table<ProductLinq> Products;
    public Table<OrderLinq> Orders;
    public Table<OrderItemLinq> OrderItems;
    public Table<CategoryLinq> Categories;

    public ShopDataContext(string connectionString) : base(connectionString)
    {
        Products = GetTable<ProductLinq>();
        Orders = GetTable<OrderLinq>();
        OrderItems = GetTable<OrderItemLinq>();
        Categories = GetTable<CategoryLinq>();
    }
}

public static class LinqToSqlService
{
    public static void PrintTop3SoldLast30Days()
    {
        try
        {
            using var db = new ShopDataContext(Data.DbHelper.ConnectionString);
            var since = DateTime.Now.AddDays(-30);

            // группировка по названию, сумма Quantity, сортировка desc, топ-3
            var query =
                from oi in db.OrderItems
                join o in db.Orders on oi.OrderId equals o.Id
                join p in db.Products on oi.ProductId equals p.Id
                where o.OrderDate >= since
                group oi by p.Name into g
                select new { Name = g.Key, Sold = g.Sum(x => x.Quantity) };

            var top = query.OrderByDescending(x => x.Sold).Take(3).ToList();

            Console.WriteLine("Топ-3 продаваемых товаров за последние 30 дней:");
            if (top.Count == 0) Console.WriteLine("  (нет продаж за период)");
            foreach (var t in top)
                Console.WriteLine($"  {t.Name} — Продано штук: {t.Sold}");
        }
        catch (Exception ex) { Console.WriteLine("Ошибка LINQ to SQL: " + ex.Message); }
    }
}