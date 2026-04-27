CREATE DATABASE  IF NOT EXISTS `poi_foodtour` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `poi_foodtour`;
-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: localhost    Database: poi_foodtour
-- ------------------------------------------------------
-- Server version	9.6.0


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
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ 'a48b77f0-1b3e-11f1-8b65-0045e27d6f98:1-19795';

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260124155412_InitialCreate','8.0.2'),('20260124162754_AddMenu','8.0.2'),('20260126131844_AddMenuAndMenuItem','8.0.2'),('20260202160704_FixPoiRelations','8.0.2'),('20260204144449_AddShopEntity','8.0.2'),('20260323161943_AddCreatedAtToUsers','8.0.2'),('20260330123249_AddFullNameToUser','8.0.2'),('20260331080457_AddPoiLocation','8.0.2'),('20260331103705_AddServicePackagesAndSubscriptions','8.0.2'),('20260331183650_AddAudioUrlToTranslation','8.0.2'),('20260405165546_AllowMultipleShopsPerOwner','8.0.2'),('20260405174141_AddMenuImagesToPoi','8.0.2'),('20260405181200_AddAudioUrlToPoi','8.0.2'),('20260406130843_AddShopStatsAndReviews','8.0.2'),('20260407065123_AddShopCategoryId','8.0.2'),('20260413054749_UpgradeV2_TourSubscriptions_AiFeatures','8.0.2');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Slug` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Categories_Slug` (`Slug`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'Lẩu/Nướng','lau-nuong',1,'2026-04-07 06:58:38.152621'),(2,'Cơm','com',1,'2026-04-07 06:59:14.684006'),(3,'Ăn vặt','an-vat',1,'2026-04-15 02:30:23.893667'),(4,'Đồ uống','o-uong',1,'2026-04-22 03:06:02.702208');
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `languages`
--

DROP TABLE IF EXISTS `languages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `languages` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Languages_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `languages`
--

LOCK TABLES `languages` WRITE;
/*!40000 ALTER TABLE `languages` DISABLE KEYS */;
/*!40000 ALTER TABLE `languages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menuitems`
--

DROP TABLE IF EXISTS `menuitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menuitems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `MenuId` int NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Price` decimal(65,30) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `ImageUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `IsAvailable` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_MenuItems_MenuId` (`MenuId`),
  CONSTRAINT `FK_MenuItems_Menus_MenuId` FOREIGN KEY (`MenuId`) REFERENCES `menus` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menuitems`
--

LOCK TABLES `menuitems` WRITE;
/*!40000 ALTER TABLE `menuitems` DISABLE KEYS */;
/*!40000 ALTER TABLE `menuitems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `menus`
--

DROP TABLE IF EXISTS `menus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `menus` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `POIId` int DEFAULT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `DisplayOrder` int NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '0',
  `ShopId` int NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_Menus_POIId` (`POIId`),
  KEY `IX_Menus_ShopId` (`ShopId`),
  CONSTRAINT `FK_Menus_POIs_POIId` FOREIGN KEY (`POIId`) REFERENCES `pois` (`Id`),
  CONSTRAINT `FK_Menus_Shops_ShopId` FOREIGN KEY (`ShopId`) REFERENCES `shops` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `menus`
--

LOCK TABLES `menus` WRITE;
/*!40000 ALTER TABLE `menus` DISABLE KEYS */;
/*!40000 ALTER TABLE `menus` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShopId` int NOT NULL,
  `TotalAmount` decimal(18,2) NOT NULL,
  `Status` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Orders_Shops_ShopId` (`ShopId`),
  CONSTRAINT `FK_Orders_Shops_ShopId` FOREIGN KEY (`ShopId`) REFERENCES `shops` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders` VALUES (2,9,20000.00,'Completed','2026-04-06 15:40:42.790213'),(3,6,20000.00,'Completed','2026-04-06 15:40:57.597201'),(5,9,20000.00,'Completed','2026-04-06 16:02:09.940362'),(8,6,20000.00,'Completed','2026-04-06 16:14:08.330605'),(9,10,20000.00,'Completed','2026-04-06 17:31:21.591368'),(10,6,20000.00,'Completed','2026-04-06 17:31:32.839762'),(11,10,20000.00,'Completed','2026-04-06 17:31:41.741345'),(12,10,20000.00,'Completed','2026-04-07 07:09:52.878618'),(13,6,20000.00,'Completed','2026-04-07 07:10:48.196297'),(14,9,20000.00,'Completed','2026-04-07 07:12:44.944043'),(15,9,20000.00,'Completed','2026-04-07 07:12:47.248269'),(16,6,20000.00,'Completed','2026-04-07 07:13:11.985395'),(17,10,20000.00,'Completed','2026-04-07 08:49:20.600853'),(18,10,20000.00,'Completed','2026-04-07 08:49:28.700621'),(19,10,20000.00,'Completed','2026-04-07 08:49:48.551660'),(20,9,20000.00,'Completed','2026-04-08 01:40:34.351854'),(21,10,20000.00,'Completed','2026-04-08 01:48:48.303430'),(22,10,20000.00,'Completed','2026-04-08 01:48:50.546427'),(23,9,20000.00,'Completed','2026-04-08 06:32:17.886882'),(24,9,20000.00,'Completed','2026-04-08 06:33:48.875818'),(25,9,20000.00,'Completed','2026-04-08 06:47:30.515888'),(26,9,20000.00,'Completed','2026-04-08 06:47:40.933422'),(27,9,20000.00,'Completed','2026-04-08 06:47:45.065889'),(28,9,20000.00,'Completed','2026-04-08 06:51:49.417074'),(29,10,20000.00,'Completed','2026-04-08 06:53:04.912611'),(30,6,20000.00,'Completed','2026-04-09 02:21:44.818910'),(31,6,20000.00,'Completed','2026-04-09 02:31:07.837504'),(32,6,20000.00,'Completed','2026-04-09 02:31:23.303292');
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pois`
--

DROP TABLE IF EXISTS `pois`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pois` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Location` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ImageUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Latitude` double DEFAULT NULL,
  `Longitude` double DEFAULT NULL,
  `MenuImagesUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `AudioUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pois`
--

LOCK TABLES `pois` WRITE;
/*!40000 ALTER TABLE `pois` DISABLE KEYS */;
INSERT INTO `pois` VALUES (1,'25 Nguyễn Bỉnh Khiêm Quận 1 TPHCM',NULL,NULL,NULL,NULL,NULL),(3,'12312331232','',10.762622,106.660172,NULL,NULL),(4,'hẻm 185 Lý Thường Kiệt','',10.76205788895036,106.65913581848146,NULL,NULL),(5,'25/25 Nguyễn Bỉnh Khiêm','',10.787906443351822,106.704078912735,NULL,NULL),(6,'25/25 Nguyễn Bỉnh Khiêm','',10.787545241099394,106.7039695603973,NULL,NULL),(7,'25/25 Nguyễn Bỉnh Khiêm','/uploads/images/edd44962-7f0c-4d22-90ae-d7a5c66d2488.jpg',10.787567840986723,106.70397429540412,'',NULL),(10,'Đại học Sài Gòn cơ sở 1','/uploads/images/20a2b31e-db62-406b-9d5a-b88649fc8752.jpg',10.779575072202105,106.6847723722458,NULL,NULL),(11,'Hẻm 310 Ngô Quyền','/uploads/images/173ec6b8-b56c-430a-beb2-97f3ade16fea.jpg',10.765641511122345,106.66411936283113,'/uploads/images/4c9f50fc-dc1f-4099-a9c2-170c66f0d296.jpg,/uploads/images/6f19b08f-1043-4b1e-b8a2-04df92621a59.jpg,/uploads/images/3435e7b3-757e-4789-9012-553c5a41d4d0.jpg,/uploads/images/382e2aeb-ec83-4a0e-98e2-f0d90d8b4ca9.png',NULL),(13,'','/uploads/images/d4d5dca4-5ccc-4841-9326-70520c0a420c.jpg',10.760764613001037,106.70355785469809,'/uploads/images/372c9812-4788-4af3-944c-2d5620dc2240.png',NULL);
/*!40000 ALTER TABLE `pois` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `poitranslations`
--

DROP TABLE IF EXISTS `poitranslations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `poitranslations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `POIId` int NOT NULL,
  `LanguageCode` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AudioUrl` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_POITranslations_POIId_LanguageCode` (`POIId`,`LanguageCode`),
  CONSTRAINT `FK_POITranslations_POIs_POIId` FOREIGN KEY (`POIId`) REFERENCES `pois` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `poitranslations`
--

LOCK TABLES `poitranslations` WRITE;
/*!40000 ALTER TABLE `poitranslations` DISABLE KEYS */;
INSERT INTO `poitranslations` VALUES (1,1,'vi','Ốc','Ốc ngon lắm','/audio/poi_1_vi.mp3'),(3,3,'vi','Cơm','Cơm gà xối mỡ',NULL),(4,4,'vi','Cơm','Cơm gà xối mỡ',NULL),(5,5,'vi','Cơm gà','Cơm gà xối mỡ',NULL),(6,6,'vi','Cơm gà nè','Cơm gà xỗi mỡ nè ngon lắm\n',NULL),(7,7,'vi','Cơm gà Nè','Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người','/audio/poi_7_vi_db54da68.mp3'),(9,7,'en','Chicken rice','Big brother Big brother Chicken rice with fat is very delicious, everyone stops by to eat and support, iu everyone','/audio/poi_7_en_aa5aae3c.mp3'),(10,7,'zh','鸡肉饭','大哥，大哥，鸡肉饭加油非常美味，大家都来吃喝支持，iu','/audio/poi_7_zh_2a835cc7.mp3'),(11,7,'th','Cơm gà Nè','Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người\n',NULL),(12,7,'de','Cơm gà Nè','Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người\n',NULL),(16,10,'vi','Anh Lớn Quán','Quán của Anh Lớn có cơm ngon lắm','/audio/poi_10_vi_558160b8.mp3'),(17,10,'en','Mr. Lon Quan','Anh Lon\'s restaurant has very good rice','/audio/poi_10_en_4b3fc59d.mp3'),(18,10,'zh','龙全先生','安龙的餐厅米饭非常好吃','/audio/poi_10_zh_b470ff65.mp3'),(19,11,'vi','Trạm Dừng Chân','Quán nướng siêu ngon','/audio/poi_11_vi_da42fd06.mp3'),(20,11,'en','Rest Stations','Grill Grill','/audio/poi_11_en_627301b9.mp3'),(21,11,'zh','休息站','烧烤烧烤','/audio/poi_11_zh_4b17035b.mp3'),(25,13,'vi','Her coffee','Her Coffee: Chút ngọt ngào cho ngày thêm tươi mới!\nBạn đang tìm một không gian "chill" hết nấc với những góc sống ảo triệu view? Her Coffee chính là điểm dừng chân lý tưởng. Với menu đồ uống đa dạng từ Coffee đậm đà đến Trà trái cây thanh mát, Her hứa hẹn sẽ làm bừng tỉnh mọi giác quan của bạn.\n📸 Tag ngay cạ cứng và ghé Her Coffee để "nạp" vitamin tích cực ngay thôi!','/audio/poi_13_vi_2abf657c.mp3'),(26,13,'en','Her coffee','Her Coffee: A little sweetness for a fresher day!\nAre you looking for a "chill" space with millions of views virtual living corners? Her Coffee is the ideal stop. With a diverse drink menu from Strong Coffee to Refreshing Fruit Tea, Her promises to awaken all your senses.\n📸 Tag right away and visit Her Coffee to "load" positive vitamins right away!','/audio/poi_13_en_82d15fd4.mp3'),(27,13,'zh','她的咖啡','她的咖啡：为清新的日子增添一点甜味！\n你是在寻找一个拥有数百万浏览量、虚拟生活角落的“轻松”空间吗？她的咖啡是理想的停靠点。她提供从浓咖啡到清爽水果茶的多样化饮品菜单，承诺唤醒你所有感官。\n📸 立即标记，访问Her Coffee，立即“加载”积极维生素！','/audio/poi_13_zh_d36c2533.mp3');
/*!40000 ALTER TABLE `poitranslations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reviews`
--

DROP TABLE IF EXISTS `reviews`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reviews` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ShopId` int NOT NULL,
  `UserId` int DEFAULT NULL,
  `CustomerName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Rating` int NOT NULL,
  `Comment` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Reviews_ShopId` (`ShopId`),
  KEY `IX_Reviews_UserId` (`UserId`),
  CONSTRAINT `FK_Reviews_Shops_ShopId` FOREIGN KEY (`ShopId`) REFERENCES `shops` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Reviews_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reviews`
--

LOCK TABLES `reviews` WRITE;
/*!40000 ALTER TABLE `reviews` DISABLE KEYS */;
INSERT INTO `reviews` VALUES (1,9,NULL,'Lê Trọng Duy',5,'Nội dung hay lắm cho quán 5 sao','2026-04-06 16:02:45.678406'),(3,6,NULL,'Dương Anh Kiệt',5,'quán ăn tuyệt quá anh lớn cơm gà ngon nhe','2026-04-06 16:14:20.016297'),(4,10,NULL,'Dương Anh Kiệt',5,'Trạm Dừng Chân này oke nhaaaaaa','2026-04-07 07:10:12.314002'),(5,10,NULL,'Lê Trọng Duy',5,'OK 5 stars for this restaurant\r','2026-04-07 08:49:45.517295'),(6,6,NULL,'Anh Lớn',5,'','2026-04-09 02:21:54.909193'),(7,6,NULL,'Dương Anh Kiệt',5,'','2026-04-09 02:31:16.027789'),(8,9,NULL,'Dương Anh Kiệt',5,'OK hay \r','2026-04-09 14:42:33.450985');
/*!40000 ALTER TABLE `reviews` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (-3,'USER'),(-2,'OWNER'),(-1,'ADMIN');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `servicepackages`
--

DROP TABLE IF EXISTS `servicepackages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `servicepackages` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Tier` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MonthlyPrice` decimal(18,2) NOT NULL,
  `YearlyPrice` decimal(18,2) NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Features` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `MaxStores` int NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `Audience` varchar(32) NOT NULL DEFAULT 'OWNER',
  `AllowAudioAccess` tinyint(1) NOT NULL DEFAULT '0',
  `AllowAiPlanAccess` tinyint(1) NOT NULL DEFAULT '0',
  `AllowChatbotAccess` tinyint(1) NOT NULL DEFAULT '0',
  `AllowTinderAccess` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `servicepackages`
--

LOCK TABLES `servicepackages` WRITE;
/*!40000 ALTER TABLE `servicepackages` DISABLE KEYS */;
INSERT INTO `servicepackages` VALUES (1,'Basic','Basic',99000.00,990000.00,'Gói cơ bản cho seller nhỏ.','Hiển thị trên bản đồ|Tối đa 1 gian hàng|1 Menu, không giới hạn món|Hỗ trợ qua email',1,1,'2026-01-01 00:00:00.000000','OWNER',0,0,0,0),(2,'Premium','Premium',299000.00,2990000.00,'Gói mở rộng cho seller đang tăng trưởng.','Tất cả tính năng Basic|Tối đa 3 gian hàng|Badge Premium trên app|Ưu tiên đề xuất score +50|Thống kê nâng cao|Hỗ trợ ưu tiên',3,1,'2026-01-01 00:00:00.000000','OWNER',0,0,0,0),(3,'VIP','VIP',599000.00,5990000.00,'Gói đầy đủ cho seller lớn và chuỗi gian hàng.','Tất cả tính năng Premium|Tối đa 5 gian hàng|Badge VIP trên app|Top đề xuất score +100|Thống kê chi tiết|Quảng cáo trên banner|Hỗ trợ riêng 24/7',5,1,'2026-01-01 00:00:00.000000','OWNER',0,0,0,0),(4,'Audio Starter','AudioBasic',49000.00,490000.00,'Gói nghe thuyết minh cơ bản.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),(5,'Audio Plus','AudioPremium',99000.00,990000.00,'Gói nghe thuyết minh mở rộng.','Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),(6,'Audio Premium','AudioVIP',199000.00,1990000.00,'Gói audio cao cấp.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),(7,'Tour Basic','TourBasic',50000.00,50000.00,'Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|Chatbot Thổ Địa tư vấn món ăn|!Tinder Ẩm Thực|!AI Kế Hoạch Tour',0,1,'2026-01-01 00:00:00.000000','USER',1,0,1,0),(8,'Tour Plus','TourPlus',99000.00,99000.00,'Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot',0,1,'2026-01-01 00:00:00.000000','USER',1,1,1,1),(9,'Tour Basic','TourBasic',50000.00,50000.00,'Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|Chatbot Thổ Địa tư vấn món ăn|!Tinder Ẩm Thực|!AI Kế Hoạch Tour',0,1,'2026-01-01 00:00:00.000000','USER',1,0,1,0),(10,'Tour Plus','TourPlus',99000.00,99000.00,'Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot',0,1,'2026-01-01 00:00:00.000000','USER',1,1,1,1),(11,'Audio Starter','AudioBasic',49000.00,490000.00,'Gói nghe thuyết minh cơ bản.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),(12,'Audio Plus','AudioPremium',99000.00,990000.00,'Gói nghe thuyết minh mở rộng.','Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),(13,'Audio Premium','AudioVIP',199000.00,1990000.00,'Gói audio cao cấp.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0);
/*!40000 ALTER TABLE `servicepackages` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `shops`
--

DROP TABLE IF EXISTS `shops`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `shops` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `OwnerId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '0',
  `PoiId` int DEFAULT NULL,
  `ListenCount` int NOT NULL DEFAULT '0',
  `ViewCount` int NOT NULL DEFAULT '0',
  `CategoryId` int DEFAULT NULL,
  `AverageRating` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_Shops_PoiId` (`PoiId`),
  KEY `IX_Shops_OwnerId` (`OwnerId`),
  KEY `IX_Shops_CategoryId` (`CategoryId`),
  CONSTRAINT `FK_Shops_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Shops_POIs_PoiId` FOREIGN KEY (`PoiId`) REFERENCES `pois` (`Id`),
  CONSTRAINT `FK_Shops_Users_OwnerId` FOREIGN KEY (`OwnerId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `shops`
--

LOCK TABLES `shops` WRITE;
/*!40000 ALTER TABLE `shops` DISABLE KEYS */;
INSERT INTO `shops` VALUES (6,'Cơm gà Nè','Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người','25/25 Nguyễn Bỉnh Khiêm',6,'2026-04-05 17:02:51.663043',1,7,9,7942,2,0),(9,'Anh Lớn Quán','Quán của Anh Lớn có cơm ngon lắm','Đại học Sài Gòn cơ sở 1',6,'2026-04-06 15:18:05.021788',1,10,13,10321,2,0),(10,'Trạm Dừng Chân','Quán nướng siêu ngon','Hẻm 310 Ngô Quyền',6,'2026-04-06 17:30:50.187522',1,11,10,26,1,0),(12,'Her coffee','Her Coffee: Chút ngọt ngào cho ngày thêm tươi mới!\nBạn đang tìm một không gian "chill" hết nấc với những góc sống ảo triệu view? Her Coffee chính là điểm dừng chân lý tưởng. Với menu đồ uống đa dạng từ Coffee đậm đà đến Trà trái cây thanh mát, Her hứa hẹn sẽ làm bừng tỉnh mọi giác quan của bạn.\n📸 Tag ngay cạ cứng và ghé Her Coffee để "nạp" vitamin tích cực ngay thôi!','',16,'2026-04-22 03:06:02.783942',1,13,0,0,4,0);
/*!40000 ALTER TABLE `shops` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `subscriptions`
--

DROP TABLE IF EXISTS `subscriptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `subscriptions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `ServicePackageId` int NOT NULL,
  `BillingCycle` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Price` decimal(18,2) NOT NULL,
  `StartDate` datetime(6) NOT NULL,
  `EndDate` datetime(6) NOT NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `PaymentProvider` varchar(32) NOT NULL DEFAULT 'PayOS',
  `PaymentStatus` varchar(32) NOT NULL DEFAULT 'Pending',
  `PaymentOrderCode` bigint DEFAULT NULL,
  `PaymentLinkId` varchar(255) DEFAULT NULL,
  `CheckoutUrl` longtext,
  `ActivatedAt` datetime(6) DEFAULT NULL,
  `CancelAtPeriodEnd` tinyint(1) NOT NULL DEFAULT '0',
  `CancelRequestedAt` datetime(6) DEFAULT NULL,
  `RevenueRecipientUserId` int DEFAULT NULL,
  `RevenueRecipientShopId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Subscriptions_ServicePackageId` (`ServicePackageId`),
  KEY `IX_Subscriptions_UserId` (`UserId`),
  CONSTRAINT `FK_Subscriptions_ServicePackages_ServicePackageId` FOREIGN KEY (`ServicePackageId`) REFERENCES `servicepackages` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Subscriptions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `subscriptions`
--

LOCK TABLES `subscriptions` WRITE;
/*!40000 ALTER TABLE `subscriptions` DISABLE KEYS */;
INSERT INTO `subscriptions` VALUES (1,6,1,'Monthly',99000.00,'2026-04-08 08:53:38.189520','2026-05-08 08:53:38.189520','Cancelled','2026-04-08 08:53:38.189901','PayOS','Failed',1775638418006,NULL,NULL,NULL,0,NULL,NULL,NULL),(2,6,1,'Monthly',99000.00,'2026-04-08 09:10:24.071512','2026-05-08 09:10:24.071512','Cancelled','2026-04-08 09:10:24.072020','PayOS','Failed',1775639424006,NULL,NULL,NULL,0,NULL,NULL,NULL),(3,6,1,'Monthly',99000.00,'2026-04-08 09:18:09.802938','2026-05-08 09:18:09.802938','Cancelled','2026-04-08 09:18:09.803663','PayOS','Failed',1775639889006,NULL,NULL,NULL,0,NULL,NULL,NULL),(4,6,1,'Monthly',99000.00,'2026-04-08 09:28:22.900384','2026-05-08 09:28:22.900384','Cancelled','2026-04-08 09:28:22.900759','PayOS','Failed',1775640502006,NULL,NULL,NULL,0,NULL,NULL,NULL),(5,6,1,'Yearly',990000.00,'2026-04-08 09:28:23.451728','2027-04-08 09:28:23.451728','Cancelled','2026-04-08 09:28:23.451744','PayOS','Failed',1775640503006,NULL,NULL,NULL,0,NULL,NULL,NULL),(6,6,1,'Monthly',99000.00,'2026-04-08 09:28:28.187318','2026-05-08 09:28:28.187318','Cancelled','2026-04-08 09:28:28.187325','PayOS','Failed',1775640508006,NULL,NULL,NULL,0,NULL,NULL,NULL),(7,6,1,'Monthly',99000.00,'2026-04-08 09:32:57.771752','2026-05-08 09:32:57.771752','Cancelled','2026-04-08 09:32:57.772447','PayOS','Failed',1775640777006,NULL,NULL,NULL,0,NULL,NULL,NULL),(8,6,1,'Monthly',99000.00,'2026-04-08 09:32:58.417161','2026-05-08 09:32:58.417161','Cancelled','2026-04-08 09:32:58.417178','PayOS','Failed',1775640778006,NULL,NULL,NULL,0,NULL,NULL,NULL),(9,6,1,'Monthly',99000.00,'2026-04-08 10:01:01.139459','2026-04-09 14:38:46.444796','Cancelled','2026-04-08 09:36:43.425835','PayOS','Paid',1775641003006,'ca671f4849bc4a2090f20032ce601e31','https://pay.payos.vn/web/ca671f4849bc4a2090f20032ce601e31','2026-04-08 10:01:01.139459',0,NULL,NULL,NULL),(10,16,2,'Monthly',299000.00,'2026-04-09 01:16:34.783542','2026-05-09 01:16:34.783542','Cancelled','2026-04-09 01:16:34.783928','PayOS','Cancelled',1775697394016,'465e406409514c3fb886954f7b204719','https://pay.payos.vn/web/465e406409514c3fb886954f7b204719',NULL,0,NULL,NULL,NULL),(11,5,9,'Monthly',49000.00,'2026-04-09 01:23:45.008462','2026-05-09 01:23:45.008462','Cancelled','2026-04-09 01:23:45.008892','PayOS','Cancelled',1775697825005,'ade1bb9074af47bd918c56a5f1004305','https://pay.payos.vn/web/ade1bb9074af47bd918c56a5f1004305',NULL,0,NULL,NULL,NULL),(12,11,10,'Monthly',99000.00,'2026-04-09 01:33:34.954724','2026-05-09 01:33:34.954724','Cancelled','2026-04-09 01:33:34.954747','PayOS','Cancelled',1775698414011,'22450ecba72d4951a564c02103b71e9a','https://pay.payos.vn/web/22450ecba72d4951a564c02103b71e9a',NULL,0,NULL,NULL,NULL),(13,5,9,'Daily',20000.00,'2026-04-09 02:21:30.240841','2026-04-10 02:21:30.240841','Expired','2026-04-09 02:20:54.277803','PayOS','Paid',1775701254005,'2af87c6b5a144b6ea22bf4bc7b2a5de6','https://pay.payos.vn/web/2af87c6b5a144b6ea22bf4bc7b2a5de6','2026-04-09 02:21:30.240841',0,NULL,NULL,NULL),(14,11,9,'Daily',20000.00,'2026-04-09 02:28:29.557184','2026-04-10 02:28:29.557184','Cancelled','2026-04-09 02:28:29.557627','PayOS','Cancelled',1775701709011,'384168ad143945608a2040a19d44bb39','https://pay.payos.vn/web/384168ad143945608a2040a19d44bb39',NULL,0,NULL,NULL,NULL),(15,11,10,'Monthly',100000.00,'2026-04-09 02:28:39.784245','2026-05-09 02:28:39.784245','Cancelled','2026-04-09 02:28:39.784277','PayOS','Cancelled',1775701719011,'b6d7d27a2aca479d9c7caa6af5ba488b','https://pay.payos.vn/web/b6d7d27a2aca479d9c7caa6af5ba488b',NULL,0,NULL,NULL,NULL),(16,11,9,'Daily',20000.00,'2026-04-09 02:30:07.370528','2026-04-10 02:30:07.370528','Expired','2026-04-09 02:29:25.994441','PayOS','Paid',1775701765011,'3832b582286945fdb26d70c53c0102e6','https://pay.payos.vn/web/3832b582286945fdb26d70c53c0102e6','2026-04-09 02:30:07.370528',0,NULL,NULL,NULL),(17,16,3,'Monthly',599000.00,'2026-04-09 03:12:18.781221','2026-05-09 03:12:18.781221','Cancelled','2026-04-09 03:12:18.781605','PayOS','Cancelled',1775704338016,'f2e86857d61d42e7adf58c9d1033112f','https://pay.payos.vn/web/f2e86857d61d42e7adf58c9d1033112f',NULL,0,NULL,NULL,NULL),(18,16,2,'Monthly',299000.00,'2026-04-09 03:12:27.516530','2026-05-09 03:12:27.516530','Cancelled','2026-04-09 03:12:27.516551','PayOS','Cancelled',1775704347016,'4c81703672ec4f958ae8c9e57ce94889','https://pay.payos.vn/web/4c81703672ec4f958ae8c9e57ce94889',NULL,0,NULL,NULL,NULL),(19,16,2,'Yearly',2990000.00,'2026-04-09 03:12:40.028811','2027-04-09 03:12:40.028811','Cancelled','2026-04-09 03:12:40.028830','PayOS','Cancelled',1775704360016,'53a7f8b75e544537b8fcdd558a80781e','https://pay.payos.vn/web/53a7f8b75e544537b8fcdd558a80781e',NULL,0,NULL,NULL,NULL),(20,16,2,'Monthly',299000.00,'2026-04-09 03:15:57.564813','2026-05-09 03:15:57.564813','Cancelled','2026-04-09 03:15:57.565363','PayOS','Cancelled',1775704557016,'44113f0b00ca446c9f03f56403a0973b','https://pay.payos.vn/web/44113f0b00ca446c9f03f56403a0973b',NULL,0,NULL,NULL,NULL),(21,16,3,'Monthly',599000.00,'2026-04-09 03:16:04.832830','2026-05-09 03:16:04.832830','Cancelled','2026-04-09 03:16:04.832850','PayOS','Cancelled',1775704564016,'9d7a5e4050444cb1ae3ca6fe0b861a0b','https://pay.payos.vn/web/9d7a5e4050444cb1ae3ca6fe0b861a0b',NULL,0,NULL,NULL,NULL),(22,16,2,'Yearly',2990000.00,'2026-04-09 03:16:11.505478','2027-04-09 03:16:11.505478','Cancelled','2026-04-09 03:16:11.505494','PayOS','Cancelled',1775704571016,'1b4f72d32d454b808ca78125ee2f2329','https://pay.payos.vn/web/1b4f72d32d454b808ca78125ee2f2329',NULL,0,NULL,NULL,NULL),(23,16,1,'Monthly',99000.00,'2026-04-09 03:17:15.705584','2026-05-09 03:17:15.705584','Active','2026-04-09 03:16:49.960380','PayOS','Paid',1775704609016,'ed1e798cff134ec2a8fd94cf9ba79eb8','https://pay.payos.vn/web/ed1e798cff134ec2a8fd94cf9ba79eb8','2026-04-09 03:17:15.705584',0,NULL,NULL,NULL),(24,6,2,'Monthly',299000.00,'2026-04-09 14:38:46.444796','2026-05-09 14:38:46.444796','Active','2026-04-09 14:38:16.365856','PayOS','Paid',1775745496006,'37dee3d82ca14918be1686115c57c9e4','https://pay.payos.vn/web/37dee3d82ca14918be1686115c57c9e4','2026-04-09 14:38:46.444796',0,NULL,NULL,NULL),(25,17,10,'Monthly',100000.00,'2026-04-09 14:42:14.509165','2026-05-09 14:42:14.509165','Active','2026-04-09 14:41:59.146265','PayOS','Paid',1775745719017,'ec4c7b5c5be44d86b71a9bd60b62f24d','https://pay.payos.vn/web/ec4c7b5c5be44d86b71a9bd60b62f24d','2026-04-09 14:42:14.509165',0,NULL,NULL,NULL),(26,5,9,'Daily',20000.00,'2026-04-11 07:24:00.793600','2026-04-12 07:24:00.793600','Expired','2026-04-11 07:23:35.539395','PayOS','Paid',1775892215005,'606fb2881bc247a6bb2759b86386bd4d','https://pay.payos.vn/web/606fb2881bc247a6bb2759b86386bd4d','2026-04-11 07:24:00.793600',0,NULL,6,6),(27,18,10,'Monthly',100000.00,'2026-04-11 07:31:41.055825','2026-05-11 07:31:41.055825','Active','2026-04-11 07:30:52.634045','PayOS','Paid',1775892652018,'9bb054b0d2574ea3865815ec031e18e1','https://pay.payos.vn/web/9bb054b0d2574ea3865815ec031e18e1','2026-04-11 07:31:41.055825',0,NULL,NULL,NULL),(28,19,9,'Daily',20000.00,'2026-04-13 02:38:22.999207','2026-04-14 02:38:22.999207','Cancelled','2026-04-13 02:38:23.017194','PayOS','Failed',1776047903019,NULL,NULL,NULL,0,NULL,NULL,NULL),(29,19,9,'Daily',20000.00,'2026-04-13 02:38:27.736498','2026-04-14 02:38:27.736498','Cancelled','2026-04-13 02:38:27.740821','PayOS','Failed',1776047907019,NULL,NULL,NULL,0,NULL,NULL,NULL),(30,19,9,'Daily',20000.00,'2026-04-13 02:41:52.666321','2026-04-14 02:41:52.666321','Cancelled','2026-04-13 02:41:52.668326','PayOS','Cancelled',1776048112019,'c9a5764afee5441e9f1286b39fb3ddb0','https://pay.payos.vn/web/c9a5764afee5441e9f1286b39fb3ddb0',NULL,0,NULL,NULL,NULL),(31,19,10,'Daily',99000.00,'2026-04-13 06:25:44.185313','2026-04-14 06:25:44.185313','Expired','2026-04-13 06:24:38.637913','PayOS','Paid',1776061478019,'ccd6ca6b1fea41a1bea1899115346eff','https://pay.payos.vn/web/ccd6ca6b1fea41a1bea1899115346eff','2026-04-13 06:25:44.185313',0,NULL,NULL,NULL),(32,20,10,'Daily',99000.00,'2026-04-13 07:04:14.209912','2026-04-14 07:04:14.209912','Expired','2026-04-13 07:03:50.207535','PayOS','Paid',1776063830020,'e4277fc6457e4d919fdc4768acd8b04a','https://pay.payos.vn/web/e4277fc6457e4d919fdc4768acd8b04a','2026-04-13 07:04:14.209912',0,NULL,NULL,NULL),(33,5,10,'Daily',99000.00,'2026-04-13 07:39:25.570695','2026-04-14 07:39:25.570695','Expired','2026-04-13 07:39:03.174558','PayOS','Paid',1776065943005,'928a8f370e6f4df5bcb386619c182f0c','https://pay.payos.vn/web/928a8f370e6f4df5bcb386619c182f0c','2026-04-13 07:39:25.570695',0,NULL,6,6),(34,6,3,'Monthly',599000.00,'2026-04-14 17:05:26.776433','2026-05-14 17:05:26.776433','PendingPayment','2026-04-14 17:05:26.776943','PayOS','Pending',1776186326006,'d3a57e03da2a41038c3806ad2525582c','https://pay.payos.vn/web/d3a57e03da2a41038c3806ad2525582c',NULL,0,NULL,NULL,NULL),(35,21,1,'Monthly',99000.00,'2026-04-14 17:06:45.446452','2026-05-14 17:06:45.446452','Cancelled','2026-04-14 17:06:45.446470','PayOS','Cancelled',1776186405021,'8ac75d640d7a4b96806a26a63dcb1dbe','https://pay.payos.vn/web/8ac75d640d7a4b96806a26a63dcb1dbe',NULL,0,NULL,NULL,NULL),(36,21,1,'Monthly',99000.00,'2026-04-14 17:07:02.403124','2026-05-14 17:07:02.403124','PendingPayment','2026-04-14 17:07:02.403139','PayOS','Pending',1776186422021,'48bab2af5f6b46aba661bb6d41227e89','https://pay.payos.vn/web/48bab2af5f6b46aba661bb6d41227e89',NULL,0,NULL,NULL,NULL),(37,19,9,'Daily',50000.00,'2026-04-15 00:30:50.197855','2026-04-16 00:30:50.197855','Cancelled','2026-04-15 00:30:50.221543','PayOS','Cancelled',1776213050019,'26ae69c1a05b4339b4a2da238a11cfb0','https://pay.payos.vn/web/26ae69c1a05b4339b4a2da238a11cfb0',NULL,0,NULL,6,10),(38,19,10,'Daily',99000.00,'2026-04-15 00:32:47.636774','2026-04-16 00:32:47.636774','Active','2026-04-15 00:30:58.671806','PayOS','Paid',1776213058019,'214853fe00634a54a82797f0b09fdaad','https://pay.payos.vn/web/214853fe00634a54a82797f0b09fdaad','2026-04-15 00:32:47.636774',0,NULL,6,10),(39,22,10,'Daily',99000.00,'2026-04-15 01:20:29.043272','2026-04-16 01:20:29.043272','Active','2026-04-15 01:20:00.972211','PayOS','Paid',1776216000022,'8ed780d494ec4440b642c90f41981bc1','https://pay.payos.vn/web/8ed780d494ec4440b642c90f41981bc1','2026-04-15 01:20:29.043272',0,NULL,NULL,NULL);
/*!40000 ALTER TABLE `subscriptions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `swipeditems`
--

DROP TABLE IF EXISTS `swipeditems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `swipeditems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `ShopId` int NOT NULL,
  `IsLiked` tinyint(1) NOT NULL,
  `SwipedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_SwipedItems_UserId_ShopId` (`UserId`,`ShopId`),
  KEY `IX_SwipedItems_ShopId` (`ShopId`),
  CONSTRAINT `FK_SwipedItems_Shops_ShopId` FOREIGN KEY (`ShopId`) REFERENCES `shops` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_SwipedItems_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `swipeditems`
--

LOCK TABLES `swipeditems` WRITE;
/*!40000 ALTER TABLE `swipeditems` DISABLE KEYS */;
INSERT INTO `swipeditems` VALUES (1,5,6,1,'2026-04-13 09:16:53.788734'),(2,5,9,0,'2026-04-13 09:16:55.967610'),(3,5,10,1,'2026-04-13 09:16:56.814822'),(5,19,6,1,'2026-04-13 13:31:52.193144'),(6,19,9,1,'2026-04-13 13:31:53.142656'),(7,19,10,1,'2026-04-13 13:31:53.893473');
/*!40000 ALTER TABLE `swipeditems` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usagehistories`
--

DROP TABLE IF EXISTS `usagehistories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usagehistories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `DeviceId` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ShopId` int NOT NULL,
  `LanguageCode` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ListenedAt` datetime(6) NOT NULL,
  `DurationSeconds` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UsageHistories_ShopId` (`ShopId`),
  CONSTRAINT `FK_UsageHistories_Shops_ShopId` FOREIGN KEY (`ShopId`) REFERENCES `shops` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usagehistories`
--

LOCK TABLES `usagehistories` WRITE;
/*!40000 ALTER TABLE `usagehistories` DISABLE KEYS */;
INSERT INTO `usagehistories` VALUES (1,'user:11',9,'vi','2026-04-06 15:40:42.830202',0),(2,'user:11',6,'vi','2026-04-06 15:40:57.598003',0),(4,'user:5',9,'vi','2026-04-06 16:02:09.959686',0),(7,'user:11',6,'vi','2026-04-06 16:14:08.347897',0),(8,'user:11',10,'vi','2026-04-06 17:31:21.597360',0),(9,'user:11',6,'vi','2026-04-06 17:31:32.839861',0),(10,'user:11',10,'vi','2026-04-06 17:31:41.741527',0),(11,'user:11',10,'vi','2026-04-07 07:09:52.889772',0),(12,'user:11',6,'vi','2026-04-07 07:10:48.196421',0),(13,'user:5',9,'vi','2026-04-07 07:12:44.985642',0),(14,'user:5',9,'vi','2026-04-07 07:12:47.249543',0),(15,'user:5',6,'vi','2026-04-07 07:13:11.985593',0),(16,'user:5',10,'vi','2026-04-07 08:49:20.614649',0),(17,'user:5',10,'vi','2026-04-07 08:49:28.701567',0),(18,'user:5',10,'vi','2026-04-07 08:49:48.551770',0),(19,'user:5',9,'vi','2026-04-08 01:40:34.397183',0),(20,'user:5',10,'vi','2026-04-08 01:48:48.333084',0),(21,'user:5',10,'vi','2026-04-08 01:48:50.547764',0),(22,'user:5',9,'vi','2026-04-08 06:32:17.905552',0),(23,'user:5',9,'vi','2026-04-08 06:33:48.876752',0),(24,'user:5',9,'vi','2026-04-08 06:47:30.535662',0),(25,'user:5',9,'vi','2026-04-08 06:47:40.934232',0),(26,'user:5',9,'vi','2026-04-08 06:47:45.066097',0),(27,'user:5',9,'vi','2026-04-08 06:51:49.443695',0),(28,'user:5',10,'vi','2026-04-08 06:53:04.913659',0),(29,'user:5',6,'vi','2026-04-09 02:21:44.827511',0),(30,'user:11',6,'vi','2026-04-09 02:31:07.844827',0),(31,'user:11',6,'vi','2026-04-09 02:31:23.303574',0),(32,'user:17',9,'vi','2026-04-09 14:42:24.551109',0),(33,'user:18',9,'vi','2026-04-11 07:31:52.014096',0),(34,'user:18',6,'vi','2026-04-11 07:32:39.766640',0),(35,'user:19',10,'vi','2026-04-13 18:19:33.669588',0);
/*!40000 ALTER TABLE `usagehistories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RoleId` int NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
  `IsActive` tinyint(1) NOT NULL DEFAULT '0',
  `FullName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Users_RoleId` (`RoleId`),
  CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'$2a$11$ovEQzNheew6f4TfLcUioL.lki21AIDIMPSE/s4VwYfFynyRsZYV42','admin@foodstreet.vn',-1,'2026-03-23 16:24:42.608944',1,''),(2,'$2a$11$JPIRHpKpcPGI2exCQQm4ZexLARl2kD1TUZzagyQuxAyKMoPITvXVC','test@gmail.com',-3,'2026-03-24 17:17:15.606367',1,''),(5,'$2a$11$BolKnkAQ3Gm4nQtRnj0heOO7PiDg76bgcpjGVvM0fEByQCh4guhBG','anhlontest@gmail.com',-3,'2026-03-30 12:39:23.326815',1,'Anh Lớn'),(6,'$2a$11$AyWzTKyqGBq1pedc43mlFekGNPvxICkYiQ3WVfBebeUHPA92/IslS','banhang@gmail.com',-2,'2026-04-05 09:51:02.905942',1,'Nguyễn Văn A'),(11,'$2a$11$jZXgw.jKRfcAX8NIRuGJZu0cT43MnPK4GgdKAU9pL8lAZ8u9Jfcu2','anhkiet@gmail.com',-3,'2026-04-06 15:19:08.710271',1,'Dương Anh Kiệt'),(16,'$2a$11$U84C3.2NyoK7DLiPuADZku6bxwHNRFZB6vTpT4lFp7kqsTT.jwuCG','hiuthu@gmail.com',-2,'2026-04-09 01:16:20.135780',1,''),(17,'$2a$11$fkxJ9YnYpZbAVznNrR9Nu.3MGyfiAGnerrX0IJMcv91fShe6nxoCe','kiet@gmail.com',-3,'2026-04-09 14:41:31.927941',1,'Dương Anh Kiệt'),(18,'$2a$11$MCb9Bpwsng0v7GZrZpZSvePWZJuUQfGNJL1xkM6XNwcihH.t71lNu','go@gmail.com',-3,'2026-04-11 07:30:40.998684',1,'Thằng Gơ'),(19,'$2a$11$dbWC0PLG/XAqs3udgt6TG.CeYzi84e/UEms/IYyu1h3zp9C.RKEqO','user@gmail.com',-3,'2026-04-13 02:36:53.473264',1,'Anh Lớn'),(20,'$2a$11$2hgwohBvoUEz3nVXVBG8bOMA8bUp9CvelN0ShKI3o9YdMJn440pYu','hiuthu123@gmail.com',-3,'2026-04-13 06:56:18.677258',1,'Trung Hiếu Thú'),(21,'$2a$11$aTxw1CRDYjTO6LpgXt0ejuzkGr76p.UPePGLUf1.twRMMsl29csHC','ak@gmail.com',-2,'2026-04-14 17:06:30.316560',1,'Dương Anh Kiệt'),(22,'$2a$11$Dx2VikgJ680IInvMN6igMueFCrHY8HkFK2WGKobj4gFm7EKV6HwM.','anh@gmail.com',-3,'2026-04-15 01:19:39.974535',1,'Anh Lớn');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-04-27  23:05:38
