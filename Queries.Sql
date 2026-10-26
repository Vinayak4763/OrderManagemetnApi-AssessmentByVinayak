-- 1. Products with low or zero stock.
SELECT
    Id,
    Sku,
    Name,
    Price,
    StockQuantity
FROM Products
WHERE IsActive = 1
  AND StockQuantity <= 5
ORDER BY StockQuantity ASC, Name ASC;


-- 2. Recent orders with customer information.
SELECT
    o.Id AS OrderId,
    u.FullName AS CustomerName,
    u.Email AS CustomerEmail,
    o.Status,
    o.SubTotal,
    o.Discount,
    o.Total,
    o.CreatedAt
FROM Orders AS o
INNER JOIN Users AS u ON u.Id = o.CustomerId
WHERE o.CreatedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
ORDER BY o.CreatedAt DESC;


-- 3. Line items for a specified order.
DECLARE @OrderId INT = 1;

SELECT
    oi.OrderId,
    p.Sku,
    p.Name AS ProductName,
    oi.Quantity,
    oi.UnitPrice,
    oi.Quantity * oi.UnitPrice AS LineTotal
FROM OrderItems AS oi
INNER JOIN Products AS p ON p.Id = oi.ProductId
WHERE oi.OrderId = @OrderId
ORDER BY p.Name;


-- 4. Best-selling products by quantity in the last 30 days.
SELECT TOP (10)
    p.Id AS ProductId,
    p.Sku,
    p.Name,
    SUM(oi.Quantity) AS UnitsSold
FROM OrderItems AS oi
INNER JOIN Orders AS o ON o.Id = oi.OrderId
INNER JOIN Products AS p ON p.Id = oi.ProductId
WHERE o.CreatedAt >= DATEADD(DAY, -30, SYSUTCDATETIME())
  AND o.Status IN (1, 2) -- Adjust if OrderStatus is stored as text.
GROUP BY p.Id, p.Sku, p.Name
ORDER BY UnitsSold DESC, p.Name ASC;


-- 5. Top customers by spending over a configurable period.
-- This is one SQL query suitable for the top-customers report.
DECLARE @Days INT = 30;

SELECT TOP (10)
    u.Id AS CustomerId,
    u.FullName,
    u.Email,
    COUNT(o.Id) AS OrderCount,
    SUM(o.Total) AS TotalSpent
FROM Users AS u
INNER JOIN Orders AS o ON o.CustomerId = u.Id
WHERE o.CreatedAt >= DATEADD(DAY, -@Days, SYSUTCDATETIME())
  AND o.Status IN (1, 2) -- Adjust if OrderStatus is stored as text.
GROUP BY u.Id, u.FullName, u.Email
ORDER BY TotalSpent DESC, OrderCount DESC;