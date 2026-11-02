using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using ElectronicsShop.Data;

namespace ElectronicsShop.Services;

public class SleepingCustomerDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class DapperService
{
    public List<SleepingCustomerDto> GetSleepingCustomers()
    {
        const string sql = @"
            SELECT c.FullName, c.Email, c.RegistrationDate
            FROM Customers c
            LEFT JOIN Orders o ON o.CustomerId = c.Id
            WHERE o.Id IS NULL
              AND c.RegistrationDate < DATEADD(month, -3, GETDATE())
            ORDER BY c.RegistrationDate";
        try
        {
            using var conn = new SqlConnection(DbHelper.ConnectionString);
            return conn.Query<SleepingCustomerDto>(sql).AsList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка Dapper-запроса: " + ex.Message);
            return new List<SleepingCustomerDto>();
        }
    }

    public void PrintSleepingCustomers()
    {
        var list = GetSleepingCustomers();
        Console.WriteLine($"«Спящих» клиентов (регистрация > 3 мес назад, 0 заказов): {list.Count}");
        foreach (var c in list)
            Console.WriteLine($"  {c.FullName} | {c.Email} | зарегистрирован(а) {c.RegistrationDate:d}");
    }
}