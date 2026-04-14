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

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ 'a48b77f0-1b3e-11f1-8b65-0045e27d6f98:1-19667';

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
INSERT INTO `__efmigrationshistory` VALUES ('20260124155412_InitialCreate','8.0.2'),('20260124162754_AddMenu','8.0.2'),('20260126131844_AddMenuAndMenuItem','8.0.2'),('20260202160704_FixPoiRelations','8.0.2'),('20260204144449_AddShopEntity','8.0.2'),('20260323161943_AddCreatedAtToUsers','8.0.2'),('20260330123249_AddFullNameToUser','8.0.2'),('20260331080457_AddPoiLocation','8.0.2'),('20260331103705_AddServicePackagesAndSubscriptions','8.0.2'),('20260331183650_AddAudioUrlToTranslation','8.0.2'),('20260405165546_AllowMultipleShopsPerOwner','8.0.2'),('20260405174141_AddMenuImagesToPoi','8.0.2'),('20260405181200_AddAudioUrlToPoi','8.0.2'),('20260406130843_AddShopStatsAndReviews','8.0.2'),('20260407065123_AddShopCategoryId','8.0.2');
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
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'Lẩu/Nướng','lau-nuong',1,'2026-04-07 06:58:38.152621'),(2,'Cơm','com',1,'2026-04-07 06:59:14.684006');
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
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pois`
--

LOCK TABLES `pois` WRITE;
/*!40000 ALTER TABLE `pois` DISABLE KEYS */;
INSERT INTO `pois` VALUES (1,'25 Nguyễn Bỉnh Khiêm Quận 1 TPHCM',NULL,NULL,NULL,NULL,NULL),(3,'12312331232','',10.762622,106.660172,NULL,NULL),(4,'hẻm 185 Lý Thường Kiệt','',10.76205788895036,106.65913581848146,NULL,NULL),(5,'25/25 Nguyễn Bỉnh Khiêm','',10.787906443351822,106.704078912735,NULL,NULL),(6,'25/25 Nguyễn Bỉnh Khiêm','',10.787545241099394,106.7039695603973,NULL,NULL),(7,'25/25 Nguyễn Bỉnh Khiêm','/uploads/images/edd44962-7f0c-4d22-90ae-d7a5c66d2488.jpg',10.787567840986723,106.70397429540412,'',NULL),(10,'Đại học Sài Gòn cơ sở 1','/uploads/images/20a2b31e-db62-406b-9d5a-b88649fc8752.jpg',10.779575072202105,106.6847723722458,NULL,NULL),(11,'Hẻm 310 Ngô Quyền','/uploads/images/cc9a7f6f-1792-4245-bc62-b1a4f5614c69.jpg',10.765641511122345,106.66411936283113,'/uploads/images/4c9f50fc-dc1f-4099-a9c2-170c66f0d296.jpg',NULL),(12,'ĐHSG','/uploads/images/9cb9e858-af1b-424d-911e-2fb9ae13ff67.jpg',10.759272737509738,106.68320748818378,NULL,NULL);
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
) ENGINE=InnoDB AUTO_INCREMENT=25 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `poitranslations`
--

LOCK TABLES `poitranslations` WRITE;
/*!40000 ALTER TABLE `poitranslations` DISABLE KEYS */;
INSERT INTO `poitranslations` VALUES (1,1,'vi','Ốc','Ốc ngon lắm','/audio/poi_1_vi.mp3'),(3,3,'vi','Cơm','Cơm gà xối mỡ',NULL),(4,4,'vi','Cơm','Cơm gà xối mỡ',NULL),(5,5,'vi','Cơm gà','Cơm gà xối mỡ',NULL),(6,6,'vi','Cơm gà nè','Cơm gà xỗi mỡ nè ngon lắm\n',NULL),(7,7,'vi','Cơm gà Nè','Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người','/audio/poi_7_vi_db54da68.mp3'),(9,7,'en','Chicken rice','Big brother Big brother Chicken rice with fat is very delicious, everyone stops by to eat and support, iu everyone','/audio/poi_7_en_aa5aae3c.mp3'),(10,7,'zh','鸡肉饭','大哥，大哥，鸡肉饭加油非常美味，大家都来吃喝支持，iu','/audio/poi_7_zh_2a835cc7.mp3'),(11,7,'th','Cơm gà Nè','Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người\n',NULL),(12,7,'de','Cơm gà Nè','Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người\n',NULL),(16,10,'vi','Anh Lớn Quán','Quán của Anh Lớn có cơm ngon lắm','/audio/poi_10_vi_558160b8.mp3'),(17,10,'en','Mr. Lon Quan','Anh Lon\'s restaurant has very good rice','/audio/poi_10_en_4b3fc59d.mp3'),(18,10,'zh','龙全先生','安龙的餐厅米饭非常好吃','/audio/poi_10_zh_b470ff65.mp3'),(19,11,'vi','Trạm Dừng Chân','Quán nướng Quán nướng Quán nướng Quán nướng Quán nướng','/audio/poi_11_vi_da42fd06.mp3'),(20,11,'en','Rest Stations','Grill Grill','/audio/poi_11_en_627301b9.mp3'),(21,11,'zh','休息站','烧烤烧烤','/audio/poi_11_zh_4b17035b.mp3'),(22,12,'vi','Test','Cơm Đại học Sài Gòn',NULL),(23,12,'en','Test','Cơm Đại học Sài Gòn',NULL),(24,12,'zh','Test','Cơm Đại học Sài Gòn',NULL);
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
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `servicepackages`
--

LOCK TABLES `servicepackages` WRITE;
/*!40000 ALTER TABLE `servicepackages` DISABLE KEYS */;
INSERT INTO `servicepackages` VALUES (1,'Basic','Basic',99000.00,990000.00,'Goi co ban cho seller nho.','Hien thi tren ban do|Toi da 1 gian hang|1 Menu, khong gioi han mon|Ho tro qua email',1,1,'2026-01-01 00:00:00.000000','OWNER',0),(2,'Premium','Premium',299000.00,2990000.00,'Goi mo rong cho seller dang tang truong.','Tat ca tinh nang Basic|Toi da 3 gian hang|Badge Premium tren app|Uu tien de xuat score +50|Thong ke nang cao|Ho tro uu tien',3,1,'2026-01-01 00:00:00.000000','OWNER',0),(3,'VIP','VIP',599000.00,5990000.00,'Goi day du cho seller lon va chuoi gian hang.','Tat ca tinh nang Premium|Toi da 5 gian hang|Badge VIP tren app|Top de xuat score +100|Thong ke chi tiet|Quang cao tren banner|Ho tro rieng 24/7',5,1,'2026-01-01 00:00:00.000000','OWNER',0),(4,'Audio Ngay','Basic',20000.00,20000.00,'Goi audio theo ngay cho user can nghe nhanh.','Su dung trong 1 ngay|Nghe thuyet minh 3 ngon ngu|Phu hop cho chuyen di ngan',0,1,'2026-01-01 00:00:00.000000','USER',1),(5,'Audio Thang','Premium',100000.00,100000.00,'Goi audio theo thang cho user nghe thuong xuyen.','Su dung trong 1 thang|Nghe thuyet minh 3 ngon ngu|Khong gioi han luot nghe trong thoi gian goi',0,1,'2026-01-01 00:00:00.000000','USER',1),(6,'Audio Nam','VIP',999000.00,999000.00,'Goi audio theo nam cho user su dung lau dai.','Su dung trong 1 nam|Nghe thuyet minh 3 ngon ngu|Tiet kiem chi phi cho nguoi nghe thuong xuyen',0,1,'2026-01-01 00:00:00.000000','USER',1);
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
  PRIMARY KEY (`Id`),
  KEY `IX_Shops_PoiId` (`PoiId`),
  KEY `IX_Shops_OwnerId` (`OwnerId`),
  KEY `IX_Shops_CategoryId` (`CategoryId`),
  CONSTRAINT `FK_Shops_Categories_CategoryId` FOREIGN KEY (`CategoryId`) REFERENCES `categories` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Shops_POIs_PoiId` FOREIGN KEY (`PoiId`) REFERENCES `pois` (`Id`),
  CONSTRAINT `FK_Shops_Users_OwnerId` FOREIGN KEY (`OwnerId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `shops`
--

LOCK TABLES `shops` WRITE;
/*!40000 ALTER TABLE `shops` DISABLE KEYS */;
INSERT INTO `shops` VALUES (6,'Cơm gà Nè','Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người','25/25 Nguyễn Bỉnh Khiêm',6,'2026-04-05 17:02:51.663043',1,7,9,7941,2),(9,'Anh Lớn Quán','Quán của Anh Lớn có cơm ngon lắm','Đại học Sài Gòn cơ sở 1',6,'2026-04-06 15:18:05.021788',1,10,13,10321,2),(10,'Trạm Dừng Chân','Quán nướng Quán nướng Quán nướng Quán nướng Quán nướng','Hẻm 310 Ngô Quyền',6,'2026-04-06 17:30:50.187522',1,11,9,17,1),(11,'Test','Cơm Đại học Sài Gòn','ĐHSG',16,'2026-04-09 03:18:18.135364',1,12,0,0,2);
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
) ENGINE=InnoDB AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `subscriptions`
--

LOCK TABLES `subscriptions` WRITE;
/*!40000 ALTER TABLE `subscriptions` DISABLE KEYS */;
INSERT INTO `subscriptions` VALUES (1,6,1,'Monthly',99000.00,'2026-04-08 08:53:38.189520','2026-05-08 08:53:38.189520','Cancelled','2026-04-08 08:53:38.189901','PayOS','Failed',1775638418006,NULL,NULL,NULL,0,NULL,NULL,NULL),(2,6,1,'Monthly',99000.00,'2026-04-08 09:10:24.071512','2026-05-08 09:10:24.071512','Cancelled','2026-04-08 09:10:24.072020','PayOS','Failed',1775639424006,NULL,NULL,NULL,0,NULL,NULL,NULL),(3,6,1,'Monthly',99000.00,'2026-04-08 09:18:09.802938','2026-05-08 09:18:09.802938','Cancelled','2026-04-08 09:18:09.803663','PayOS','Failed',1775639889006,NULL,NULL,NULL,0,NULL,NULL,NULL),(4,6,1,'Monthly',99000.00,'2026-04-08 09:28:22.900384','2026-05-08 09:28:22.900384','Cancelled','2026-04-08 09:28:22.900759','PayOS','Failed',1775640502006,NULL,NULL,NULL,0,NULL,NULL,NULL),(5,6,1,'Yearly',990000.00,'2026-04-08 09:28:23.451728','2027-04-08 09:28:23.451728','Cancelled','2026-04-08 09:28:23.451744','PayOS','Failed',1775640503006,NULL,NULL,NULL,0,NULL,NULL,NULL),(6,6,1,'Monthly',99000.00,'2026-04-08 09:28:28.187318','2026-05-08 09:28:28.187318','Cancelled','2026-04-08 09:28:28.187325','PayOS','Failed',1775640508006,NULL,NULL,NULL,0,NULL,NULL,NULL),(7,6,1,'Monthly',99000.00,'2026-04-08 09:32:57.771752','2026-05-08 09:32:57.771752','Cancelled','2026-04-08 09:32:57.772447','PayOS','Failed',1775640777006,NULL,NULL,NULL,0,NULL,NULL,NULL),(8,6,1,'Monthly',99000.00,'2026-04-08 09:32:58.417161','2026-05-08 09:32:58.417161','Cancelled','2026-04-08 09:32:58.417178','PayOS','Failed',1775640778006,NULL,NULL,NULL,0,NULL,NULL,NULL),(9,6,1,'Monthly',99000.00,'2026-04-08 10:01:01.139459','2026-04-09 14:38:46.444796','Cancelled','2026-04-08 09:36:43.425835','PayOS','Paid',1775641003006,'ca671f4849bc4a2090f20032ce601e31','https://pay.payos.vn/web/ca671f4849bc4a2090f20032ce601e31','2026-04-08 10:01:01.139459',0,NULL,NULL,NULL),(10,16,2,'Monthly',299000.00,'2026-04-09 01:16:34.783542','2026-05-09 01:16:34.783542','Cancelled','2026-04-09 01:16:34.783928','PayOS','Cancelled',1775697394016,'465e406409514c3fb886954f7b204719','https://pay.payos.vn/web/465e406409514c3fb886954f7b204719',NULL,0,NULL,NULL,NULL),(11,5,4,'Monthly',49000.00,'2026-04-09 01:23:45.008462','2026-05-09 01:23:45.008462','Cancelled','2026-04-09 01:23:45.008892','PayOS','Cancelled',1775697825005,'ade1bb9074af47bd918c56a5f1004305','https://pay.payos.vn/web/ade1bb9074af47bd918c56a5f1004305',NULL,0,NULL,NULL,NULL),(12,11,5,'Monthly',99000.00,'2026-04-09 01:33:34.954724','2026-05-09 01:33:34.954724','Cancelled','2026-04-09 01:33:34.954747','PayOS','Cancelled',1775698414011,'22450ecba72d4951a564c02103b71e9a','https://pay.payos.vn/web/22450ecba72d4951a564c02103b71e9a',NULL,0,NULL,NULL,NULL),(13,5,4,'Daily',20000.00,'2026-04-09 02:21:30.240841','2026-04-10 02:21:30.240841','Expired','2026-04-09 02:20:54.277803','PayOS','Paid',1775701254005,'2af87c6b5a144b6ea22bf4bc7b2a5de6','https://pay.payos.vn/web/2af87c6b5a144b6ea22bf4bc7b2a5de6','2026-04-09 02:21:30.240841',0,NULL,NULL,NULL),(14,11,4,'Daily',20000.00,'2026-04-09 02:28:29.557184','2026-04-10 02:28:29.557184','Cancelled','2026-04-09 02:28:29.557627','PayOS','Cancelled',1775701709011,'384168ad143945608a2040a19d44bb39','https://pay.payos.vn/web/384168ad143945608a2040a19d44bb39',NULL,0,NULL,NULL,NULL),(15,11,5,'Monthly',100000.00,'2026-04-09 02:28:39.784245','2026-05-09 02:28:39.784245','Cancelled','2026-04-09 02:28:39.784277','PayOS','Cancelled',1775701719011,'b6d7d27a2aca479d9c7caa6af5ba488b','https://pay.payos.vn/web/b6d7d27a2aca479d9c7caa6af5ba488b',NULL,0,NULL,NULL,NULL),(16,11,4,'Daily',20000.00,'2026-04-09 02:30:07.370528','2026-04-10 02:30:07.370528','Active','2026-04-09 02:29:25.994441','PayOS','Paid',1775701765011,'3832b582286945fdb26d70c53c0102e6','https://pay.payos.vn/web/3832b582286945fdb26d70c53c0102e6','2026-04-09 02:30:07.370528',0,NULL,NULL,NULL),(17,16,3,'Monthly',599000.00,'2026-04-09 03:12:18.781221','2026-05-09 03:12:18.781221','Cancelled','2026-04-09 03:12:18.781605','PayOS','Cancelled',1775704338016,'f2e86857d61d42e7adf58c9d1033112f','https://pay.payos.vn/web/f2e86857d61d42e7adf58c9d1033112f',NULL,0,NULL,NULL,NULL),(18,16,2,'Monthly',299000.00,'2026-04-09 03:12:27.516530','2026-05-09 03:12:27.516530','Cancelled','2026-04-09 03:12:27.516551','PayOS','Cancelled',1775704347016,'4c81703672ec4f958ae8c9e57ce94889','https://pay.payos.vn/web/4c81703672ec4f958ae8c9e57ce94889',NULL,0,NULL,NULL,NULL),(19,16,2,'Yearly',2990000.00,'2026-04-09 03:12:40.028811','2027-04-09 03:12:40.028811','Cancelled','2026-04-09 03:12:40.028830','PayOS','Cancelled',1775704360016,'53a7f8b75e544537b8fcdd558a80781e','https://pay.payos.vn/web/53a7f8b75e544537b8fcdd558a80781e',NULL,0,NULL,NULL,NULL),(20,16,2,'Monthly',299000.00,'2026-04-09 03:15:57.564813','2026-05-09 03:15:57.564813','Cancelled','2026-04-09 03:15:57.565363','PayOS','Cancelled',1775704557016,'44113f0b00ca446c9f03f56403a0973b','https://pay.payos.vn/web/44113f0b00ca446c9f03f56403a0973b',NULL,0,NULL,NULL,NULL),(21,16,3,'Monthly',599000.00,'2026-04-09 03:16:04.832830','2026-05-09 03:16:04.832830','Cancelled','2026-04-09 03:16:04.832850','PayOS','Cancelled',1775704564016,'9d7a5e4050444cb1ae3ca6fe0b861a0b','https://pay.payos.vn/web/9d7a5e4050444cb1ae3ca6fe0b861a0b',NULL,0,NULL,NULL,NULL),(22,16,2,'Yearly',2990000.00,'2026-04-09 03:16:11.505478','2027-04-09 03:16:11.505478','Cancelled','2026-04-09 03:16:11.505494','PayOS','Cancelled',1775704571016,'1b4f72d32d454b808ca78125ee2f2329','https://pay.payos.vn/web/1b4f72d32d454b808ca78125ee2f2329',NULL,0,NULL,NULL,NULL),(23,16,1,'Monthly',99000.00,'2026-04-09 03:17:15.705584','2026-05-09 03:17:15.705584','Active','2026-04-09 03:16:49.960380','PayOS','Paid',1775704609016,'ed1e798cff134ec2a8fd94cf9ba79eb8','https://pay.payos.vn/web/ed1e798cff134ec2a8fd94cf9ba79eb8','2026-04-09 03:17:15.705584',0,NULL,NULL,NULL),(24,6,2,'Monthly',299000.00,'2026-04-09 14:38:46.444796','2026-05-09 14:38:46.444796','Active','2026-04-09 14:38:16.365856','PayOS','Paid',1775745496006,'37dee3d82ca14918be1686115c57c9e4','https://pay.payos.vn/web/37dee3d82ca14918be1686115c57c9e4','2026-04-09 14:38:46.444796',0,NULL,NULL,NULL),(25,17,5,'Monthly',100000.00,'2026-04-09 14:42:14.509165','2026-05-09 14:42:14.509165','Active','2026-04-09 14:41:59.146265','PayOS','Paid',1775745719017,'ec4c7b5c5be44d86b71a9bd60b62f24d','https://pay.payos.vn/web/ec4c7b5c5be44d86b71a9bd60b62f24d','2026-04-09 14:42:14.509165',0,NULL,NULL,NULL),(26,5,4,'Daily',20000.00,'2026-04-11 07:24:00.793600','2026-04-12 07:24:00.793600','Active','2026-04-11 07:23:35.539395','PayOS','Paid',1775892215005,'606fb2881bc247a6bb2759b86386bd4d','https://pay.payos.vn/web/606fb2881bc247a6bb2759b86386bd4d','2026-04-11 07:24:00.793600',0,NULL,6,6),(27,18,5,'Monthly',100000.00,'2026-04-11 07:31:41.055825','2026-05-11 07:31:41.055825','Active','2026-04-11 07:30:52.634045','PayOS','Paid',1775892652018,'9bb054b0d2574ea3865815ec031e18e1','https://pay.payos.vn/web/9bb054b0d2574ea3865815ec031e18e1','2026-04-11 07:31:41.055825',0,NULL,NULL,NULL);
/*!40000 ALTER TABLE `subscriptions` ENABLE KEYS */;
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
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usagehistories`
--

LOCK TABLES `usagehistories` WRITE;
/*!40000 ALTER TABLE `usagehistories` DISABLE KEYS */;
INSERT INTO `usagehistories` VALUES (1,'user:11',9,'vi','2026-04-06 15:40:42.830202',0),(2,'user:11',6,'vi','2026-04-06 15:40:57.598003',0),(4,'user:5',9,'vi','2026-04-06 16:02:09.959686',0),(7,'user:11',6,'vi','2026-04-06 16:14:08.347897',0),(8,'user:11',10,'vi','2026-04-06 17:31:21.597360',0),(9,'user:11',6,'vi','2026-04-06 17:31:32.839861',0),(10,'user:11',10,'vi','2026-04-06 17:31:41.741527',0),(11,'user:11',10,'vi','2026-04-07 07:09:52.889772',0),(12,'user:11',6,'vi','2026-04-07 07:10:48.196421',0),(13,'user:5',9,'vi','2026-04-07 07:12:44.985642',0),(14,'user:5',9,'vi','2026-04-07 07:12:47.249543',0),(15,'user:5',6,'vi','2026-04-07 07:13:11.985593',0),(16,'user:5',10,'vi','2026-04-07 08:49:20.614649',0),(17,'user:5',10,'vi','2026-04-07 08:49:28.701567',0),(18,'user:5',10,'vi','2026-04-07 08:49:48.551770',0),(19,'user:5',9,'vi','2026-04-08 01:40:34.397183',0),(20,'user:5',10,'vi','2026-04-08 01:48:48.333084',0),(21,'user:5',10,'vi','2026-04-08 01:48:50.547764',0),(22,'user:5',9,'vi','2026-04-08 06:32:17.905552',0),(23,'user:5',9,'vi','2026-04-08 06:33:48.876752',0),(24,'user:5',9,'vi','2026-04-08 06:47:30.535662',0),(25,'user:5',9,'vi','2026-04-08 06:47:40.934232',0),(26,'user:5',9,'vi','2026-04-08 06:47:45.066097',0),(27,'user:5',9,'vi','2026-04-08 06:51:49.443695',0),(28,'user:5',10,'vi','2026-04-08 06:53:04.913659',0),(29,'user:5',6,'vi','2026-04-09 02:21:44.827511',0),(30,'user:11',6,'vi','2026-04-09 02:31:07.844827',0),(31,'user:11',6,'vi','2026-04-09 02:31:23.303574',0),(32,'user:17',9,'vi','2026-04-09 14:42:24.551109',0),(33,'user:18',9,'vi','2026-04-11 07:31:52.014096',0),(34,'user:18',6,'vi','2026-04-11 07:32:39.766640',0);
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
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'$2a$11$ovEQzNheew6f4TfLcUioL.lki21AIDIMPSE/s4VwYfFynyRsZYV42','admin@foodstreet.vn',-1,'2026-03-23 16:24:42.608944',1,''),(2,'$2a$11$JPIRHpKpcPGI2exCQQm4ZexLARl2kD1TUZzagyQuxAyKMoPITvXVC','test@gmail.com',-3,'2026-03-24 17:17:15.606367',1,''),(5,'$2a$11$BolKnkAQ3Gm4nQtRnj0heOO7PiDg76bgcpjGVvM0fEByQCh4guhBG','anhlontest@gmail.com',-3,'2026-03-30 12:39:23.326815',1,'Anh Lớn'),(6,'$2a$11$AyWzTKyqGBq1pedc43mlFekGNPvxICkYiQ3WVfBebeUHPA92/IslS','banhang@gmail.com',-2,'2026-04-05 09:51:02.905942',1,'Nguyễn Văn A'),(11,'$2a$11$jZXgw.jKRfcAX8NIRuGJZu0cT43MnPK4GgdKAU9pL8lAZ8u9Jfcu2','anhkiet@gmail.com',-3,'2026-04-06 15:19:08.710271',1,'Dương Anh Kiệt'),(14,'$2a$11$VNBh9wC5t2OeI.PXXKAZdOceeoJ4WLQMPdbKE2fUydn/VpJkcYZCy','customer.c@example.com',-3,'2026-04-05 07:06:21.000223',0,'Lê Hoàng C'),(16,'$2a$11$U84C3.2NyoK7DLiPuADZku6bxwHNRFZB6vTpT4lFp7kqsTT.jwuCG','hiuthu@gmail.com',-2,'2026-04-09 01:16:20.135780',1,''),(17,'$2a$11$fkxJ9YnYpZbAVznNrR9Nu.3MGyfiAGnerrX0IJMcv91fShe6nxoCe','kiet@gmail.com',-3,'2026-04-09 14:41:31.927941',1,'Dương Anh Kiệt'),(18,'$2a$11$MCb9Bpwsng0v7GZrZpZSvePWZJuUQfGNJL1xkM6XNwcihH.t71lNu','go@gmail.com',-3,'2026-04-11 07:30:40.998684',1,'Thằng Gơ');
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

-- Dump completed on 2026-04-12 15:16:17
select * from users;

-- Cho phép root truy cập từ mọi host
CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY 'Root123';
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;

-- Cấp quyền cụ thể cho localhost nếu cần
GRANT ALL PRIVILEGES ON *.* TO 'root'@'localhost' WITH GRANT OPTION;

-- Áp dụng thay đổi
FLUSH PRIVILEGES;
