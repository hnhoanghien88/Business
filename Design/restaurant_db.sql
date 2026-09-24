-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: localhost    Database: restaurant_db
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260820081749_InitialDB','10.0.7'),('20260820082036_AddProduct','10.0.7'),('20260825041949_AddRateLimitPolicies','10.0.7'),('20260825053918_RestaurantInitial','10.0.7'),('20260831040821_ConsolidateProductsIntoFoods','10.0.7'),('20260831054105_PrefixRestaurantTables','10.0.7'),('20260831061226_RestaurantCategoriesManagement','10.0.7'),('20260903093543_RestaurantFoodsManagement','10.0.7'),('20260909055744_RestaurantLayoutsTableOperationsOrdering','10.0.7');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_areas`
--

DROP TABLE IF EXISTS `restaurant_areas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_areas` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_restaurant_areas_code` (`Code`),
  KEY `ix_restaurant_areas_active_order` (`IsActive`,`DisplayOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_areas`
--

LOCK TABLES `restaurant_areas` WRITE;
/*!40000 ALTER TABLE `restaurant_areas` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_areas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_categories`
--

DROP TABLE IF EXISTS `restaurant_categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_categories` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `ParentId` bigint unsigned DEFAULT NULL,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_categories_code` (`Code`),
  KEY `ix_categories_active_order` (`IsActive`,`DisplayOrder`),
  KEY `ix_categories_parent_order` (`ParentId`,`DisplayOrder`,`Name`,`Code`),
  CONSTRAINT `fk_categories_parent` FOREIGN KEY (`ParentId`) REFERENCES `restaurant_categories` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_categories`
--

LOCK TABLES `restaurant_categories` WRITE;
/*!40000 ALTER TABLE `restaurant_categories` DISABLE KEYS */;
INSERT INTO `restaurant_categories` VALUES (1,NULL,'DA','Đồ ăn','Đồ ăn',0,1,NULL,'2026-09-03 04:04:12.107644',NULL,'2026-09-03 04:04:12.107644'),(2,NULL,'DU','Đồ uống','Đồ uống',0,1,NULL,'2026-09-03 04:04:30.870771',NULL,'2026-09-03 04:04:30.870771'),(3,2,'TS','Trà sữa','Trà sữa',0,1,NULL,'2026-09-03 04:04:55.295933',NULL,'2026-09-03 04:04:55.295933'),(4,1,'BA','Bánh mì','Bánh mì',0,1,NULL,'2026-09-03 04:05:28.845668',NULL,'2026-09-03 04:05:28.845668');
/*!40000 ALTER TABLE `restaurant_categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_food_price_histories`
--

DROP TABLE IF EXISTS `restaurant_food_price_histories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_food_price_histories` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `FoodVariantId` bigint unsigned NOT NULL,
  `Price` decimal(18,2) NOT NULL,
  `EffectiveFrom` datetime(6) NOT NULL,
  `EffectiveTo` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `ix_food_price_histories_variant_dates` (`FoodVariantId`,`EffectiveFrom`,`EffectiveTo`),
  CONSTRAINT `fk_food_price_histories_variant` FOREIGN KEY (`FoodVariantId`) REFERENCES `restaurant_food_variants` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_food_price_histories_dates` CHECK (((`EffectiveTo` is null) or (`EffectiveTo` > `EffectiveFrom`))),
  CONSTRAINT `chk_food_price_histories_price` CHECK ((`Price` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_food_price_histories`
--

LOCK TABLES `restaurant_food_price_histories` WRITE;
/*!40000 ALTER TABLE `restaurant_food_price_histories` DISABLE KEYS */;
INSERT INTO `restaurant_food_price_histories` VALUES (1,1,30000.00,'2026-09-08 07:24:54.354542',NULL,1,'2026-09-08 07:24:54.354542'),(2,2,40000.00,'2026-09-08 07:25:26.123622',NULL,1,'2026-09-08 07:25:26.123622'),(3,3,20000.00,'2026-09-08 07:26:18.450960','2026-09-08 07:30:11.687962',1,'2026-09-08 07:26:18.450960'),(4,3,25000.00,'2026-09-08 07:30:11.687962',NULL,1,'2026-09-08 07:30:11.687962');
/*!40000 ALTER TABLE `restaurant_food_price_histories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_food_variants`
--

DROP TABLE IF EXISTS `restaurant_food_variants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_food_variants` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `FoodId` bigint unsigned NOT NULL,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `CurrentPrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsAvailable` tinyint(1) NOT NULL DEFAULT '1',
  `SoldOutReason` varchar(500) DEFAULT NULL,
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_food_variants_food_code` (`FoodId`,`Code`),
  KEY `ix_food_variants_food_available` (`FoodId`,`IsActive`,`IsAvailable`,`DisplayOrder`),
  CONSTRAINT `fk_food_variants_food` FOREIGN KEY (`FoodId`) REFERENCES `restaurant_foods` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_food_variants_price` CHECK ((`CurrentPrice` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_food_variants`
--

LOCK TABLES `restaurant_food_variants` WRITE;
/*!40000 ALTER TABLE `restaurant_food_variants` DISABLE KEYS */;
INSERT INTO `restaurant_food_variants` VALUES (1,1,'TS-01','M',30000.00,1,1,NULL,0,1,1,'2026-09-08 07:24:54.354542',1,'2026-09-08 07:24:54.354542'),(2,2,'TS02-1','M',40000.00,1,1,NULL,0,1,1,'2026-09-08 07:25:26.123622',1,'2026-09-08 07:25:26.123622'),(3,1,'TS01-2','L',25000.00,0,1,NULL,0,1,1,'2026-09-08 07:26:18.450960',1,'2026-09-08 07:30:11.704926');
/*!40000 ALTER TABLE `restaurant_food_variants` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_foods`
--

DROP TABLE IF EXISTS `restaurant_foods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_foods` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `CategoryId` bigint unsigned NOT NULL,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` text,
  `ImageUrl` varchar(1000) DEFAULT NULL,
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_foods_code` (`Code`),
  KEY `ix_foods_category_active` (`CategoryId`,`IsActive`,`DisplayOrder`),
  CONSTRAINT `fk_foods_category` FOREIGN KEY (`CategoryId`) REFERENCES `restaurant_categories` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_foods`
--

LOCK TABLES `restaurant_foods` WRITE;
/*!40000 ALTER TABLE `restaurant_foods` DISABLE KEYS */;
INSERT INTO `restaurant_foods` VALUES (1,3,'TS01','Trà sữa vị chanh','Trà sữa vị chanh',NULL,0,1,1,'2026-09-08 07:24:03.849834',1,'2026-09-08 07:24:03.849834'),(2,3,'TS02','Trà sữa vị đào','Trà sữa vị đào',NULL,0,1,1,'2026-09-08 07:24:21.131591',1,'2026-09-08 07:24:21.131591');
/*!40000 ALTER TABLE `restaurant_foods` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_kitchen_order_items`
--

DROP TABLE IF EXISTS `restaurant_kitchen_order_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_kitchen_order_items` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `KitchenOrderId` bigint unsigned NOT NULL,
  `OrderItemId` bigint unsigned NOT NULL,
  `Quantity` decimal(18,3) NOT NULL,
  `Status` varchar(30) NOT NULL DEFAULT 'Pending',
  `StartedDate` datetime(6) DEFAULT NULL,
  `ReadyDate` datetime(6) DEFAULT NULL,
  `CompletedDate` datetime(6) DEFAULT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_kitchen_order_items_pair` (`KitchenOrderId`,`OrderItemId`),
  KEY `ix_kitchen_order_items_order_item` (`OrderItemId`),
  KEY `ix_kitchen_order_items_status` (`Status`),
  CONSTRAINT `fk_kitchen_order_items_kitchen_order` FOREIGN KEY (`KitchenOrderId`) REFERENCES `restaurant_kitchen_orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_kitchen_order_items_order_item` FOREIGN KEY (`OrderItemId`) REFERENCES `restaurant_order_items` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `chk_kitchen_order_items_quantity` CHECK ((`Quantity` > 0)),
  CONSTRAINT `chk_kitchen_order_items_status` CHECK ((`Status` in (_utf8mb4'Pending',_utf8mb4'Accepted',_utf8mb4'Preparing',_utf8mb4'Ready',_utf8mb4'Completed',_utf8mb4'Rejected',_utf8mb4'Cancelled')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_kitchen_order_items`
--

LOCK TABLES `restaurant_kitchen_order_items` WRITE;
/*!40000 ALTER TABLE `restaurant_kitchen_order_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_kitchen_order_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_kitchen_orders`
--

DROP TABLE IF EXISTS `restaurant_kitchen_orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_kitchen_orders` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `OrderId` bigint unsigned NOT NULL,
  `KitchenNo` varchar(50) NOT NULL,
  `Status` varchar(30) NOT NULL DEFAULT 'Pending',
  `SentToKitchenDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `AcceptedBy` bigint unsigned DEFAULT NULL,
  `AcceptedDate` datetime(6) DEFAULT NULL,
  `StartedDate` datetime(6) DEFAULT NULL,
  `ReadyDate` datetime(6) DEFAULT NULL,
  `CompletedDate` datetime(6) DEFAULT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_kitchen_orders_kitchen_no` (`KitchenNo`),
  KEY `ix_kitchen_orders_order` (`OrderId`),
  KEY `ix_kitchen_orders_status_sent` (`Status`,`SentToKitchenDate`),
  CONSTRAINT `fk_kitchen_orders_order` FOREIGN KEY (`OrderId`) REFERENCES `restaurant_orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `chk_kitchen_orders_status` CHECK ((`Status` in (_utf8mb4'Pending',_utf8mb4'Accepted',_utf8mb4'Preparing',_utf8mb4'Ready',_utf8mb4'Completed',_utf8mb4'Rejected',_utf8mb4'Cancelled')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_kitchen_orders`
--

LOCK TABLES `restaurant_kitchen_orders` WRITE;
/*!40000 ALTER TABLE `restaurant_kitchen_orders` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_kitchen_orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_order_items`
--

DROP TABLE IF EXISTS `restaurant_order_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_order_items` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `OrderId` bigint unsigned NOT NULL,
  `FoodId` bigint unsigned NOT NULL,
  `FoodVariantId` bigint unsigned NOT NULL,
  `FoodCode` varchar(50) NOT NULL,
  `FoodName` varchar(200) NOT NULL,
  `VariantName` varchar(100) NOT NULL,
  `Quantity` decimal(18,3) NOT NULL,
  `UnitPrice` decimal(18,2) NOT NULL,
  `DiscountAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `TotalAmount` decimal(18,2) NOT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `Status` varchar(30) NOT NULL DEFAULT 'Pending',
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `ix_order_items_food` (`FoodId`),
  KEY `ix_order_items_order_status` (`OrderId`,`Status`),
  KEY `ix_order_items_variant` (`FoodVariantId`),
  CONSTRAINT `fk_order_items_food` FOREIGN KEY (`FoodId`) REFERENCES `restaurant_foods` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_order_items_order` FOREIGN KEY (`OrderId`) REFERENCES `restaurant_orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_order_items_variant` FOREIGN KEY (`FoodVariantId`) REFERENCES `restaurant_food_variants` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_order_items_amounts` CHECK (((`UnitPrice` >= 0) and (`DiscountAmount` >= 0) and (`TotalAmount` >= 0))),
  CONSTRAINT `chk_order_items_quantity` CHECK ((`Quantity` > 0)),
  CONSTRAINT `chk_order_items_status` CHECK ((`Status` in (_utf8mb4'Pending',_utf8mb4'Accepted',_utf8mb4'Preparing',_utf8mb4'Ready',_utf8mb4'Served',_utf8mb4'Completed',_utf8mb4'Cancelled',_utf8mb4'Rejected')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_order_items`
--

LOCK TABLES `restaurant_order_items` WRITE;
/*!40000 ALTER TABLE `restaurant_order_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_order_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_order_promotions`
--

DROP TABLE IF EXISTS `restaurant_order_promotions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_order_promotions` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `OrderId` bigint unsigned NOT NULL,
  `PromotionId` bigint unsigned NOT NULL,
  `PromotionCode` varchar(50) NOT NULL,
  `PromotionName` varchar(200) NOT NULL,
  `DiscountAmount` decimal(18,2) NOT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_order_promotions_order_promotion` (`OrderId`,`PromotionId`),
  KEY `ix_order_promotions_promotion` (`PromotionId`),
  CONSTRAINT `fk_order_promotions_order` FOREIGN KEY (`OrderId`) REFERENCES `restaurant_orders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_order_promotions_promotion` FOREIGN KEY (`PromotionId`) REFERENCES `restaurant_promotion_codes` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_order_promotions_discount` CHECK ((`DiscountAmount` >= 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_order_promotions`
--

LOCK TABLES `restaurant_order_promotions` WRITE;
/*!40000 ALTER TABLE `restaurant_order_promotions` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_order_promotions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_order_status_histories`
--

DROP TABLE IF EXISTS `restaurant_order_status_histories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_order_status_histories` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `OrderId` bigint unsigned NOT NULL,
  `FromStatus` varchar(30) DEFAULT NULL,
  `ToStatus` varchar(30) NOT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `ChangedBy` bigint unsigned DEFAULT NULL,
  `ChangedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `ix_order_status_histories_order_date` (`OrderId`,`ChangedDate`),
  CONSTRAINT `fk_order_status_histories_order` FOREIGN KEY (`OrderId`) REFERENCES `restaurant_orders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_order_status_histories`
--

LOCK TABLES `restaurant_order_status_histories` WRITE;
/*!40000 ALTER TABLE `restaurant_order_status_histories` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_order_status_histories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_orders`
--

DROP TABLE IF EXISTS `restaurant_orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_orders` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `OrderNo` varchar(50) NOT NULL,
  `TableSessionId` bigint unsigned DEFAULT NULL,
  `CustomerId` bigint unsigned DEFAULT NULL,
  `OrderType` varchar(30) NOT NULL DEFAULT 'DineIn',
  `Status` varchar(30) NOT NULL DEFAULT 'Pending',
  `SubtotalAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `DiscountAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `TaxAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `TotalAmount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `Note` varchar(1000) DEFAULT NULL,
  `OrderedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `CompletedDate` datetime(6) DEFAULT NULL,
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `ClientRequestId` char(36) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_orders_order_no` (`OrderNo`),
  UNIQUE KEY `uk_orders_client_request_id` (`ClientRequestId`),
  KEY `ix_orders_customer` (`CustomerId`),
  KEY `ix_orders_ordered_date_status` (`OrderedDate`,`Status`),
  KEY `ix_orders_table_session` (`TableSessionId`),
  CONSTRAINT `fk_orders_table_session` FOREIGN KEY (`TableSessionId`) REFERENCES `restaurant_table_sessions` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_orders_amounts` CHECK (((`SubtotalAmount` >= 0) and (`DiscountAmount` >= 0) and (`TaxAmount` >= 0) and (`TotalAmount` >= 0))),
  CONSTRAINT `chk_orders_status` CHECK ((`Status` in (_utf8mb4'Draft',_utf8mb4'Pending',_utf8mb4'Confirmed',_utf8mb4'Serving',_utf8mb4'Completed',_utf8mb4'Cancelled'))),
  CONSTRAINT `chk_orders_type` CHECK ((`OrderType` in (_utf8mb4'DineIn',_utf8mb4'TakeAway',_utf8mb4'Delivery')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_orders`
--

LOCK TABLES `restaurant_orders` WRITE;
/*!40000 ALTER TABLE `restaurant_orders` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_payment_allocations`
--

DROP TABLE IF EXISTS `restaurant_payment_allocations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_payment_allocations` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `PaymentId` bigint unsigned NOT NULL,
  `OrderId` bigint unsigned NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_payment_allocations_payment_order` (`PaymentId`,`OrderId`),
  KEY `ix_payment_allocations_order` (`OrderId`),
  CONSTRAINT `fk_payment_allocations_order` FOREIGN KEY (`OrderId`) REFERENCES `restaurant_orders` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `fk_payment_allocations_payment` FOREIGN KEY (`PaymentId`) REFERENCES `restaurant_payments` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `chk_payment_allocations_amount` CHECK ((`Amount` > 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_payment_allocations`
--

LOCK TABLES `restaurant_payment_allocations` WRITE;
/*!40000 ALTER TABLE `restaurant_payment_allocations` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_payment_allocations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_payments`
--

DROP TABLE IF EXISTS `restaurant_payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_payments` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TableSessionId` bigint unsigned NOT NULL,
  `PaymentMethod` varchar(30) NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `Status` varchar(30) NOT NULL DEFAULT 'Pending',
  `TransactionNo` varchar(100) DEFAULT NULL,
  `PaidDate` datetime(6) DEFAULT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `ix_payments_paid_date_status` (`PaidDate`,`Status`),
  KEY `ix_payments_session_status` (`TableSessionId`,`Status`),
  KEY `ix_payments_transaction_no` (`TransactionNo`),
  CONSTRAINT `fk_payments_table_session` FOREIGN KEY (`TableSessionId`) REFERENCES `restaurant_table_sessions` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_payments_amount` CHECK ((`Amount` > 0)),
  CONSTRAINT `chk_payments_method` CHECK ((`PaymentMethod` in (_utf8mb4'Cash',_utf8mb4'Card',_utf8mb4'BankTransfer',_utf8mb4'Momo',_utf8mb4'VNPay',_utf8mb4'Other'))),
  CONSTRAINT `chk_payments_status` CHECK ((`Status` in (_utf8mb4'Pending',_utf8mb4'Paid',_utf8mb4'Failed',_utf8mb4'Refunded',_utf8mb4'Cancelled')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_payments`
--

LOCK TABLES `restaurant_payments` WRITE;
/*!40000 ALTER TABLE `restaurant_payments` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_payments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_promotion_categories`
--

DROP TABLE IF EXISTS `restaurant_promotion_categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_promotion_categories` (
  `PromotionId` bigint unsigned NOT NULL,
  `CategoryId` bigint unsigned NOT NULL,
  PRIMARY KEY (`PromotionId`,`CategoryId`),
  KEY `ix_promotion_categories_category` (`CategoryId`),
  CONSTRAINT `fk_promotion_categories_category` FOREIGN KEY (`CategoryId`) REFERENCES `restaurant_categories` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_promotion_categories_promotion` FOREIGN KEY (`PromotionId`) REFERENCES `restaurant_promotion_codes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_promotion_categories`
--

LOCK TABLES `restaurant_promotion_categories` WRITE;
/*!40000 ALTER TABLE `restaurant_promotion_categories` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_promotion_categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_promotion_codes`
--

DROP TABLE IF EXISTS `restaurant_promotion_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_promotion_codes` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `DiscountType` varchar(30) NOT NULL,
  `DiscountValue` decimal(18,2) NOT NULL,
  `MinOrderAmount` decimal(18,2) DEFAULT NULL,
  `MaxDiscountAmount` decimal(18,2) DEFAULT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) NOT NULL,
  `UsageLimit` int DEFAULT NULL,
  `UsageCount` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_promotion_codes_code` (`Code`),
  KEY `ix_promotion_codes_validity` (`IsActive`,`StartDate`,`EndDate`),
  CONSTRAINT `chk_promotion_codes_dates` CHECK ((`EndDate` > `StartDate`)),
  CONSTRAINT `chk_promotion_codes_type` CHECK ((`DiscountType` in (_utf8mb4'Percentage',_utf8mb4'FixedAmount'))),
  CONSTRAINT `chk_promotion_codes_usage` CHECK (((`UsageLimit` is null) or (`UsageLimit` >= 0))),
  CONSTRAINT `chk_promotion_codes_usage_count` CHECK ((`UsageCount` >= 0)),
  CONSTRAINT `chk_promotion_codes_value` CHECK ((`DiscountValue` > 0))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_promotion_codes`
--

LOCK TABLES `restaurant_promotion_codes` WRITE;
/*!40000 ALTER TABLE `restaurant_promotion_codes` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_promotion_codes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_promotion_foods`
--

DROP TABLE IF EXISTS `restaurant_promotion_foods`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_promotion_foods` (
  `PromotionId` bigint unsigned NOT NULL,
  `FoodId` bigint unsigned NOT NULL,
  PRIMARY KEY (`PromotionId`,`FoodId`),
  KEY `ix_promotion_foods_food` (`FoodId`),
  CONSTRAINT `fk_promotion_foods_food` FOREIGN KEY (`FoodId`) REFERENCES `restaurant_foods` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `fk_promotion_foods_promotion` FOREIGN KEY (`PromotionId`) REFERENCES `restaurant_promotion_codes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_promotion_foods`
--

LOCK TABLES `restaurant_promotion_foods` WRITE;
/*!40000 ALTER TABLE `restaurant_promotion_foods` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_promotion_foods` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_rate_limit_policies`
--

DROP TABLE IF EXISTS `restaurant_rate_limit_policies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_rate_limit_policies` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `RoutePattern` varchar(255) NOT NULL,
  `HttpMethods` varchar(100) DEFAULT NULL,
  `PartitionBy` varchar(100) NOT NULL,
  `Algorithm` varchar(30) NOT NULL,
  `PermitLimit` int unsigned NOT NULL,
  `WindowSeconds` int unsigned NOT NULL,
  `BurstLimit` int unsigned DEFAULT NULL,
  `Priority` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `Version` bigint unsigned NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_rate_limit_policies_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_rate_limit_policies`
--

LOCK TABLES `restaurant_rate_limit_policies` WRITE;
/*!40000 ALTER TABLE `restaurant_rate_limit_policies` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_rate_limit_policies` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_table_sessions`
--

DROP TABLE IF EXISTS `restaurant_table_sessions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_table_sessions` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `TableId` bigint unsigned NOT NULL,
  `GuestCount` int NOT NULL DEFAULT '1',
  `Status` varchar(30) NOT NULL DEFAULT 'Open',
  `OpenedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `ClosedDate` datetime(6) DEFAULT NULL,
  `OpenedBy` bigint unsigned DEFAULT NULL,
  `ClosedBy` bigint unsigned DEFAULT NULL,
  `Note` varchar(500) DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  KEY `ix_table_sessions_opened_date` (`OpenedDate`),
  KEY `ix_table_sessions_table_status` (`TableId`,`Status`),
  CONSTRAINT `fk_table_sessions_table` FOREIGN KEY (`TableId`) REFERENCES `restaurant_tables` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_table_sessions_guest_count` CHECK ((`GuestCount` > 0)),
  CONSTRAINT `chk_table_sessions_status` CHECK ((`Status` in (_utf8mb4'Open',_utf8mb4'Closed',_utf8mb4'Cancelled')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_table_sessions`
--

LOCK TABLES `restaurant_table_sessions` WRITE;
/*!40000 ALTER TABLE `restaurant_table_sessions` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_table_sessions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `restaurant_tables`
--

DROP TABLE IF EXISTS `restaurant_tables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `restaurant_tables` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `AreaId` bigint unsigned NOT NULL,
  `Code` varchar(50) NOT NULL,
  `Name` varchar(150) NOT NULL,
  `Capacity` int NOT NULL,
  `Status` varchar(30) NOT NULL DEFAULT 'Available',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedBy` bigint unsigned DEFAULT NULL,
  `CreatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedBy` bigint unsigned DEFAULT NULL,
  `UpdatedDate` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uk_restaurant_tables_code` (`Code`),
  KEY `ix_restaurant_tables_area_status` (`AreaId`,`Status`,`IsActive`),
  CONSTRAINT `fk_restaurant_tables_area` FOREIGN KEY (`AreaId`) REFERENCES `restaurant_areas` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `chk_restaurant_tables_capacity` CHECK ((`Capacity` > 0)),
  CONSTRAINT `chk_restaurant_tables_status` CHECK ((`Status` in (_utf8mb4'Available',_utf8mb4'Occupied',_utf8mb4'Reserved',_utf8mb4'Cleaning',_utf8mb4'Disabled')))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `restaurant_tables`
--

LOCK TABLES `restaurant_tables` WRITE;
/*!40000 ALTER TABLE `restaurant_tables` DISABLE KEYS */;
/*!40000 ALTER TABLE `restaurant_tables` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-09-09 13:33:17
