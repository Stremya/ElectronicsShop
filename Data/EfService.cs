using System;
using System.Collections.Generic;
using System.Linq;
using ElectronicsShop.Models;

namespace ElectronicsShop.Services;

public class EfService
{
    // Инициализация БД
    public static void InitializeAndSeed()
    {
        using var ctx = new ShopContext();
        ctx.Database.CreateIfNotExists(); 

        if (ctx.Products.Any()) return;

        var now = DateTime.Now;

        var cats = new List<Category>();
        for (int i = 1; i <= 5; i++)
            cats.Add(new Category { Name = "Категория " + i });

        var prods = new List<Product>();
        string[] pn = { "Ноутбук X1", "Смартфон P20", "Наушники ANC", "Монитор 27''", "Клавиатура MK",
                        "Мышь GX", "SSD 1TB", "Видеокарта RTX", "Смарт-часы T", "Планшет F8",
                        "Колонка Boom", "Зарядка 65W" };
        decimal[] pp = { 89990m, 54990m, 12990m, 25990m, 7990m, 4990m, 9990m, 64990m, 19990m, 32990m, 6990m, 2990m };
        int[] st = { 10, 8, 25, 12, 30, 40, 20, 5, 15, 9, 22, 50 };
        for (int i = 0; i < pn.Length; i++)
            prods.Add(new Product { Name = pn[i], Price = pp[i], Stock = st[i], Category = cats[i % 5] });

        // Клиенты: часть «спящие»  часть активные
        var custs = new List<Customer>();
        custs.Add(new Customer { FullName = "Анна Смирнова", Email = "anna@mail.ru", RegistrationDate = now.AddMonths(-8) }); // спящий
        custs.Add(new Customer { FullName = "Борис Иванов", Email = "boris@mail.ru", RegistrationDate = now.AddMonths(-1) });  // не спящий
        custs.Add(new Customer { FullName = "Грета Мюллер", Email = "greta@mail.de", RegistrationDate = now.AddMonths(-10) }); // спящий
        custs.Add(new Customer { FullName = "Ханс Шмидт", Email = "hans@mail.de", RegistrationDate = now.AddMonths(-6) });  // не спящий
        custs.Add(new Customer { FullName = "Карлос Гомес", Email = "carlos@mail.es", RegistrationDate = now.AddMonths(-2) });  // недавний
        custs.Add(new Customer { FullName = "Мария Лопес", Email = "maria@mail.es", RegistrationDate = now.AddMonths(-12) }); // не спящий
        custs.Add(new Customer { FullName = "Дмитрий Орлов", Email = "dmitry@mail.ru", RegistrationDate = now.AddMonths(-7) });  // спящий
        custs.Add(new Customer { FullName = "Елена Волкова", Email = "elena@mail.ru", RegistrationDate = now.AddMonths(-3) });  // будет заказ

        ctx.Categories.AddRange(cats);
        ctx.Products.AddRange(prods);
        ctx.Customers.AddRange(custs);
        ctx.SaveChanges(); // проставит Id родителям

        // Заказы: недавние и старые
        void MakeOrder(Customer c, DateTime date, params (Product p, int q)[] lines)
        {
            var o = new Order { Customer = c, OrderDate = date, TotalAmount = 0 };
            foreach (var (p, q) in lines)
            {
                o.OrderItems.Add(new OrderItem { Product = p, Quantity = q });
                o.TotalAmount += p.Price * q;
            }
            ctx.Orders.Add(o);
        }

        MakeOrder(custs[3], now.AddDays(-5), (prods[0], 1), (prods[2], 2));          // недавний
        MakeOrder(custs[5], now.AddDays(-90), (prods[1], 1));                          // старый
        MakeOrder(custs[7], now.AddDays(-12), (prods[2], 3), (prods[5], 2));           // недавний
        MakeOrder(custs[3], now.AddDays(-20), (prods[0], 2));                          // недавний
        MakeOrder(custs[5], now.AddDays(-3), (prods[6], 4), (prods[11], 5));          // недавний 
        MakeOrder(custs[7], now.AddDays(-25), (prods[2], 1));                          // недавний

        ctx.SaveChanges();
        Console.WriteLine("Тестовые данные загружены (EF6).");
    }

    // оформление заказа с явной транзакцией и проверкой склада
    public string PlaceOrder(int customerId, Dictionary<int, int> productsToBuy)
    {
        using var ctx = new ShopContext();
        var tx = ctx.Database.BeginTransaction();
        try
        {
            // проверка достаточности склада до изменений
            foreach (var kv in productsToBuy)
            {
                var p = ctx.Products.Find(kv.Key);
                if (p == null) throw new Exception($"Товар Id={kv.Key} не найден.");
                if (p.Stock < kv.Value)
                    throw new Exception($"Недостаточно на складе: «{p.Name}», запрошено {kv.Value}, в наличии {p.Stock}.");
            }

            var order = new Order { CustomerId = customerId, OrderDate = DateTime.Now, TotalAmount = 0m };
            foreach (var kv in productsToBuy)
            {
                var p = ctx.Products.Find(kv.Key);
                order.OrderItems.Add(new OrderItem { Product = p, Quantity = kv.Value });
                order.TotalAmount += p.Price * kv.Value;
                p.Stock -= kv.Value;
            }

            ctx.Orders.Add(order);
            ctx.SaveChanges();
            tx.Commit();
            return $"Заказ #{order.Id} успешно оформлен. Сумма: {order.TotalAmount:0.00} руб.";
        }
        catch (Exception ex)
        {
            try { tx.Rollback(); } catch { }
            return "ОТКАТ транзакции (Rollback): " + ex.Message;
        }
        finally { tx.Dispose(); }
    }
}