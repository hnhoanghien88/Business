-- ============================================================
-- Restaurant Management System - MySQL 8.x
-- Generated: 2026-08-26
-- Charset: utf8mb4
-- ============================================================

CREATE DATABASE IF NOT EXISTS restaurant_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE restaurant_db;

SET NAMES utf8mb4;
SET time_zone = '+07:00';

-- NOTE:
-- 1) CreatedBy / UpdatedBy / *_by fields intentionally do not have a FK.
--    They can store user IDs from a separate Identity service/database.
-- 2) Money uses DECIMAL(18,2).
-- 3) Status columns are VARCHAR so the workflow can evolve without ALTER ENUM.

-- ============================================================
-- 1. RESTAURANT LAYOUT
-- ============================================================

CREATE TABLE IF NOT EXISTS restaurant_areas (
    Id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    Code            VARCHAR(50) NOT NULL,
    Name            VARCHAR(150) NOT NULL,
    Description     VARCHAR(500) NULL,
    DisplayOrder   INT NOT NULL DEFAULT 0,
    IsActive       TINYINT(1) NOT NULL DEFAULT 1,
    CreatedBy      BIGINT UNSIGNED NULL,
    CreatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy      BIGINT UNSIGNED NULL,
    UpdatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_restaurant_areas_code (Code),
    KEY ix_restaurant_areas_active_order (IsActive, DisplayOrder)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_tables (
    Id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    AreaId         BIGINT UNSIGNED NOT NULL,
    Code            VARCHAR(50) NOT NULL,
    Name            VARCHAR(150) NOT NULL,
    Capacity        INT NOT NULL,
    Status          VARCHAR(30) NOT NULL DEFAULT 'Available',
    IsActive       TINYINT(1) NOT NULL DEFAULT 1,
    CreatedBy      BIGINT UNSIGNED NULL,
    CreatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy      BIGINT UNSIGNED NULL,
    UpdatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_restaurant_tables_code (Code),
    KEY ix_restaurant_tables_area_status (AreaId, Status, IsActive),
    CONSTRAINT fk_restaurant_tables_area
        FOREIGN KEY (AreaId) REFERENCES restaurant_areas(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_restaurant_tables_capacity CHECK (Capacity > 0),
    CONSTRAINT chk_restaurant_tables_status CHECK (Status IN ('Available','Occupied','Reserved','Cleaning','Disabled'))
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_table_sessions (
    Id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    TableId        BIGINT UNSIGNED NOT NULL,
    GuestCount     INT NOT NULL DEFAULT 1,
    Status          VARCHAR(30) NOT NULL DEFAULT 'Open',
    OpenedDate     DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    ClosedDate     DATETIME(6) NULL,
    OpenedBy       BIGINT UNSIGNED NULL,
    ClosedBy       BIGINT UNSIGNED NULL,
    Note            VARCHAR(500) NULL,
    CreatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    KEY ix_table_sessions_table_status (TableId, Status),
    KEY ix_table_sessions_opened_date (OpenedDate),
    CONSTRAINT fk_table_sessions_table
        FOREIGN KEY (TableId) REFERENCES restaurant_tables(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_table_sessions_guest_count CHECK (GuestCount > 0),
    CONSTRAINT chk_table_sessions_status CHECK (Status IN ('Open','Closed','Cancelled'))
) ENGINE=InnoDB;

-- ============================================================
-- 2. MENU / CATALOG
-- ============================================================

CREATE TABLE IF NOT EXISTS restaurant_categories (
    Id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ParentId        BIGINT UNSIGNED NULL,
    Code            VARCHAR(50) NOT NULL,
    Name            VARCHAR(150) NOT NULL,
    Description     VARCHAR(500) NULL,
    DisplayOrder   INT NOT NULL DEFAULT 0,
    IsActive       TINYINT(1) NOT NULL DEFAULT 1,
    CreatedBy      BIGINT UNSIGNED NULL,
    CreatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy      BIGINT UNSIGNED NULL,
    UpdatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_categories_code (Code),
    KEY ix_categories_active_order (IsActive, DisplayOrder),
    KEY ix_categories_parent_order (ParentId, DisplayOrder, Name),
    CONSTRAINT fk_categories_parent
        FOREIGN KEY (ParentId) REFERENCES restaurant_categories(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_categories_not_self_parent CHECK (ParentId IS NULL OR ParentId <> Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_foods (
    Id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    CategoryId     BIGINT UNSIGNED NOT NULL,
    Code            VARCHAR(50) NOT NULL,
    Name            VARCHAR(200) NOT NULL,
    Description     TEXT NULL,
    ImageUrl       VARCHAR(1000) NULL,
    DisplayOrder   INT NOT NULL DEFAULT 0,
    IsActive       TINYINT(1) NOT NULL DEFAULT 1,
    CreatedBy      BIGINT UNSIGNED NULL,
    CreatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy      BIGINT UNSIGNED NULL,
    UpdatedDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_foods_code (Code),
    KEY ix_foods_category_active (CategoryId, IsActive, DisplayOrder),
    CONSTRAINT fk_foods_category
        FOREIGN KEY (CategoryId) REFERENCES restaurant_categories(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_food_variants (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    FoodId             BIGINT UNSIGNED NOT NULL,
    Code                VARCHAR(50) NOT NULL,
    Name                VARCHAR(100) NOT NULL,
    CurrentPrice       DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    IsDefault          TINYINT(1) NOT NULL DEFAULT 0,
    IsAvailable        TINYINT(1) NOT NULL DEFAULT 1,
    SoldOutReason     VARCHAR(500) NULL,
    DisplayOrder       INT NOT NULL DEFAULT 0,
    IsActive           TINYINT(1) NOT NULL DEFAULT 1,
    CreatedBy          BIGINT UNSIGNED NULL,
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy          BIGINT UNSIGNED NULL,
    UpdatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_food_variants_food_code (FoodId, Code),
    KEY ix_food_variants_food_available (FoodId, IsActive, IsAvailable, DisplayOrder),
    CONSTRAINT fk_food_variants_food
        FOREIGN KEY (FoodId) REFERENCES restaurant_foods(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_food_variants_price CHECK (CurrentPrice >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_food_price_histories (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    FoodVariantId     BIGINT UNSIGNED NOT NULL,
    Price               DECIMAL(18,2) NOT NULL,
    EffectiveFrom      DATETIME(6) NOT NULL,
    EffectiveTo        DATETIME(6) NULL,
    CreatedBy          BIGINT UNSIGNED NULL,
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    KEY ix_food_price_histories_variant_dates (FoodVariantId, EffectiveFrom, EffectiveTo),
    CONSTRAINT fk_food_price_histories_variant
        FOREIGN KEY (FoodVariantId) REFERENCES restaurant_food_variants(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_food_price_histories_price CHECK (Price >= 0),
    CONSTRAINT chk_food_price_histories_dates CHECK (EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom)
) ENGINE=InnoDB;

-- ============================================================
-- 3. PROMOTIONS
-- ============================================================

CREATE TABLE IF NOT EXISTS restaurant_promotion_codes (
    Id                      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    Code                    VARCHAR(50) NOT NULL,
    Name                    VARCHAR(200) NOT NULL,
    Description             VARCHAR(1000) NULL,
    DiscountType           VARCHAR(30) NOT NULL,
    DiscountValue          DECIMAL(18,2) NOT NULL,
    MinOrderAmount        DECIMAL(18,2) NULL,
    MaxDiscountAmount     DECIMAL(18,2) NULL,
    StartDate              DATETIME(6) NOT NULL,
    EndDate                DATETIME(6) NOT NULL,
    UsageLimit             INT NULL,
    UsageCount             INT NOT NULL DEFAULT 0,
    IsActive               TINYINT(1) NOT NULL DEFAULT 1,
    CreatedBy              BIGINT UNSIGNED NULL,
    CreatedDate            DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy              BIGINT UNSIGNED NULL,
    UpdatedDate            DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_promotion_codes_code (Code),
    KEY ix_promotion_codes_validity (IsActive, StartDate, EndDate),
    CONSTRAINT chk_promotion_codes_type CHECK (DiscountType IN ('Percentage','FixedAmount')),
    CONSTRAINT chk_promotion_codes_value CHECK (DiscountValue > 0),
    CONSTRAINT chk_promotion_codes_dates CHECK (EndDate > StartDate),
    CONSTRAINT chk_promotion_codes_usage CHECK (UsageLimit IS NULL OR UsageLimit >= 0),
    CONSTRAINT chk_promotion_codes_usage_count CHECK (UsageCount >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_promotion_foods (
    PromotionId       BIGINT UNSIGNED NOT NULL,
    FoodId            BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (PromotionId, FoodId),
    KEY ix_promotion_foods_food (FoodId),
    CONSTRAINT fk_promotion_foods_promotion
        FOREIGN KEY (PromotionId) REFERENCES restaurant_promotion_codes(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_promotion_foods_food
        FOREIGN KEY (FoodId) REFERENCES restaurant_foods(Id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_promotion_categories (
    PromotionId       BIGINT UNSIGNED NOT NULL,
    CategoryId        BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (PromotionId, CategoryId),
    KEY ix_promotion_categories_category (CategoryId),
    CONSTRAINT fk_promotion_categories_promotion
        FOREIGN KEY (PromotionId) REFERENCES restaurant_promotion_codes(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_promotion_categories_category
        FOREIGN KEY (CategoryId) REFERENCES restaurant_categories(Id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 4. ORDERS
-- ============================================================

CREATE TABLE IF NOT EXISTS restaurant_orders (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OrderNo            VARCHAR(50) NOT NULL,
    TableSessionId    BIGINT UNSIGNED NULL,
    CustomerId         BIGINT UNSIGNED NULL,
    OrderType          VARCHAR(30) NOT NULL DEFAULT 'DineIn',
    Status              VARCHAR(30) NOT NULL DEFAULT 'Pending',
    SubtotalAmount     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    DiscountAmount     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TaxAmount          DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TotalAmount        DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Note                VARCHAR(1000) NULL,
    OrderedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CompletedDate      DATETIME(6) NULL,
    CreatedBy          BIGINT UNSIGNED NULL,
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy          BIGINT UNSIGNED NULL,
    UpdatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_orders_order_no (OrderNo),
    KEY ix_orders_ordered_date_status (OrderedDate, Status),
    KEY ix_orders_table_session (TableSessionId),
    KEY ix_orders_customer (CustomerId),
    CONSTRAINT fk_orders_table_session
        FOREIGN KEY (TableSessionId) REFERENCES restaurant_table_sessions(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_orders_type CHECK (OrderType IN ('DineIn','TakeAway','Delivery')),
    CONSTRAINT chk_orders_status CHECK (Status IN ('Draft','Pending','Confirmed','Serving','Completed','Cancelled')),
    CONSTRAINT chk_orders_amounts CHECK (
        SubtotalAmount >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND TotalAmount >= 0
    )
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_order_items (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OrderId            BIGINT UNSIGNED NOT NULL,
    FoodId             BIGINT UNSIGNED NOT NULL,
    FoodVariantId     BIGINT UNSIGNED NOT NULL,
    FoodCode           VARCHAR(50) NOT NULL,
    FoodName           VARCHAR(200) NOT NULL,
    VariantName        VARCHAR(100) NOT NULL,
    Quantity            DECIMAL(18,3) NOT NULL,
    UnitPrice          DECIMAL(18,2) NOT NULL,
    DiscountAmount     DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    TotalAmount        DECIMAL(18,2) NOT NULL,
    Note                VARCHAR(500) NULL,
    Status              VARCHAR(30) NOT NULL DEFAULT 'Pending',
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    KEY ix_order_items_order_status (OrderId, Status),
    KEY ix_order_items_food (FoodId),
    KEY ix_order_items_variant (FoodVariantId),
    CONSTRAINT fk_order_items_order
        FOREIGN KEY (OrderId) REFERENCES restaurant_orders(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_order_items_food
        FOREIGN KEY (FoodId) REFERENCES restaurant_foods(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_order_items_variant
        FOREIGN KEY (FoodVariantId) REFERENCES restaurant_food_variants(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_order_items_quantity CHECK (Quantity > 0),
    CONSTRAINT chk_order_items_amounts CHECK (UnitPrice >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0),
    CONSTRAINT chk_order_items_status CHECK (Status IN ('Pending','Accepted','Preparing','Ready','Served','Completed','Cancelled','Rejected'))
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_order_promotions (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OrderId            BIGINT UNSIGNED NOT NULL,
    PromotionId        BIGINT UNSIGNED NOT NULL,
    PromotionCode      VARCHAR(50) NOT NULL,
    PromotionName      VARCHAR(200) NOT NULL,
    DiscountAmount     DECIMAL(18,2) NOT NULL,
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_order_promotions_order_promotion (OrderId, PromotionId),
    KEY ix_order_promotions_promotion (PromotionId),
    CONSTRAINT fk_order_promotions_order
        FOREIGN KEY (OrderId) REFERENCES restaurant_orders(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_order_promotions_promotion
        FOREIGN KEY (PromotionId) REFERENCES restaurant_promotion_codes(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_order_promotions_discount CHECK (DiscountAmount >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_order_status_histories (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OrderId            BIGINT UNSIGNED NOT NULL,
    FromStatus         VARCHAR(30) NULL,
    ToStatus           VARCHAR(30) NOT NULL,
    Note                VARCHAR(500) NULL,
    ChangedBy          BIGINT UNSIGNED NULL,
    ChangedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    KEY ix_order_status_histories_order_date (OrderId, ChangedDate),
    CONSTRAINT fk_order_status_histories_order
        FOREIGN KEY (OrderId) REFERENCES restaurant_orders(Id)
        ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ============================================================
-- 5. KITCHEN
-- ============================================================

CREATE TABLE IF NOT EXISTS restaurant_kitchen_orders (
    Id                      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OrderId                BIGINT UNSIGNED NOT NULL,
    KitchenNo              VARCHAR(50) NOT NULL,
    Status                  VARCHAR(30) NOT NULL DEFAULT 'Pending',
    SentToKitchenDate    DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    AcceptedBy             BIGINT UNSIGNED NULL,
    AcceptedDate           DATETIME(6) NULL,
    StartedDate            DATETIME(6) NULL,
    ReadyDate              DATETIME(6) NULL,
    CompletedDate          DATETIME(6) NULL,
    Note                    VARCHAR(500) NULL,
    CreatedDate            DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedDate            DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_kitchen_orders_kitchen_no (KitchenNo),
    KEY ix_kitchen_orders_order (OrderId),
    KEY ix_kitchen_orders_status_sent (Status, SentToKitchenDate),
    CONSTRAINT fk_kitchen_orders_order
        FOREIGN KEY (OrderId) REFERENCES restaurant_orders(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT chk_kitchen_orders_status CHECK (Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled'))
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS restaurant_kitchen_order_items (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    KitchenOrderId    BIGINT UNSIGNED NOT NULL,
    OrderItemId       BIGINT UNSIGNED NOT NULL,
    Quantity            DECIMAL(18,3) NOT NULL,
    Status              VARCHAR(30) NOT NULL DEFAULT 'Pending',
    StartedDate        DATETIME(6) NULL,
    ReadyDate          DATETIME(6) NULL,
    CompletedDate      DATETIME(6) NULL,
    Note                VARCHAR(500) NULL,
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    -- Một OrderItem có thể nằm ở nhiều KitchenOrder khi gửi bếp từng phần.
    -- Trong cùng một phiếu bếp, mỗi OrderItem chỉ xuất hiện một lần.
    UNIQUE KEY uk_kitchen_order_items_pair (KitchenOrderId, OrderItemId),
    KEY ix_kitchen_order_items_status (Status),
    KEY ix_kitchen_order_items_order_item (OrderItemId),
    CONSTRAINT fk_kitchen_order_items_kitchen_order
        FOREIGN KEY (KitchenOrderId) REFERENCES restaurant_kitchen_orders(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_kitchen_order_items_order_item
        FOREIGN KEY (OrderItemId) REFERENCES restaurant_order_items(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT chk_kitchen_order_items_quantity CHECK (Quantity > 0),
    CONSTRAINT chk_kitchen_order_items_status CHECK (Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled'))
) ENGINE=InnoDB;

-- Số lượng gửi bếp là dữ liệu suy ra, không lưu lặp trên restaurant_order_items.
-- Khi gửi bếp, application phải SELECT ... FOR UPDATE restaurant_order_items tương ứng,
-- tính RemainingQuantity và insert kitchen_order + items trong cùng transaction.
-- Cancelled/Rejected được giải phóng để có thể gửi lại; các trạng thái khác giữ chỗ.
CREATE OR REPLACE VIEW restaurant_order_item_kitchen_quantities AS
SELECT
    oi.Id AS OrderItemId,
    oi.OrderId,
    oi.Quantity AS OrderedQuantity,
    COALESCE(SUM(CASE
        WHEN koi.Status NOT IN ('Cancelled', 'Rejected') THEN koi.Quantity
        ELSE 0
    END), 0) AS SentQuantity,
    oi.Quantity - COALESCE(SUM(CASE
        WHEN koi.Status NOT IN ('Cancelled', 'Rejected') THEN koi.Quantity
        ELSE 0
    END), 0) AS RemainingQuantity
FROM restaurant_order_items oi
LEFT JOIN restaurant_kitchen_order_items koi ON koi.OrderItemId = oi.Id
GROUP BY oi.Id, oi.OrderId, oi.Quantity;

-- ============================================================
-- 6. PAYMENTS
-- ============================================================

CREATE TABLE IF NOT EXISTS restaurant_payments (
    Id                  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    TableSessionId     BIGINT UNSIGNED NOT NULL,
    PaymentMethod      VARCHAR(30) NOT NULL,
    Amount              DECIMAL(18,2) NOT NULL,
    Status              VARCHAR(30) NOT NULL DEFAULT 'Pending',
    TransactionNo      VARCHAR(100) NULL,
    PaidDate           DATETIME(6) NULL,
    Note                VARCHAR(500) NULL,
    CreatedBy          BIGINT UNSIGNED NULL,
    CreatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UpdatedBy          BIGINT UNSIGNED NULL,
    UpdatedDate        DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    KEY ix_payments_session_status (TableSessionId, Status),
    KEY ix_payments_paid_date_status (PaidDate, Status),
    KEY ix_payments_transaction_no (TransactionNo),
    CONSTRAINT fk_payments_table_session
        FOREIGN KEY (TableSessionId) REFERENCES restaurant_table_sessions(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_payments_method CHECK (PaymentMethod IN ('Cash','Card','BankTransfer','Momo','VNPay','Other')),
    CONSTRAINT chk_payments_status CHECK (Status IN ('Pending','Paid','Failed','Refunded','Cancelled')),
    CONSTRAINT chk_payments_amount CHECK (Amount > 0)
) ENGINE=InnoDB;

-- Một payment của lượt bàn có thể được phân bổ cho nhiều order.
-- Khi payment chuyển Paid, application phải kiểm tra trong một transaction:
-- tổng allocation = payment amount và tổng Paid của order không vượt total amount.
CREATE TABLE IF NOT EXISTS restaurant_payment_allocations (
    Id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PaymentId       BIGINT UNSIGNED NOT NULL,
    OrderId         BIGINT UNSIGNED NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    CreatedDate     DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (Id),
    UNIQUE KEY uk_payment_allocations_payment_order (PaymentId, OrderId),
    KEY ix_payment_allocations_order (OrderId),
    CONSTRAINT fk_payment_allocations_payment
        FOREIGN KEY (PaymentId) REFERENCES restaurant_payments(Id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT fk_payment_allocations_order
        FOREIGN KEY (OrderId) REFERENCES restaurant_orders(Id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_payment_allocations_amount CHECK (Amount > 0)
) ENGINE=InnoDB;

CREATE OR REPLACE VIEW restaurant_order_payment_balances AS
SELECT
    o.Id AS OrderId,
    o.TableSessionId,
    o.TotalAmount,
    COALESCE(pa.PaidAmount, 0) AS PaidAmount,
    GREATEST(o.TotalAmount - COALESCE(pa.PaidAmount, 0), 0) AS RemainingAmount
FROM restaurant_orders o
LEFT JOIN (
    SELECT
        a.OrderId,
        SUM(a.Amount) AS PaidAmount
    FROM restaurant_payment_allocations a
    INNER JOIN restaurant_payments p ON p.Id = a.PaymentId
    WHERE p.Status = 'Paid'
    GROUP BY a.OrderId
) pa ON pa.OrderId = o.Id;

CREATE OR REPLACE VIEW restaurant_table_session_payment_balances AS
SELECT
    ts.Id AS TableSessionId,
    COALESCE(ot.TotalAmount, 0) AS TotalAmount,
    COALESCE(pt.PaidAmount, 0) AS PaidAmount,
    GREATEST(COALESCE(ot.TotalAmount, 0) - COALESCE(pt.PaidAmount, 0), 0) AS RemainingAmount
FROM restaurant_table_sessions ts
LEFT JOIN (
    SELECT TableSessionId, SUM(TotalAmount) AS TotalAmount
    FROM restaurant_orders
    WHERE Status <> 'Cancelled'
    GROUP BY TableSessionId
) ot ON ot.TableSessionId = ts.Id
LEFT JOIN (
    SELECT TableSessionId, SUM(Amount) AS PaidAmount
    FROM restaurant_payments
    WHERE Status = 'Paid'
    GROUP BY TableSessionId
) pt ON pt.TableSessionId = ts.Id;

-- ============================================================
-- 7. OPTIONAL SEED DATA
-- ============================================================

INSERT INTO restaurant_areas (Code, Name, DisplayOrder)
SELECT 'MAIN', 'Khu vực chính', 1
WHERE NOT EXISTS (SELECT 1 FROM restaurant_areas WHERE Code = 'MAIN');

INSERT INTO restaurant_categories (Code, Name, DisplayOrder)
SELECT 'FOOD', 'Món ăn', 1
WHERE NOT EXISTS (SELECT 1 FROM restaurant_categories WHERE Code = 'FOOD');

INSERT INTO restaurant_categories (Code, Name, DisplayOrder)
SELECT 'DRINK', 'Đồ uống', 2
WHERE NOT EXISTS (SELECT 1 FROM restaurant_categories WHERE Code = 'DRINK');

-- ============================================================
-- 8. REPORT QUERY EXAMPLES
-- ============================================================

-- A. Daily order count / sales value
-- SELECT
--     DATE(OrderedDate) AS business_date,
--     COUNT(*) AS total_orders,
--     SUM(SubtotalAmount) AS gross_sales,
--     SUM(DiscountAmount) AS total_discount,
--     SUM(TotalAmount) AS net_sales
-- FROM restaurant_orders
-- WHERE Status = 'Completed'
-- GROUP BY DATE(OrderedDate)
-- ORDER BY business_date DESC;

-- B. Daily actual collected Amount
-- SELECT
--     DATE(PaidDate) AS business_date,
--     SUM(Amount) AS collected_amount
-- FROM restaurant_payments
-- WHERE Status = 'Paid'
-- GROUP BY DATE(PaidDate)
-- ORDER BY business_date DESC;

-- C. Best-selling restaurant_foods
-- SELECT
--     oi.FoodId,
--     oi.FoodName,
--     SUM(oi.Quantity) AS quantity_sold,
--     SUM(oi.TotalAmount) AS sales_amount
-- FROM restaurant_order_items oi
-- INNER JOIN restaurant_orders o ON o.Id = oi.OrderId
-- WHERE o.Status = 'Completed'
--   AND oi.Status <> 'Cancelled'
-- GROUP BY oi.FoodId, oi.FoodName
-- ORDER BY quantity_sold DESC;

-- D. Current occupied tables
-- SELECT
--     rt.Id,
--     rt.Code,
--     rt.Name,
--     rt.Capacity,
--     ts.Id AS TableSessionId,
--     ts.GuestCount,
--     ts.OpenedDate
-- FROM restaurant_tables rt
-- INNER JOIN restaurant_table_sessions ts
--     ON ts.TableId = rt.Id
--    AND ts.Status = 'Open'
-- WHERE rt.Status = 'Occupied';

-- ============================================================
-- END
-- ============================================================
