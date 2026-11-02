IF DB_ID('ShopDb') IS NULL CREATE DATABASE ShopDb;
GO
USE ShopDb;
GO

IF OBJECT_ID('dbo.Categories','U') IS NULL CREATE TABLE dbo.Categories(Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100));
IF OBJECT_ID('dbo.Products','U')   IS NULL CREATE TABLE dbo.Products(Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100), CategoryId INT NOT NULL REFERENCES dbo.Categories(Id), Price DECIMAL(18,2), Stock INT);
IF OBJECT_ID('dbo.Customers','U')  IS NULL CREATE TABLE dbo.Customers(Id INT IDENTITY PRIMARY KEY, FullName NVARCHAR(100), Email NVARCHAR(100), RegistrationDate DATETIME);
IF OBJECT_ID('dbo.Orders','U')     IS NULL CREATE TABLE dbo.Orders(Id INT IDENTITY PRIMARY KEY, CustomerId INT NOT NULL REFERENCES dbo.Customers(Id), OrderDate DATETIME, TotalAmount DECIMAL(18,2));
IF OBJECT_ID('dbo.OrderItems','U') IS NULL CREATE TABLE dbo.OrderItems(Id INT IDENTITY PRIMARY KEY, OrderId INT NOT NULL REFERENCES dbo.Orders(Id), ProductId INT NOT NULL REFERENCES dbo.Products(Id), Quantity INT);
GO

IF (SELECT COUNT(1) FROM dbo.Categories) = 0
BEGIN
    INSERT INTO dbo.Categories(Name) VALUES (N'Категория 1'),(N'Категория 2'),(N'Категория 3'),(N'Категория 4'),(N'Категория 5');
    INSERT INTO dbo.Products(Name,CategoryId,Price,Stock) VALUES
      (N'Ноутбук X1',1,89990,10),(N'Смартфон P20',2,54990,8),(N'Наушники ANC',3,12990,25),
      (N'Монитор 27''',4,25990,12),(N'Клавиатура MK',5,7990,30),(N'Мышь GX',1,4990,40),
      (N'SSD 1TB',2,9990,20),(N'Видеокарта RTX',3,64990,5),(N'Смарт-часы T',4,19990,15),
      (N'Планшет F8',5,32990,9),(N'Колонка Boom',1,6990,22),(N'Зарядка 65W',2,2990,50);
    INSERT INTO dbo.Customers(FullName,Email,RegistrationDate) VALUES
      (N'Анна Смирнова',N'anna@mail.ru',DATEADD(month,-8,GETDATE())),
      (N'Борис Иванов',N'boris@mail.ru',DATEADD(month,-1,GETDATE())),
      (N'Грета Мюллер',N'greta@mail.de',DATEADD(month,-10,GETDATE())),
      (N'Ханс Шмидт',N'hans@mail.de',DATEADD(month,-6,GETDATE())),
      (N'Карлос Гомес',N'carlos@mail.es',DATEADD(month,-2,GETDATE())),
      (N'Мария Лопес',N'maria@mail.es',DATEADD(month,-12,GETDATE())),
      (N'Дмитрий Орлов',N'dmitry@mail.ru',DATEADD(month,-7,GETDATE())),
      (N'Елена Волкова',N'elena@mail.ru',DATEADD(month,-3,GETDATE()));
    INSERT INTO dbo.Orders(CustomerId,OrderDate,TotalAmount) VALUES
      (4,DATEADD(day,-5,GETDATE()),115970),(6,DATEADD(day,-90,GETDATE()),54990),
      (8,DATEADD(day,-12,GETDATE()),50960),(4,DATEADD(day,-20,GETDATE()),179980),
      (6,DATEADD(day,-3,GETDATE()),79910),(8,DATEADD(day,-25,GETDATE()),12990);
    INSERT INTO dbo.OrderItems(OrderId,ProductId,Quantity) VALUES
      (1,1,1),(1,3,2),(2,2,1),(3,3,3),(3,6,2),(4,1,2),(5,7,4),(5,12,5),(6,3,1);
END
GO