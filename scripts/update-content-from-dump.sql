SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO `categories` (`Id`, `Name`, `Slug`, `IsActive`, `CreatedAt`) VALUES
(1,'Lẩu/Nướng','lau-nuong',1,'2026-04-07 06:58:38.152621'),
(2,'Cơm','com',1,'2026-04-07 06:59:14.684006'),
(3,'Ăn vặt','an-vat',1,'2026-04-15 02:30:23.893667'),
(4,'Đồ uống','o-uong',1,'2026-04-22 03:06:02.702208')
ON DUPLICATE KEY UPDATE
`Name` = VALUES(`Name`),
`Slug` = VALUES(`Slug`),
`IsActive` = VALUES(`IsActive`),
`CreatedAt` = VALUES(`CreatedAt`);

INSERT INTO `pois` (`Id`, `Location`, `ImageUrl`, `Latitude`, `Longitude`, `MenuImagesUrl`, `AudioUrl`) VALUES
(1,'25 Nguyễn Bỉnh Khiêm Quận 1 TPHCM',NULL,NULL,NULL,NULL,NULL),
(3,'12312331232','',10.762622,106.660172,NULL,NULL),
(4,'hẻm 185 Lý Thường Kiệt','',10.76205788895036,106.65913581848146,NULL,NULL),
(5,'25/25 Nguyễn Bỉnh Khiêm','',10.787906443351822,106.704078912735,NULL,NULL),
(6,'25/25 Nguyễn Bỉnh Khiêm','',10.787545241099394,106.7039695603973,NULL,NULL),
(7,'25/25 Nguyễn Bỉnh Khiêm','/uploads/images/edd44962-7f0c-4d22-90ae-d7a5c66d2488.jpg',10.787567840986723,106.70397429540412,'',NULL),
(10,'Đại học Sài Gòn cơ sở 1','/uploads/images/20a2b31e-db62-406b-9d5a-b88649fc8752.jpg',10.779575072202105,106.6847723722458,NULL,NULL),
(11,'Hẻm 310 Ngô Quyền','/uploads/images/173ec6b8-b56c-430a-beb2-97f3ade16fea.jpg',10.765641511122345,106.66411936283113,'/uploads/images/4c9f50fc-dc1f-4099-a9c2-170c66f0d296.jpg,/uploads/images/6f19b08f-1043-4b1e-b8a2-04df92621a59.jpg,/uploads/images/3435e7b3-757e-4789-9012-553c5a41d4d0.jpg,/uploads/images/382e2aeb-ec83-4a0e-98e2-f0d90d8b4ca9.png',NULL),
(13,'','/uploads/images/d4d5dca4-5ccc-4841-9326-70520c0a420c.jpg',10.760764613001037,106.70355785469809,'/uploads/images/372c9812-4788-4af3-944c-2d5620dc2240.png',NULL)
ON DUPLICATE KEY UPDATE
`Location` = VALUES(`Location`),
`ImageUrl` = VALUES(`ImageUrl`),
`Latitude` = VALUES(`Latitude`),
`Longitude` = VALUES(`Longitude`),
`MenuImagesUrl` = VALUES(`MenuImagesUrl`),
`AudioUrl` = VALUES(`AudioUrl`);

INSERT INTO `poitranslations` (`Id`, `POIId`, `LanguageCode`, `Name`, `Description`, `AudioUrl`) VALUES
(1,1,'vi','Ốc','Ốc ngon lắm','/audio/poi_1_vi.mp3'),
(3,3,'vi','Cơm','Cơm gà xối mỡ',NULL),
(4,4,'vi','Cơm','Cơm gà xối mỡ',NULL),
(5,5,'vi','Cơm gà','Cơm gà xối mỡ',NULL),
(6,6,'vi','Cơm gà nè','Cơm gà xỗi mỡ nè ngon lắm\n',NULL),
(7,7,'vi','Cơm gà Nè','Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người','/audio/poi_7_vi_db54da68.mp3'),
(9,7,'en','Chicken rice','Big brother Big brother Chicken rice with fat is very delicious, everyone stops by to eat and support, iu everyone','/audio/poi_7_en_aa5aae3c.mp3'),
(10,7,'zh','鸡肉饭','大哥，大哥，鸡肉饭加油非常美味，大家都来吃喝支持，iu','/audio/poi_7_zh_2a835cc7.mp3'),
(11,7,'th','Cơm gà Nè','Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người\n',NULL),
(12,7,'de','Cơm gà Nè','Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người\n',NULL),
(16,10,'vi','Anh Lớn Quán','Quán của Anh Lớn có cơm ngon lắm','/audio/poi_10_vi_558160b8.mp3'),
(17,10,'en','Mr. Lon Quan','Anh Lon\'s restaurant has very good rice','/audio/poi_10_en_4b3fc59d.mp3'),
(18,10,'zh','龙全先生','安龙的餐厅米饭非常好吃','/audio/poi_10_zh_b470ff65.mp3'),
(19,11,'vi','Trạm Dừng Chân','Quán nướng siêu ngon','/audio/poi_11_vi_da42fd06.mp3'),
(20,11,'en','Rest Stations','Grill Grill','/audio/poi_11_en_627301b9.mp3'),
(21,11,'zh','休息站','烧烤烧烤','/audio/poi_11_zh_4b17035b.mp3'),
(25,13,'vi','Her coffee','Her Coffee: Chút ngọt ngào cho ngày thêm tươi mới!\nBạn đang tìm một không gian "chill" hết nấc với những góc sống ảo triệu view? Her Coffee chính là điểm dừng chân lý tưởng. Với menu đồ uống đa dạng từ Coffee đậm đà đến Trà trái cây thanh mát, Her hứa hẹn sẽ làm bừng tỉnh mọi giác quan của bạn.\n📸 Tag ngay cạ cứng và ghé Her Coffee để "nạp" vitamin tích cực ngay thôi!','/audio/poi_13_vi_2abf657c.mp3'),
(26,13,'en','Her coffee','Her Coffee: A little sweetness for a fresher day!\nAre you looking for a "chill" space with millions of views virtual living corners? Her Coffee is the ideal stop. With a diverse drink menu from Strong Coffee to Refreshing Fruit Tea, Her promises to awaken all your senses.\n📸 Tag right away and visit Her Coffee to "load" positive vitamins right away!','/audio/poi_13_en_82d15fd4.mp3'),
(27,13,'zh','她的咖啡','她的咖啡：为清新的日子增添一点甜味！\n你是在寻找一个拥有数百万浏览量、虚拟生活角落的“轻松”空间吗？她的咖啡是理想的停靠点。她提供从浓咖啡到清爽水果茶的多样化饮品菜单，承诺唤醒你所有感官。\n📸 立即标记，访问Her Coffee，立即“加载”积极维生素！','/audio/poi_13_zh_d36c2533.mp3')
ON DUPLICATE KEY UPDATE
`POIId` = VALUES(`POIId`),
`LanguageCode` = VALUES(`LanguageCode`),
`Name` = VALUES(`Name`),
`Description` = VALUES(`Description`),
`AudioUrl` = VALUES(`AudioUrl`);

INSERT INTO `shops` (`Id`, `Name`, `Description`, `Address`, `OwnerId`, `CreatedAt`, `IsActive`, `PoiId`, `ListenCount`, `ViewCount`, `CategoryId`, `AverageRating`) VALUES
(6,'Cơm gà Nè','Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Anh Lớn Cơm gà xối mỡ nè ngon lắm mọi người ghé vào ăn ủng hộ nhé iu mọi người','25/25 Nguyễn Bỉnh Khiêm',6,'2026-04-05 17:02:51.663043',1,7,9,7942,2,0),
(9,'Anh Lớn Quán','Quán của Anh Lớn có cơm ngon lắm','Đại học Sài Gòn cơ sở 1',6,'2026-04-06 15:18:05.021788',1,10,13,10321,2,0),
(10,'Trạm Dừng Chân','Quán nướng siêu ngon','Hẻm 310 Ngô Quyền',6,'2026-04-06 17:30:50.187522',1,11,10,26,1,0),
(12,'Her coffee','Her Coffee: Chút ngọt ngào cho ngày thêm tươi mới!\nBạn đang tìm một không gian "chill" hết nấc với những góc sống ảo triệu view? Her Coffee chính là điểm dừng chân lý tưởng. Với menu đồ uống đa dạng từ Coffee đậm đà đến Trà trái cây thanh mát, Her hứa hẹn sẽ làm bừng tỉnh mọi giác quan của bạn.\n📸 Tag ngay cạ cứng và ghé Her Coffee để "nạp" vitamin tích cực ngay thôi!','',16,'2026-04-22 03:06:02.783942',1,13,0,0,4,0)
ON DUPLICATE KEY UPDATE
`Name` = VALUES(`Name`),
`Description` = VALUES(`Description`),
`Address` = VALUES(`Address`),
`OwnerId` = VALUES(`OwnerId`),
`CreatedAt` = VALUES(`CreatedAt`),
`IsActive` = VALUES(`IsActive`),
`PoiId` = VALUES(`PoiId`),
`ListenCount` = VALUES(`ListenCount`),
`ViewCount` = VALUES(`ViewCount`),
`CategoryId` = VALUES(`CategoryId`),
`AverageRating` = VALUES(`AverageRating`);

INSERT INTO `servicepackages` (`Id`, `Name`, `Tier`, `MonthlyPrice`, `YearlyPrice`, `Description`, `Features`, `MaxStores`, `IsActive`, `CreatedAt`, `Audience`, `AllowAudioAccess`, `AllowAiPlanAccess`, `AllowChatbotAccess`, `AllowTinderAccess`) VALUES
(1,'Basic','Basic',99000.00,990000.00,'Gói cơ bản cho seller nhỏ.','Hiển thị trên bản đồ|Tối đa 1 gian hàng|1 Menu, không giới hạn món|Hỗ trợ qua email',1,1,'2026-01-01 00:00:00.000000','OWNER',0,0,0,0),
(2,'Premium','Premium',299000.00,2990000.00,'Gói mở rộng cho seller đang tăng trưởng.','Tất cả tính năng Basic|Tối đa 3 gian hàng|Badge Premium trên app|Ưu tiên đề xuất score +50|Thống kê nâng cao|Hỗ trợ ưu tiên',3,1,'2026-01-01 00:00:00.000000','OWNER',0,0,0,0),
(3,'VIP','VIP',599000.00,5990000.00,'Gói đầy đủ cho seller lớn và chuỗi gian hàng.','Tất cả tính năng Premium|Tối đa 5 gian hàng|Badge VIP trên app|Top đề xuất score +100|Thống kê chi tiết|Quảng cáo trên banner|Hỗ trợ riêng 24/7',5,1,'2026-01-01 00:00:00.000000','OWNER',0,0,0,0),
(4,'Audio Starter','AudioBasic',49000.00,490000.00,'Gói nghe thuyết minh cơ bản.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),
(5,'Audio Plus','AudioPremium',99000.00,990000.00,'Gói nghe thuyết minh mở rộng.','Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),
(6,'Audio Premium','AudioVIP',199000.00,1990000.00,'Gói audio cao cấp.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),
(7,'Tour Basic','TourBasic',50000.00,50000.00,'Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|Chatbot Thổ Địa tư vấn món ăn|!Tinder Ẩm Thực|!AI Kế Hoạch Tour',0,1,'2026-01-01 00:00:00.000000','USER',1,0,1,0),
(8,'Tour Plus','TourPlus',99000.00,99000.00,'Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot',0,1,'2026-01-01 00:00:00.000000','USER',1,1,1,1),
(9,'Tour Basic','TourBasic',50000.00,50000.00,'Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|Chatbot Thổ Địa tư vấn món ăn|!Tinder Ẩm Thực|!AI Kế Hoạch Tour',0,1,'2026-01-01 00:00:00.000000','USER',1,0,1,0),
(10,'Tour Plus','TourPlus',99000.00,99000.00,'Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.','Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot',0,1,'2026-01-01 00:00:00.000000','USER',1,1,1,1),
(11,'Audio Starter','AudioBasic',49000.00,490000.00,'Gói nghe thuyết minh cơ bản.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),
(12,'Audio Plus','AudioPremium',99000.00,990000.00,'Gói nghe thuyết minh mở rộng.','Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0),
(13,'Audio Premium','AudioVIP',199000.00,1990000.00,'Gói audio cao cấp.','Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên',0,0,'2026-01-01 00:00:00.000000','USER',1,0,0,0)
ON DUPLICATE KEY UPDATE
`Name` = VALUES(`Name`),
`Tier` = VALUES(`Tier`),
`MonthlyPrice` = VALUES(`MonthlyPrice`),
`YearlyPrice` = VALUES(`YearlyPrice`),
`Description` = VALUES(`Description`),
`Features` = VALUES(`Features`),
`MaxStores` = VALUES(`MaxStores`),
`IsActive` = VALUES(`IsActive`),
`CreatedAt` = VALUES(`CreatedAt`),
`Audience` = VALUES(`Audience`),
`AllowAudioAccess` = VALUES(`AllowAudioAccess`),
`AllowAiPlanAccess` = VALUES(`AllowAiPlanAccess`),
`AllowChatbotAccess` = VALUES(`AllowChatbotAccess`),
`AllowTinderAccess` = VALUES(`AllowTinderAccess`);

COMMIT;
