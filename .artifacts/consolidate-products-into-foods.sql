START TRANSACTION;
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `categories` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `ParentId` bigint unsigned NULL,
        `Code` varchar(50) NOT NULL,
        `Name` varchar(150) NOT NULL,
        `Description` varchar(500) NULL,
        `DisplayOrder` int NOT NULL DEFAULT 0,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_categories_not_self_parent` CHECK (ParentId IS NULL OR ParentId <> Id),
        CONSTRAINT `fk_categories_parent` FOREIGN KEY (`ParentId`) REFERENCES `categories` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `promotion_codes` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `Code` varchar(50) NOT NULL,
        `Name` varchar(200) NOT NULL,
        `Description` varchar(1000) NULL,
        `DiscountType` varchar(30) NOT NULL,
        `DiscountValue` decimal(18,2) NOT NULL,
        `MinOrderAmount` decimal(18,2) NULL,
        `MaxDiscountAmount` decimal(18,2) NULL,
        `StartDate` datetime(6) NOT NULL,
        `EndDate` datetime(6) NOT NULL,
        `UsageLimit` int NULL,
        `UsageCount` int NOT NULL DEFAULT 0,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_promotion_codes_dates` CHECK (EndDate > StartDate),
        CONSTRAINT `chk_promotion_codes_type` CHECK (DiscountType IN ('Percentage','FixedAmount')),
        CONSTRAINT `chk_promotion_codes_usage` CHECK (UsageLimit IS NULL OR UsageLimit >= 0),
        CONSTRAINT `chk_promotion_codes_usage_count` CHECK (UsageCount >= 0),
        CONSTRAINT `chk_promotion_codes_value` CHECK (DiscountValue > 0)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `restaurant_areas` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `Code` varchar(50) NOT NULL,
        `Name` varchar(150) NOT NULL,
        `Description` varchar(500) NULL,
        `DisplayOrder` int NOT NULL DEFAULT 0,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`)
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `foods` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `CategoryId` bigint unsigned NOT NULL,
        `Code` varchar(50) NOT NULL,
        `Name` varchar(200) NOT NULL,
        `Description` text NULL,
        `ImageUrl` varchar(1000) NULL,
        `DisplayOrder` int NOT NULL DEFAULT 0,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `fk_foods_category` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    DROP PROCEDURE IF EXISTS sp_get_restaurant_products;
    DROP VIEW IF EXISTS restaurant_products_view;

    INSERT INTO categories
        (Code, Name, Description, DisplayOrder, IsActive, CreatedDate, UpdatedDate)
    SELECT 'LEGACY', 'Chưa phân loại',
           'Danh mục hệ thống dùng để tiếp nhận dữ liệu Product cũ.',
           0, TRUE, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)
    WHERE EXISTS (SELECT 1 FROM restaurant_products)
      AND NOT EXISTS (SELECT 1 FROM categories WHERE Code = 'LEGACY');

    INSERT INTO foods
        (CategoryId, Code, Name, DisplayOrder, IsActive, CreatedDate, UpdatedDate)
    SELECT c.Id, p.Code, LEFT(p.Name, 200), 0, TRUE,
           CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)
    FROM restaurant_products p
    JOIN categories c ON c.Code = 'LEGACY'
    WHERE NOT EXISTS (SELECT 1 FROM foods f WHERE f.Code = p.Code);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    DROP TABLE `restaurant_products`;
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `promotion_categories` (
        `PromotionId` bigint unsigned NOT NULL,
        `CategoryId` bigint unsigned NOT NULL,
        PRIMARY KEY (`PromotionId`, `CategoryId`),
        CONSTRAINT `fk_promotion_categories_category` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `fk_promotion_categories_promotion` FOREIGN KEY (`PromotionId`) REFERENCES `promotion_codes` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `restaurant_tables` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `AreaId` bigint unsigned NOT NULL,
        `Code` varchar(50) NOT NULL,
        `Name` varchar(150) NOT NULL,
        `Capacity` int NOT NULL,
        `Status` varchar(30) NOT NULL DEFAULT 'Available',
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_restaurant_tables_capacity` CHECK (Capacity > 0),
        CONSTRAINT `chk_restaurant_tables_status` CHECK (Status IN ('Available','Occupied','Reserved','Cleaning','Disabled')),
        CONSTRAINT `fk_restaurant_tables_area` FOREIGN KEY (`AreaId`) REFERENCES `restaurant_areas` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `food_variants` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `FoodId` bigint unsigned NOT NULL,
        `Code` varchar(50) NOT NULL,
        `Name` varchar(100) NOT NULL,
        `CurrentPrice` decimal(18,2) NOT NULL DEFAULT 0.0,
        `IsDefault` tinyint(1) NOT NULL DEFAULT FALSE,
        `IsAvailable` tinyint(1) NOT NULL DEFAULT TRUE,
        `SoldOutReason` varchar(500) NULL,
        `DisplayOrder` int NOT NULL DEFAULT 0,
        `IsActive` tinyint(1) NOT NULL DEFAULT TRUE,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_food_variants_price` CHECK (CurrentPrice >= 0),
        CONSTRAINT `fk_food_variants_food` FOREIGN KEY (`FoodId`) REFERENCES `foods` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `promotion_foods` (
        `PromotionId` bigint unsigned NOT NULL,
        `FoodId` bigint unsigned NOT NULL,
        PRIMARY KEY (`PromotionId`, `FoodId`),
        CONSTRAINT `fk_promotion_foods_food` FOREIGN KEY (`FoodId`) REFERENCES `foods` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `fk_promotion_foods_promotion` FOREIGN KEY (`PromotionId`) REFERENCES `promotion_codes` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `table_sessions` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `TableId` bigint unsigned NOT NULL,
        `GuestCount` int NOT NULL DEFAULT 1,
        `Status` varchar(30) NOT NULL DEFAULT 'Open',
        `OpenedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `ClosedDate` datetime(6) NULL,
        `OpenedBy` bigint unsigned NULL,
        `ClosedBy` bigint unsigned NULL,
        `Note` varchar(500) NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_table_sessions_guest_count` CHECK (GuestCount > 0),
        CONSTRAINT `chk_table_sessions_status` CHECK (Status IN ('Open','Closed','Cancelled')),
        CONSTRAINT `fk_table_sessions_table` FOREIGN KEY (`TableId`) REFERENCES `restaurant_tables` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `food_price_histories` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `FoodVariantId` bigint unsigned NOT NULL,
        `Price` decimal(18,2) NOT NULL,
        `EffectiveFrom` datetime(6) NOT NULL,
        `EffectiveTo` datetime(6) NULL,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_food_price_histories_dates` CHECK (EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom),
        CONSTRAINT `chk_food_price_histories_price` CHECK (Price >= 0),
        CONSTRAINT `fk_food_price_histories_variant` FOREIGN KEY (`FoodVariantId`) REFERENCES `food_variants` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `orders` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `OrderNo` varchar(50) NOT NULL,
        `TableSessionId` bigint unsigned NULL,
        `CustomerId` bigint unsigned NULL,
        `OrderType` varchar(30) NOT NULL DEFAULT 'DineIn',
        `Status` varchar(30) NOT NULL DEFAULT 'Pending',
        `SubtotalAmount` decimal(18,2) NOT NULL DEFAULT 0.0,
        `DiscountAmount` decimal(18,2) NOT NULL DEFAULT 0.0,
        `TaxAmount` decimal(18,2) NOT NULL DEFAULT 0.0,
        `TotalAmount` decimal(18,2) NOT NULL DEFAULT 0.0,
        `Note` varchar(1000) NULL,
        `OrderedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `CompletedDate` datetime(6) NULL,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_orders_amounts` CHECK (SubtotalAmount >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND TotalAmount >= 0),
        CONSTRAINT `chk_orders_status` CHECK (Status IN ('Draft','Pending','Confirmed','Serving','Completed','Cancelled')),
        CONSTRAINT `chk_orders_type` CHECK (OrderType IN ('DineIn','TakeAway','Delivery')),
        CONSTRAINT `fk_orders_table_session` FOREIGN KEY (`TableSessionId`) REFERENCES `table_sessions` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `payments` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `TableSessionId` bigint unsigned NOT NULL,
        `PaymentMethod` varchar(30) NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `Status` varchar(30) NOT NULL DEFAULT 'Pending',
        `TransactionNo` varchar(100) NULL,
        `PaidDate` datetime(6) NULL,
        `Note` varchar(500) NULL,
        `CreatedBy` bigint unsigned NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedBy` bigint unsigned NULL,
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_payments_amount` CHECK (Amount > 0),
        CONSTRAINT `chk_payments_method` CHECK (PaymentMethod IN ('Cash','Card','BankTransfer','Momo','VNPay','Other')),
        CONSTRAINT `chk_payments_status` CHECK (Status IN ('Pending','Paid','Failed','Refunded','Cancelled')),
        CONSTRAINT `fk_payments_table_session` FOREIGN KEY (`TableSessionId`) REFERENCES `table_sessions` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `kitchen_orders` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `OrderId` bigint unsigned NOT NULL,
        `KitchenNo` varchar(50) NOT NULL,
        `Status` varchar(30) NOT NULL DEFAULT 'Pending',
        `SentToKitchenDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `AcceptedBy` bigint unsigned NULL,
        `AcceptedDate` datetime(6) NULL,
        `StartedDate` datetime(6) NULL,
        `ReadyDate` datetime(6) NULL,
        `CompletedDate` datetime(6) NULL,
        `Note` varchar(500) NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_kitchen_orders_status` CHECK (Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled')),
        CONSTRAINT `fk_kitchen_orders_order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `order_items` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `OrderId` bigint unsigned NOT NULL,
        `FoodId` bigint unsigned NOT NULL,
        `FoodVariantId` bigint unsigned NOT NULL,
        `FoodCode` varchar(50) NOT NULL,
        `FoodName` varchar(200) NOT NULL,
        `VariantName` varchar(100) NOT NULL,
        `Quantity` decimal(18,3) NOT NULL,
        `UnitPrice` decimal(18,2) NOT NULL,
        `DiscountAmount` decimal(18,2) NOT NULL DEFAULT 0.0,
        `TotalAmount` decimal(18,2) NOT NULL,
        `Note` varchar(500) NULL,
        `Status` varchar(30) NOT NULL DEFAULT 'Pending',
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_order_items_amounts` CHECK (UnitPrice >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0),
        CONSTRAINT `chk_order_items_quantity` CHECK (Quantity > 0),
        CONSTRAINT `chk_order_items_status` CHECK (Status IN ('Pending','Accepted','Preparing','Ready','Served','Completed','Cancelled','Rejected')),
        CONSTRAINT `fk_order_items_food` FOREIGN KEY (`FoodId`) REFERENCES `foods` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_order_items_order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `fk_order_items_variant` FOREIGN KEY (`FoodVariantId`) REFERENCES `food_variants` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `order_promotions` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `OrderId` bigint unsigned NOT NULL,
        `PromotionId` bigint unsigned NOT NULL,
        `PromotionCode` varchar(50) NOT NULL,
        `PromotionName` varchar(200) NOT NULL,
        `DiscountAmount` decimal(18,2) NOT NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_order_promotions_discount` CHECK (DiscountAmount >= 0),
        CONSTRAINT `fk_order_promotions_order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `fk_order_promotions_promotion` FOREIGN KEY (`PromotionId`) REFERENCES `promotion_codes` (`Id`) ON DELETE RESTRICT
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `order_status_histories` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `OrderId` bigint unsigned NOT NULL,
        `FromStatus` varchar(30) NULL,
        `ToStatus` varchar(30) NOT NULL,
        `Note` varchar(500) NULL,
        `ChangedBy` bigint unsigned NULL,
        `ChangedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `fk_order_status_histories_order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `payment_allocations` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `PaymentId` bigint unsigned NOT NULL,
        `OrderId` bigint unsigned NOT NULL,
        `Amount` decimal(18,2) NOT NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_payment_allocations_amount` CHECK (Amount > 0),
        CONSTRAINT `fk_payment_allocations_order` FOREIGN KEY (`OrderId`) REFERENCES `orders` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_payment_allocations_payment` FOREIGN KEY (`PaymentId`) REFERENCES `payments` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE TABLE `kitchen_order_items` (
        `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
        `KitchenOrderId` bigint unsigned NOT NULL,
        `OrderItemId` bigint unsigned NOT NULL,
        `Quantity` decimal(18,3) NOT NULL,
        `Status` varchar(30) NOT NULL DEFAULT 'Pending',
        `StartedDate` datetime(6) NULL,
        `ReadyDate` datetime(6) NULL,
        `CompletedDate` datetime(6) NULL,
        `Note` varchar(500) NULL,
        `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
        `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
        PRIMARY KEY (`Id`),
        CONSTRAINT `chk_kitchen_order_items_quantity` CHECK (Quantity > 0),
        CONSTRAINT `chk_kitchen_order_items_status` CHECK (Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled')),
        CONSTRAINT `fk_kitchen_order_items_kitchen_order` FOREIGN KEY (`KitchenOrderId`) REFERENCES `kitchen_orders` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `fk_kitchen_order_items_order_item` FOREIGN KEY (`OrderItemId`) REFERENCES `order_items` (`Id`) ON DELETE CASCADE
    );
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_categories_active_order` ON `categories` (`IsActive`, `DisplayOrder`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_categories_parent_order` ON `categories` (`ParentId`, `DisplayOrder`, `Name`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_categories_code` ON `categories` (`Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_food_price_histories_variant_dates` ON `food_price_histories` (`FoodVariantId`, `EffectiveFrom`, `EffectiveTo`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_food_variants_food_available` ON `food_variants` (`FoodId`, `IsActive`, `IsAvailable`, `DisplayOrder`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_food_variants_food_code` ON `food_variants` (`FoodId`, `Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_foods_category_active` ON `foods` (`CategoryId`, `IsActive`, `DisplayOrder`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_foods_code` ON `foods` (`Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_kitchen_order_items_order_item` ON `kitchen_order_items` (`OrderItemId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_kitchen_order_items_status` ON `kitchen_order_items` (`Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_kitchen_order_items_pair` ON `kitchen_order_items` (`KitchenOrderId`, `OrderItemId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_kitchen_orders_order` ON `kitchen_orders` (`OrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_kitchen_orders_status_sent` ON `kitchen_orders` (`Status`, `SentToKitchenDate`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_kitchen_orders_kitchen_no` ON `kitchen_orders` (`KitchenNo`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_order_items_food` ON `order_items` (`FoodId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_order_items_order_status` ON `order_items` (`OrderId`, `Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_order_items_variant` ON `order_items` (`FoodVariantId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_order_promotions_promotion` ON `order_promotions` (`PromotionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_order_promotions_order_promotion` ON `order_promotions` (`OrderId`, `PromotionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_order_status_histories_order_date` ON `order_status_histories` (`OrderId`, `ChangedDate`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_orders_customer` ON `orders` (`CustomerId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_orders_ordered_date_status` ON `orders` (`OrderedDate`, `Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_orders_table_session` ON `orders` (`TableSessionId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_orders_order_no` ON `orders` (`OrderNo`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_payment_allocations_order` ON `payment_allocations` (`OrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_payment_allocations_payment_order` ON `payment_allocations` (`PaymentId`, `OrderId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_payments_paid_date_status` ON `payments` (`PaidDate`, `Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_payments_session_status` ON `payments` (`TableSessionId`, `Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_payments_transaction_no` ON `payments` (`TransactionNo`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_promotion_categories_category` ON `promotion_categories` (`CategoryId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_promotion_codes_validity` ON `promotion_codes` (`IsActive`, `StartDate`, `EndDate`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_promotion_codes_code` ON `promotion_codes` (`Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_promotion_foods_food` ON `promotion_foods` (`FoodId`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_restaurant_areas_active_order` ON `restaurant_areas` (`IsActive`, `DisplayOrder`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_restaurant_areas_code` ON `restaurant_areas` (`Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_restaurant_tables_area_status` ON `restaurant_tables` (`AreaId`, `Status`, `IsActive`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE UNIQUE INDEX `uk_restaurant_tables_code` ON `restaurant_tables` (`Code`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_table_sessions_opened_date` ON `table_sessions` (`OpenedDate`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    CREATE INDEX `ix_table_sessions_table_status` ON `table_sessions` (`TableId`, `Status`);
END;

IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260831040821_ConsolidateProductsIntoFoods')
BEGIN
    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260831040821_ConsolidateProductsIntoFoods', '10.0.7');
END;

COMMIT;

