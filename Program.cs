using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ElectronicsShop.Data;
using ElectronicsShop.LinqSql;
using ElectronicsShop.Services;

namespace ElectronicsShop;

class Program
{
    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Строка подключения из App.config: " + DbHelper.ConnectionString);
        Console.WriteLine("Провайдер фабрики: " + DbHelper.ProviderName + "\n");

        Console.WriteLine("Инициализация базы ShopDb (создание таблиц + тестовые данные)...");
        EfService.InitializeAndSeed();
        Console.WriteLine("База готова.\n");

        var ado = new AdoService();
        var disc = new DisconnectedService();
        var dap = new DapperService();
        var ef = new EfService();

        while (true)
        {
            Console.WriteLine("================= МЕНЮ =================");
            Console.WriteLine("-- Задание 1: присоединённый режим --");
            Console.WriteLine(" 1. Добавить товар (INSERT, ExecuteNonQuery)");
            Console.WriteLine(" 2. Все товары (JOIN + DbDataReader)");
            Console.WriteLine(" 3. Остаток товара асинхронно (GetStockAsync)");
            Console.WriteLine("-- Задание 2: отсоединённый режим --");
            Console.WriteLine(" 4. Demo: +10% к ценам категории, новая строка, удаление, Update()");
            Console.WriteLine("-- Задание 3: LINQ to SQL --");
            Console.WriteLine(" 5. Топ-3 продаваемых за 30 дней");
            Console.WriteLine("-- Задание 4: Entity Framework --");
            Console.WriteLine(" 6. Оформить заказ (демо успеха + транзакция)");
            Console.WriteLine(" 7. Оформить заказ (демо ошибки -> Rollback)");
            Console.WriteLine("-- Задание 5: Dapper --");
            Console.WriteLine(" 8. «Спящие» клиенты");
            Console.WriteLine(" 0. Выход");
            Console.Write("Выбор: ");
            var cmd = Console.ReadLine();

            switch (cmd)
            {
                case "1":
                    Console.Write("Название: "); var nm = Console.ReadLine();
                    Console.Write("CategoryId: "); int cid = int.Parse(Console.ReadLine());
                    Console.Write("Цена: "); decimal pr = decimal.Parse(Console.ReadLine());
                    Console.Write("Остаток: "); int stk = int.Parse(Console.ReadLine());
                    var rows = ado.AddProduct(nm, cid, pr, stk);
                    Console.WriteLine(rows >= 0 ? $">> Вставлено строк: {rows}" : ">> Не удалось вставить.");
                    break;
                case "2": ado.PrintAllProductsJoin(); break;
                case "3":
                    Console.Write("Id товара: "); int pid = int.Parse(Console.ReadLine());
                    Console.WriteLine($">> Остаток товара {pid}: {await ado.GetStockAsync(pid)} шт.");
                    break;
                case "4":
                    Console.Write("CategoryId для +10%: "); int dcid = int.Parse(Console.ReadLine());
                    disc.RunDemo(dcid);
                    break;
                case "5": LinqToSqlService.PrintTop3SoldLast30Days(); break;
                case "6":
                    Console.WriteLine(ef.PlaceOrder(4, new Dictionary<int, int> { { 1, 1 } }));
                    Console.WriteLine($"   остаток товара 1 теперь: {await ado.GetStockAsync(1)}");
                    break;
                case "7":
                    int before = await ado.GetStockAsync(1);
                    Console.WriteLine(ef.PlaceOrder(4, new Dictionary<int, int> { { 1, 99999 } }));
                    int after = await ado.GetStockAsync(1);
                    Console.WriteLine($"   проверка отката: остаток был {before}, стал {after} (должен совпадать)");
                    break;
                case "8": dap.PrintSleepingCustomers(); break;
                case "0": Console.WriteLine("Выход."); return;
                default: Console.WriteLine("Неизвестная команда."); break;
            }
            Console.WriteLine();
        }
    }
}