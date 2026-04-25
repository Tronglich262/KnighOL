CREATE DATABASE IF NOT EXISTS knightChibi
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE knightChibi;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS TradeItems, Trade, UserSkills, Skills, InventoryItems,
                     Inventory, MarketItem, NpcShopItem, NPC, UserQuests,
                     Quest, PlayerStats, PlayerState, Items, Account;

SET FOREIGN_KEY_CHECKS = 1;

SELECT * FROM NPC;
SELECT * FROM Account;
SELECT * FROM PlayerState;
SELECT * FROM PlayerStats;
SELECT * FROM Quest;
SELECT * FROM Items;
SELECT * FROM MarketItem;
SELECT * FROM Inventory;
SELECT * FROM InventoryItems;
SELECT * FROM Skills;
SELECT * FROM UserSkills;
SELECT * FROM Trade;
SELECT * FROM TradeItems;
SELECT * FROM NPC;
SELECT * FROM NpcShopItem;

-- 1. Tắt Safe Update Mode tạm thời
SET SQL_SAFE_UPDATES = 0;

-- 2. Xóa hết RefreshTokens
DELETE FROM RefreshTokens;

-- 3. Bật lại Safe Update Mode
SET SQL_SAFE_UPDATES = 1;

-- Kiểm tra đã xóa chưa
SELECT * FROM RefreshTokens;
-- ====================== Account ======================
CREATE TABLE Account (
    Account_ID     INT AUTO_INCREMENT PRIMARY KEY,
    Name           VARCHAR(100) NOT NULL,
    Email          VARCHAR(100) NOT NULL,
    PasswordHash   VARCHAR(255) NULL,
    password       VARCHAR(100) NULL,
    CurrentToken   LONGTEXT NULL,
    CharacterData  LONGTEXT NULL,
    CreatedAt      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    LastLogin      DATETIME NULL,
    IsActive       TINYINT(1) NOT NULL DEFAULT 1,
    Role           VARCHAR(50) NOT NULL DEFAULT 'player',
    UpdatedAt      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT UQ_Account_Email UNIQUE (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== PlayerState ======================
CREATE TABLE PlayerState (
    Account_ID INT PRIMARY KEY,
    Level      INT NOT NULL DEFAULT 1,
    Exp        INT NOT NULL DEFAULT 0,
    Gold       INT NOT NULL DEFAULT 0,
    Diamond    INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_PlayerState_Account FOREIGN KEY (Account_ID) REFERENCES Account(Account_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== PlayerStats ======================
CREATE TABLE PlayerStats (
    Account_ID      INT PRIMARY KEY,
    HP              INT NOT NULL DEFAULT 100,
    Strength        INT NOT NULL DEFAULT 10,
    Speed           INT NOT NULL DEFAULT 5,
    Agility         INT NOT NULL DEFAULT 5,
    Spirit          INT NOT NULL DEFAULT 5,
    PotentialPoints INT NOT NULL DEFAULT 0,
    Defense         INT NOT NULL DEFAULT 5,
    CONSTRAINT FK_PlayerStats_Account FOREIGN KEY (Account_ID) REFERENCES Account(Account_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== Quest ======================
CREATE TABLE Quest (
    Quest_ID      INT AUTO_INCREMENT PRIMARY KEY,
    Name          VARCHAR(100) NOT NULL,
    Description   LONGTEXT NULL,
    Reward_gold   INT NOT NULL DEFAULT 0,
    Reward_exp    INT NOT NULL DEFAULT 0,
    TargetType    VARCHAR(50) NULL,
    TargetId      INT NULL,
    TargetAmount  INT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== UserQuests ======================
CREATE TABLE UserQuests (
    Account_ID    INT NOT NULL,
    Quest_ID      INT NOT NULL,
    is_completed  TINYINT(1) NOT NULL DEFAULT 0,
    Progress      INT NOT NULL DEFAULT 0,
    PRIMARY KEY (Account_ID, Quest_ID),
    CONSTRAINT FK_UserQuests_Account FOREIGN KEY (Account_ID) REFERENCES Account(Account_ID) ON DELETE CASCADE,
    CONSTRAINT FK_UserQuests_Quest FOREIGN KEY (Quest_ID) REFERENCES Quest(Quest_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== Items ======================
CREATE TABLE Items (
    Item_ID       INT AUTO_INCREMENT PRIMARY KEY,
    Name          VARCHAR(100) NOT NULL,
    Type          VARCHAR(30) NOT NULL,
    Description   LONGTEXT NULL,
    Value         INT NOT NULL DEFAULT 0,
    Rarity        VARCHAR(20) NULL,
    Strength      INT NULL DEFAULT 0,
    Defense       INT NULL DEFAULT 0,
    Agility       INT NULL DEFAULT 0,
    Intelligence  INT NULL DEFAULT 0,
    Vitality      INT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE INDEX IX_Items_Name ON Items(Name);
CREATE INDEX IX_Items_Type ON Items(Type);

-- ====================== MarketItem ======================
CREATE TABLE MarketItem (
    MarketItem_ID      INT AUTO_INCREMENT PRIMARY KEY,
    Seller_Account_ID  INT NOT NULL,
    Item_ID            INT NOT NULL,
    Quantity           INT NOT NULL DEFAULT 1,
    Price              INT NOT NULL,
    CreatedAt          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_MarketItem_Seller FOREIGN KEY (Seller_Account_ID) REFERENCES Account(Account_ID) ON DELETE CASCADE,
    CONSTRAINT FK_MarketItem_Item FOREIGN KEY (Item_ID) REFERENCES Items(Item_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== Inventory ======================
CREATE TABLE Inventory (
    Inventory_ID INT AUTO_INCREMENT PRIMARY KEY,
    Account_ID   INT NOT NULL UNIQUE,
    CONSTRAINT FK_Inventory_Account FOREIGN KEY (Account_ID) REFERENCES Account(Account_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== InventoryItems ======================
CREATE TABLE InventoryItems (
    InventoryItem_ID INT AUTO_INCREMENT PRIMARY KEY,
    Inventory_ID     INT NOT NULL,
    Item_ID          INT NOT NULL,
    Quantity         INT NOT NULL DEFAULT 1,
    Is_equipped      TINYINT(1) NOT NULL DEFAULT 0,
    CONSTRAINT FK_InventoryItems_Inventory FOREIGN KEY (Inventory_ID) REFERENCES Inventory(Inventory_ID) ON DELETE CASCADE,
    CONSTRAINT FK_InventoryItems_Items FOREIGN KEY (Item_ID) REFERENCES Items(Item_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== Skills ======================
CREATE TABLE Skills (
    Skill_ID           INT AUTO_INCREMENT PRIMARY KEY,
    Name               VARCHAR(100) NOT NULL,
    Description        LONGTEXT NULL,
    Dame               INT NOT NULL DEFAULT 0,
    Cooldown_seconds   FLOAT NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== UserSkills ======================
CREATE TABLE UserSkills (
    Account_ID INT NOT NULL,
    Skill_ID   INT NOT NULL,
    Level      INT NOT NULL DEFAULT 1,
    PRIMARY KEY (Account_ID, Skill_ID),
    CONSTRAINT FK_UserSkills_Account FOREIGN KEY (Account_ID) REFERENCES Account(Account_ID) ON DELETE CASCADE,
    CONSTRAINT FK_UserSkills_Skills FOREIGN KEY (Skill_ID) REFERENCES Skills(Skill_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== Trade ======================
CREATE TABLE Trade (
    Trade_ID     INT AUTO_INCREMENT PRIMARY KEY,
    Sender_id    INT NOT NULL,
    Receiver_id  INT NOT NULL,
    Status       VARCHAR(20) NOT NULL DEFAULT 'pending',
    Created_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Trade_Sender FOREIGN KEY (Sender_id) REFERENCES Account(Account_ID),
    CONSTRAINT FK_Trade_Receiver FOREIGN KEY (Receiver_id) REFERENCES Account(Account_ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== TradeItems ======================
CREATE TABLE TradeItems (
    Trade_Item_ID INT AUTO_INCREMENT PRIMARY KEY,
    Trade_ID      INT NOT NULL,
    Item_ID       INT NOT NULL,
    Quantity      INT NOT NULL DEFAULT 1,
    From_sender   TINYINT(1) NOT NULL DEFAULT 1,
    CONSTRAINT FK_TradeItems_Trade FOREIGN KEY (Trade_ID) REFERENCES Trade(Trade_ID) ON DELETE CASCADE,
    CONSTRAINT FK_TradeItems_Items FOREIGN KEY (Item_ID) REFERENCES Items(Item_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== NPC ======================
CREATE TABLE NPC (
    Npc_ID       INT AUTO_INCREMENT PRIMARY KEY,
    Name         VARCHAR(100) NOT NULL,
    Description  LONGTEXT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ====================== NpcShopItem ======================
CREATE TABLE NpcShopItem (
    NpcShopItem_ID INT AUTO_INCREMENT PRIMARY KEY,
    Npc_ID         INT NOT NULL,
    Item_ID        INT NOT NULL,
    Price          INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_NpcShopItem_NPC FOREIGN KEY (Npc_ID) REFERENCES NPC(Npc_ID) ON DELETE CASCADE,
    CONSTRAINT FK_NpcShopItem_Item FOREIGN KEY (Item_ID) REFERENCES Items(Item_ID) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- 1. Tạo bảng RefreshTokens (nếu chưa có)
CREATE TABLE IF NOT EXISTS `RefreshTokens` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `AccountId` int NOT NULL,
  `TokenHash` longtext CHARACTER SET utf8mb4 NOT NULL,
  `ExpiresAt` datetime(6) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `IsRevoked` tinyint(1) NOT NULL DEFAULT 0,
  `ReplacedByToken` longtext CHARACTER SET utf8mb4 NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_RefreshTokens_AccountId` (`AccountId`),
  CONSTRAINT `FK_RefreshTokens_Account` 
    FOREIGN KEY (`AccountId`) REFERENCES `Account` (`Account_ID`) 
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Đánh dấu migration đã được áp dụng (quan trọng)
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260420171450_AddRefreshTokenTable', '9.0.5');
-- ====================== Seed base ======================
INSERT INTO NPC (Npc_ID, Name, Description) VALUES
(1, 'ShopKeeper', 'NPC bán đồ cơ bản'),
(2, 'Blacksmith', 'NPC bán vũ khí và giáp'),
(3, 'GeneralMerchant', 'NPC bán vật phẩm tổng hợp');

INSERT INTO Quest (Name, Description, Reward_gold, Reward_exp, TargetType, TargetId, TargetAmount) VALUES
('Tiêu diệt thủ lĩnh', 'Hạ 5 con thủ lĩnh', 3000, 10, 'KillEnemy', 105, 3);

-- Chuẩn hóa dữ liệu mặc định cho account cũ / mới
UPDATE Account SET CharacterData = '{}' WHERE CharacterData IS NULL;
UPDATE Account SET PasswordHash = password WHERE PasswordHash IS NULL AND password IS NOT NULL;

SET SESSION sql_mode = REPLACE(@@sql_mode, 'NO_AUTO_VALUE_ON_ZERO', '');


-- ====================== Items + NpcShopItem (354 dòng mỗi bảng) ======================

-- MySQL seed for Items + NpcShopItem
SET NAMES utf8mb4;

-- Optional: clear old seed data before import
DELETE FROM `NpcShopItem`;
DELETE FROM `Items`;

-- ====================== Items ======================
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (1, 'Cape.AizenBack', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (2, 'Vest.AizenHeavyArmor', 'Vest', 'tăng chỉ số', 0, '0', 50, 70, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (3, 'Belt.AizenHeavyArmor', 'Belt', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (4, 'Gloves.AizenHeavyArmor', 'Gloves', 'tăng chỉ số', 0, '0', 70, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (5, 'Boots.AizenHeavyArmor', 'Boots', 'tăng chỉ số', 0, '0', 50, 50, 70, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (6, 'Pauldrons.AizenHeavyArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (7, 'Vest.ArcaneRobe', 'Vest', 'tăng chỉ số', 0, '0', 50, 70, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (8, 'Belt.ArcaneRobe', 'Belt', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (9, 'Gloves.ArcaneRobe', 'Gloves', 'tăng chỉ số', 0, '0', 70, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (10, 'Boots.ArcaneRobe', 'Boots', 'tăng chỉ số', 0, '0', 50, 50, 70, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (11, 'Pauldrons.ArcaneRobe', 'Pauldrons', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (12, 'Cape.ArcCape', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (13, 'Vest.ArmorOfEndingLight', 'Vest', 'tăng chỉ số', 0, '0', 120, 170, 120, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (14, 'Belt.ArmorOfEndingLight', 'Belt', 'tăng chỉ số', 0, '0', 120, 120, 120, 120, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (15, 'Gloves.ArmorOfEndingLight', 'Gloves', 'tăng chỉ số', 0, '0', 170, 120, 120, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (16, 'Boots.ArmorOfEndingLight', 'Boots', 'tăng chỉ số', 0, '0', 120, 120, 170, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (17, 'Pauldrons.ArmorOfEndingLight', 'Pauldrons', 'tăng chỉ số', 0, '0', 120, 120, 120, 120, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (18, 'Vest.ArmorOfFadedHeavens', 'Vest', 'tăng chỉ số', 0, '0', 100, 150, 100, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (19, 'Belt.ArmorOfFadedHeavens ', 'Belt', 'tăng chỉ số', 0, '0', 100, 100, 100, 100, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (20, 'Gloves.ArmorOfFadedHeavens', 'Gloves', 'tăng chỉ số', 0, '0', 150, 100, 100, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (21, 'Boots.ArmorOfFadedHeavens', 'Boots', 'tăng chỉ số', 0, '0', 100, 100, 150, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (22, 'Pauldrons.ArmorOfFadedHeavens ', 'Pauldrons', 'tăng chỉ số', 0, '0', 100, 100, 100, 100, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (23, 'Vest.ArmorOfRagingCrux', 'Vest', 'tăng chỉ số', 0, '0', 170, 250, 170, 170, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (24, 'Belt.ArmorOfRagingCrux', 'Belt', 'tăng chỉ số', 0, '0', 170, 170, 170, 170, 250);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (25, 'Gloves.ArmorOfRagingCrux', 'Gloves', 'tăng chỉ số', 0, '0', 250, 170, 170, 170, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (26, 'Boots.ArmorOfRagingCrux', 'Boots', 'tăng chỉ số', 0, '0', 170, 170, 250, 170, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (27, 'Pauldrons.ArmorOfRagingCrux', 'Pauldrons', 'tăng chỉ số', 0, '0', 170, 170, 170, 170, 250);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (28, 'Vest.ArmorOfRelentlessNightmares', 'Vest', 'tăng chỉ số', 0, '0', 180, 265, 180, 180, 180);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (29, 'Belt.ArmorOfRelentlessNightmares', 'Belt', 'tăng chỉ số', 0, '0', 180, 180, 180, 180, 265);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (30, 'Gloves.ArmorOfRelentlessNightmares', 'Gloves', 'tăng chỉ số', 0, '0', 265, 180, 180, 180, 180);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (31, 'Boots.ArmorOfRelentlessNightmares', 'Boots', 'tăng chỉ số', 0, '0', 180, 180, 265, 180, 180);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (32, 'Pauldrons.ArmorOfRelentlessNightmares', 'Pauldrons', 'tăng chỉ số', 0, '0', 180, 180, 180, 180, 265);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (33, 'Back.ArrowQuiver', 'Back', 'tăng chỉ số', 0, '0', 5, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (34, 'Back.ArrowQuiver', 'Back', 'tăng chỉ số', 0, '0', 15, 15, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (35, 'Back.ArrowQuiver2', 'Back', 'tăng chỉ số', 0, '0', 20, 15, 10, 10, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (36, 'Back.ArrowQuiver3', 'Back', 'tăng chỉ số', 0, '0', 25, 25, 25, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (37, 'Back.ArrowQuiverBig', 'Back', 'tăng chỉ số', 0, '0', 35, 35, 35, 50, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (38, 'Cape.BatCape [Paint] ', 'Cape', 'tăng chỉ số', 0, '0', 10, 15, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (39, 'Vest.BlazewingArmor', 'Vest', 'tăng chỉ số', 0, '0', 150, 200, 150, 150, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (40, 'Belt.BlazewingArmor', 'Belt', 'tăng chỉ số', 0, '0', 150, 150, 150, 150, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (41, 'Gloves.BlazewingArmor', 'Gloves', 'tăng chỉ số', 0, '0', 200, 150, 150, 150, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (42, 'Boots.BlazewingArmor', 'Boots', 'tăng chỉ số', 0, '0', 150, 150, 200, 150, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (43, 'Pauldrons.BlazewingArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 150, 150, 150, 150, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (44, 'Cape.BlazewingCape', 'Cape', 'tăng chỉ số', 0, '0', 15, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (45, 'Vest.BloodiedSpiritArmor', 'Vest', 'tăng chỉ số', 0, '0', 150, 200, 150, 150, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (46, 'Belt.BloodiedSpiritArmor', 'Belt', 'tăng chỉ số', 0, '0', 150, 150, 150, 150, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (47, 'Gloves.BloodiedSpiritArmor', 'Gloves', 'tăng chỉ số', 0, '0', 200, 150, 150, 150, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (48, 'Boots.BloodiedSpiritArmor', 'Boots', 'tăng chỉ số', 0, '0', 150, 150, 200, 150, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (49, 'Pauldrons.BloodiedSpiritArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 150, 150, 150, 150, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (50, 'Cape.BloodiedSpiritCape', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (51, 'Cape.KronosCape', 'Cape', 'tăng chỉ số', 0, '0', 75, 80, 80, 80, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (52, 'Cape.KronosCogs', 'Cape', 'tăng chỉ số', 0, '0', 110, 110, 110, 120, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (53, 'Cape.StormcallerCape', 'Cape', 'tăng chỉ số', 0, '0', 45, 45, 55, 45, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (54, 'Cape.SupremeMikadoGarbCape ', 'Cape', 'tăng chỉ số', 0, '0', 60, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (55, 'Cape.TechBackV2', 'Cape', 'tăng chỉ số', 0, '0', 20, 25, 35, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (56, 'Cape.UnderlordWings', 'Cape', 'tăng chỉ số', 0, '0', 60, 80, 75, 45, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (57, 'Cape.WingsOfBrynhildr ', 'Cape', 'tăng chỉ số', 0, '0', 120, 150, 90, 120, 140);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (58, 'Cape.NemeanCape', 'Cape', 'tăng chỉ số', 0, '0', 50, 50, 50, 60, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (59, 'Cape.NominakaBack ', 'Cape', 'tăng chỉ số', 0, '0', 120, 130, 120, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (60, 'Cape.OldCape2 [Paint] ', 'Cape', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (61, 'Cape.PlatinumUnicornCape', 'Cape', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (62, 'Cape.PotOfGold', 'Cape', 'tăng chỉ số', 0, '0', 140, 150, 120, 140, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (63, 'Cape.QitianCape', 'Cape', 'tăng chỉ số', 0, '0', 130, 130, 120, 140, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (64, 'Cape.SeraphimBack', 'Cape', 'tăng chỉ số', 0, '0', 100, 150, 100, 120, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (65, 'Cape.SpikesOfDecay', 'Cape', 'tăng chỉ số', 0, '0', 60, 65, 80, 60, 60);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (66, 'Cape.CapeOfEndingLight', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 15, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (67, 'Cape.CapeOfEternalLight', 'Cape', 'tăng chỉ số', 0, '0', 10, 15, 15, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (68, 'Cape.CapeOfRagingCrux', 'Cape', 'tăng chỉ số', 0, '0', 15, 15, 15, 15, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (69, 'Cape.CapeOfRelentlessNightmares', 'Cape', 'tăng chỉ số', 0, '0', 15, 10, 15, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (70, 'Cape.CapeOfTheMadGod', 'Cape', 'tăng chỉ số', 0, '0', 15, 15, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (71, 'Cape.CapeOfTheMadGodTypeB', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 15, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (72, 'Cape.CherufeCape', 'Cape', 'tăng chỉ số', 0, '0', 20, 15, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (73, 'Cape.ChocolatierLollipop', 'Cape', 'tăng chỉ số', 0, '0', 25, 15, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (74, 'Cape.CotttonCape [Paint] ', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (75, 'Vest.DarkJusticeArmor', 'Vest', 'tăng chỉ số', 0, '0', 200, 280, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (76, 'Belt.DarkJusticeArmor', 'Belt', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (77, 'Gloves.DarkJusticeArmor', 'Gloves', 'tăng chỉ số', 0, '0', 280, 200, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (78, 'Boots.DarkJusticeArmor', 'Boots', 'tăng chỉ số', 0, '0', 200, 200, 280, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (79, 'Pauldrons.DarkJusticeArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (80, 'Cape.DarkJusticeCape', 'Cape', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (81, 'Vest.DarkKnight', 'Vest', 'tăng chỉ số', 0, '0', 120, 170, 120, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (82, 'Belt.DarkKnight', 'Belt', 'tăng chỉ số', 0, '0', 120, 120, 120, 120, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (83, 'Gloves.DarkKnight', 'Gloves', 'tăng chỉ số', 0, '0', 170, 120, 120, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (84, 'Boots.DarkKnight', 'Boots', 'tăng chỉ số', 0, '0', 120, 120, 170, 120, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (85, 'Pauldrons.DarkKnight', 'Pauldrons', 'tăng chỉ số', 0, '0', 120, 120, 120, 120, 170);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (86, 'Vest.DarkLordArmor', 'Vest', 'tăng chỉ số', 0, '0', 200, 280, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (87, 'Belt.DarkLordArmor', 'Belt', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (88, 'Gloves.DarkLordArmor', 'Gloves', 'tăng chỉ số', 0, '0', 280, 200, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (89, 'Boots.DarkLordArmor', 'Boots', 'tăng chỉ số', 0, '0', 200, 200, 280, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (90, 'Pauldrons.DarkLordArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (91, 'Cape.DragonTempestBack', 'Cape', 'tăng chỉ số', 0, '0', 70, 70, 70, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (92, 'Cape.ExorcismRing', 'Cape', 'tăng chỉ số', 0, '0', 100, 100, 100, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (93, 'Cape.FrozenDragonBack', 'Cape', 'tăng chỉ số', 0, '0', 100, 100, 100, 110, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (94, 'Glasses.GlassesTypeA', 'Glasses', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (95, 'Glasses.GlassesTypeB', 'Glasses', 'tăng chỉ số', 20, '0', 20, 20, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (96, 'Glasses.GlassesTypeM', 'Glasses', 'tăng chỉ số', 0, '0', 45, 45, 70, 70, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (97, 'Glasses.GlassesTypeO', 'Glasses', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (98, 'Glasses.LibrarianGlasses', 'Glasses', 'tăng chỉ số', 0, '0', 35, 25, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (99, 'Glasses.SteampunkGlasses', 'Glasses', 'tăng chỉ số', 0, '0', 20, 25, 30, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (100, 'Glasses.GlassesTypeC', 'Glasses', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (101, 'Glasses.GlassesTypeD', 'Glasses', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (102, 'Glasses.GlassesTypeE', 'Glasses', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (103, 'Glasses.GlassesTypeF', 'Glasses', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (104, 'Glasses.GlassesTypeH', 'Glasses', 'tăng chỉ số', 0, '0', 30, 35, 30, 20, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (105, 'Glasses.GlassesTypeI', 'Glasses', 'tăng chỉ số', 0, '0', 35, 35, 35, 35, 35);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (106, 'Glasses.GlassesTypeJ', 'Glasses', 'tăng chỉ số', 0, '0', 10, 10, 50, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (107, 'Glasses.GlassesTypeL', 'Glasses', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (108, 'Cape.GrandmaCape [Paint] ', 'Cape', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (109, 'Cape.IgnisWings', 'Cape', 'tăng chỉ số', 0, '0', 75, 80, 80, 80, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (110, 'Vest.KronosArmoredOutfit', 'Vest', 'tăng chỉ số', 0, '0', 50, 70, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (111, 'Belt.KronosArmoredOutfit ', 'Belt', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (112, 'Gloves.KronosArmoredOutfit', 'Gloves', 'tăng chỉ số', 0, '0', 70, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (113, 'Boots.KronosArmoredOutfit', 'Boots', 'tăng chỉ số', 0, '0', 50, 50, 70, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (114, 'Pauldrons.KronosArmoredOutfit', 'Pauldrons', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (115, 'Mask.BanditMask [Paint] ', 'Mask', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (116, 'Mask.Mask1 [Paint]', 'Mask', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (117, 'Mask.Mask2 [Paint] ', 'Mask', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (118, 'Mask.Piglet1 [Paint] ', 'Mask', 'tăng chỉ số', 0, '0', 15, 15, 15, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (119, 'Mask.CarnivalMask ', 'Mask', 'tăng chỉ số', 0, '0', 25, 25, 25, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (120, 'Mask.FurryNose', 'Mask', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (121, 'Mask.Piglet2 [Paint]', 'Mask', 'tăng chỉ số', 0, '0', 35, 35, 35, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (122, 'Mask.Piglet3 [Paint] ', 'Mask', 'tăng chỉ số', 0, '0', 40, 40, 40, 35, 35);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (123, 'Mask.Piglet4 [Paint] ', 'Mask', 'tăng chỉ số', 0, '0', 40, 40, 40, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (124, 'Mask.ShamanMask [Paint] ', 'Mask', 'tăng chỉ số', 0, '0', 40, 45, 50, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (125, 'Vest.PlatinumUnicornArmor', 'Vest', 'tăng chỉ số', 0, '0', 200, 280, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (126, 'Belt.PlatinumUnicornArmor', 'Belt', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (127, 'Gloves.PlatinumUnicornArmor', 'Gloves', 'tăng chỉ số', 0, '0', 280, 200, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (128, 'Boots.PlatinumUnicornArmor', 'Boots', 'tăng chỉ số', 0, '0', 200, 200, 280, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (129, 'Pauldrons.PlatinumUnicornArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (130, 'Shield.AdvancedGladiatorShield', 'Shield', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (131, 'Shield.AbsoluteShieldSP', 'Shield', 'tăng chỉ số', 0, '0', 40, 40, 40, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (132, 'Shield.CebertShield ', 'Shield', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (133, 'Shield.ChampionShield', 'Shield', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (134, 'Shield.CrusaderShield ', 'Shield', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (135, 'Shield.CrusaderShield ', 'Shield', 'tăng chỉ số', 0, '0', 45, 40, 25, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (136, 'Shield.DarkKnightShield', 'Shield', 'tăng chỉ số', 0, '0', 25, 35, 35, 35, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (137, 'Shield.DazzlingDevilStar', 'Shield', 'tăng chỉ số', 0, '0', 45, 45, 45, 45, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (138, 'Shield.DivineDragoonGrimoire', 'Shield', 'tăng chỉ số', 0, '0', 100, 100, 80, 120, 110);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (139, 'Shield.DivineDragoonGrimoireTypeB', 'Shield', 'tăng chỉ số', 0, '0', 100, 100, 80, 140, 110);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (140, 'Shield.DragoniteShield ', 'Shield', 'tăng chỉ số', 0, '0', 40, 40, 40, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (141, 'Shield.DragonTempestSteelCore ', 'Shield', 'tăng chỉ số', 0, '0', 110, 140, 90, 95, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (142, 'Shield.AdvancedKnightShield', 'Shield', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (143, 'Shield.DreytonShield', 'Shield', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (144, 'Shield.ElementalShield ', 'Shield', 'tăng chỉ số', 0, '0', 120, 150, 90, 90, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (145, 'Shield.EliteGuardShield ', 'Shield', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (146, 'Shield.FireKnightShield', 'Shield', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (147, 'Shield.FireWarriorShield ', 'Shield', 'tăng chỉ số', 0, '0', 25, 10, 25, 10, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (148, 'Shield.GakuraHitenShield ', 'Shield', 'tăng chỉ số', 0, '0', 140, 140, 120, 120, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (149, 'Shield.HermitLamp', 'Shield', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (150, 'Shield.HexWoodenShield', 'Shield', 'tăng chỉ số', 0, '0', 25, 25, 25, 15, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (151, 'Shield.ImperialTemplarShield', 'Shield', 'tăng chỉ số', 0, '0', 45, 45, 30, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (152, 'Shield.MagicShield', 'Shield', 'tăng chỉ số', 0, '0', 45, 45, 30, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (153, 'Shield.AoiWingRelic', 'Shield', 'tăng chỉ số', 0, '0', 40, 40, 40, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (154, 'Shield.ManticoreShield ', 'Shield', 'tăng chỉ số', 0, '0', 45, 45, 30, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (155, 'Shield.NecromancerShield', 'Shield', 'tăng chỉ số', 0, '0', 45, 45, 30, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (156, 'Shield.NinjaShield', 'Shield', 'tăng chỉ số', 0, '0', 40, 55, 40, 35, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (157, 'Shield.NordicShield ', 'Shield', 'tăng chỉ số', 0, '0', 40, 55, 40, 35, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (158, 'Shield.ObsidianShield', 'Shield', 'tăng chỉ số', 0, '0', 140, 140, 120, 120, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (159, 'Shield.OrkShield', 'Shield', 'tăng chỉ số', 0, '0', 55, 70, 55, 55, 60);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (160, 'Shield.RelicOfIncandescence ', 'Shield', 'tăng chỉ số', 0, '0', 110, 140, 90, 95, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (161, 'Shield.SaintPatrickShield', 'Shield', 'tăng chỉ số', 0, '0', 55, 45, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (162, 'Shield.ShieldOfCryingDemon', 'Shield', 'tăng chỉ số', 0, '0', 55, 45, 70, 40, 65);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (163, 'Shield.ShieldOfEndingLight', 'Shield', 'tăng chỉ số', 0, '0', 55, 45, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (164, 'Shield.BasicIronShield', 'Shield', 'tăng chỉ số', 0, '0', 15, 15, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (165, 'Shield.ShieldOfRagingCrux', 'Shield', 'tăng chỉ số', 0, '0', 55, 45, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (166, 'Shield.Sincerity', 'Shield', 'tăng chỉ số', 0, '0', 140, 140, 120, 100, 140);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (167, 'Shield.TachyonRing', 'Shield', 'tăng chỉ số', 0, '0', 90, 80, 85, 1040, 120);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (168, 'Shield.TomeOfDemise', 'Shield', 'tăng chỉ số', 0, '0', 65, 80, 80, 95, 90);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (169, 'Shield.BloodiedSpiritShield', 'Shield', 'tăng chỉ số', 0, '0', 25, 20, 20, 20, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (170, 'Shield.BlueKnightShield ', 'Shield', 'tăng chỉ số', 0, '0', 25, 20, 20, 20, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (171, 'Shield.BottleBomb', 'Shield', 'tăng chỉ số', 0, '0', 25, 20, 20, 20, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (172, 'Shield.CardinalBook', 'Shield', 'tăng chỉ số', 0, '0', 25, 20, 20, 20, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (173, 'Shield.CataphractShield', 'Shield', 'tăng chỉ số', 0, '0', 30, 30, 20, 25, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (174, 'Vest.Underpants9', 'Vest', 'tăng chỉ số', 0, '0', 50, 70, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (175, 'Belt.Underpants9', 'Belt', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (176, 'Gloves.Underpants9 ', 'Gloves', 'tăng chỉ số', 0, '0', 70, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (177, 'Boots.Underpants9 ', 'Boots', 'tăng chỉ số', 0, '0', 50, 50, 70, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (178, 'Pauldrons.Underpants9', 'Pauldrons', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (179, 'Vest.UranosMail', 'Vest', 'tăng chỉ số', 0, '0', 200, 280, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (180, 'Belt.UranosMail', 'Belt', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (181, 'Gloves.UranosMail', 'Gloves', 'tăng chỉ số', 0, '0', 280, 200, 200, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (182, 'Boots.UranosMail', 'Boots', 'tăng chỉ số', 0, '0', 200, 200, 280, 200, 200);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (183, 'Pauldrons.UranosMail', 'Pauldrons', 'tăng chỉ số', 0, '0', 200, 200, 200, 200, 280);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (184, 'Vest.VikingFurArmor', 'Vest', 'tăng chỉ số', 0, '0', 20, 30, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (185, 'Belt.VikingFurArmor', 'Belt', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (186, 'Gloves.VikingFurArmor', 'Gloves', 'tăng chỉ số', 0, '0', 30, 20, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (187, 'Boots.VikingFurArmor', 'Boots', 'tăng chỉ số', 0, '0', 20, 20, 30, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (188, 'Pauldrons.VikingFurArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (189, 'Vest.VikingLightArmor2', 'Vest', 'tăng chỉ số', 0, '0', 5, 10, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (190, 'Belt.VikingLightArmor2', 'Belt', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (191, 'Gloves.VikingLightArmor2', 'Gloves', 'tăng chỉ số', 0, '0', 10, 5, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (192, 'Boots.VikingLightArmor2', 'Boots', 'tăng chỉ số', 0, '0', 5, 5, 10, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (193, 'Pauldrons.VikingLightArmor2', 'Pauldrons', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (194, 'Vest.VikingLightArmor3', 'Vest', 'tăng chỉ số', 0, '0', 5, 10, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (195, 'Belt.VikingLightArmor3', 'Belt', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (196, 'Gloves.VikingLightArmor3', 'Gloves', 'tăng chỉ số', 0, '0', 10, 5, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (197, 'Boots.VikingLightArmor3', 'Boots', 'tăng chỉ số', 0, '0', 5, 5, 10, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (198, 'Pauldrons.VikingLightArmor3', 'Pauldrons', 'tăng chỉ số', 0, '0', 5, 5, 5, 5, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (199, 'Vest.VikingRoughArmor1', 'Vest', 'tăng chỉ số', 0, '0', 10, 15, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (200, 'Belt.VikingRoughArmor1 ', 'Belt', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (201, 'Gloves.VikingRoughArmor1 ', 'Gloves', 'tăng chỉ số', 0, '0', 15, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (202, 'Boots.VikingRoughArmor1', 'Boots', 'tăng chỉ số', 0, '0', 10, 10, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (203, 'Pauldrons.VikingRoughArmor1', 'Pauldrons', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (204, 'Vest.VikingRoughArmor2', 'Vest', 'tăng chỉ số', 0, '0', 20, 30, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (205, 'Belt.VikingRoughArmor2', 'Belt', 'tăng chỉ số', 0, '0', 20, 30, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (206, 'Gloves.VikingRoughArmor2', 'Gloves', 'tăng chỉ số', 0, '0', 30, 20, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (207, 'Boots.VikingRoughArmor2', 'Boots', 'tăng chỉ số', 0, '0', 20, 20, 30, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (208, 'Pauldrons.VikingRoughArmor2', 'Pauldrons', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (209, 'Vest.VikingRoughArmor3 [Paint] ', 'Vest', 'tăng chỉ số', 0, '0', 30, 45, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (210, 'Belt.VikingRoughArmor3 [Paint] ', 'Belt', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (211, 'Gloves.VikingRoughArmor3 [Paint]', 'Gloves', 'tăng chỉ số', 0, '0', 45, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (212, 'Boots.VikingRoughArmor3 [Paint] ', 'Boots', 'tăng chỉ số', 0, '0', 30, 30, 45, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (213, 'Pauldrons.VikingRoughArmor3 [Paint] ', 'Pauldrons', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (214, 'Vest.VoidDragonArmor', 'Vest', 'tăng chỉ số', 0, '0', 100, 150, 100, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (215, 'Belt.VoidDragonArmor', 'Belt', 'tăng chỉ số', 0, '0', 150, 100, 100, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (216, 'Gloves.VoidDragonArmor', 'Gloves', 'tăng chỉ số', 0, '0', 150, 100, 100, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (217, 'Boots.VoidDragonArmor', 'Boots', 'tăng chỉ số', 0, '0', 100, 100, 150, 100, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (218, 'Pauldrons.VoidDragonArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 100, 100, 100, 100, 150);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (219, 'Vest.WallKeeperArmor', 'Vest', 'tăng chỉ số', 0, '0', 20, 30, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (220, 'Belt.WallKeeperArmor', 'Belt', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (221, 'Gloves.WallKeeperArmor', 'Gloves', 'tăng chỉ số', 0, '0', 30, 20, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (222, 'Boots.WallKeeperArmor', 'Boots', 'tăng chỉ số', 0, '0', 20, 20, 30, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (223, 'Pauldrons.WallKeeperArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (224, 'Vest.Wanderer', 'Vest', 'tăng chỉ số', 0, '0', 70, 100, 70, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (225, 'Belt.Wanderer', 'Belt', 'tăng chỉ số', 0, '0', 70, 70, 70, 70, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (226, 'Gloves.Wanderer', 'Gloves', 'tăng chỉ số', 0, '0', 100, 70, 70, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (227, 'Boots.Wanderer', 'Boots', 'tăng chỉ số', 0, '0', 70, 70, 100, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (228, 'Pauldrons.Wanderer', 'Pauldrons', 'tăng chỉ số', 0, '0', 70, 70, 70, 70, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (229, 'Vest.WarlockArmor', 'Vest', 'tăng chỉ số', 0, '0', 70, 100, 70, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (230, 'Belt.WarlockArmor', 'Belt', 'tăng chỉ số', 0, '0', 70, 70, 70, 70, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (231, 'Gloves.WarlockArmor', 'Gloves', 'tăng chỉ số', 0, '0', 100, 70, 70, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (232, 'Boots.WarlockArmor', 'Boots', 'tăng chỉ số', 0, '0', 70, 70, 100, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (233, 'Pauldrons.WarlockArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 70, 70, 70, 70, 100);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (234, 'Vest.WaterWizardArmor', 'Vest', 'tăng chỉ số', 0, '0', 50, 70, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (235, 'Belt.WaterWizardArmor', 'Belt', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (236, 'Gloves.WaterWizardArmor', 'Gloves', 'tăng chỉ số', 0, '0', 70, 50, 50, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (237, 'Boots.WaterWizardArmor', 'Boots', 'tăng chỉ số', 0, '0', 50, 50, 70, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (238, 'Pauldrons.WaterWizardArmor ', 'Pauldrons', 'tăng chỉ số', 0, '0', 50, 50, 50, 50, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (239, 'Vest.WhiteKittyRobe', 'Vest', 'tăng chỉ số', 0, '0', 30, 45, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (240, 'Belt.WhiteKittyRobe', 'Belt', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (241, 'Gloves.WhiteKittyRobe', 'Gloves', 'tăng chỉ số', 0, '0', 45, 30, 30, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (242, 'Boots.WhiteKittyRobe', 'Boots', 'tăng chỉ số', 0, '0', 30, 30, 45, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (243, 'Pauldrons.WhiteKittyRobe', 'Pauldrons', 'tăng chỉ số', 0, '0', 30, 30, 30, 30, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (244, 'Vest.WitchArmor ', 'Vest', 'tăng chỉ số', 0, '0', 20, 30, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (245, 'Belt.WitchArmor', 'Belt', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (246, 'Boots.WitchArmor', 'Boots', 'tăng chỉ số', 0, '0', 20, 20, 30, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (247, 'Pauldrons.WitchArmor', 'Pauldrons', 'tăng chỉ số', 0, '0', 20, 20, 20, 20, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (248, 'Pauldrons.WitchHunterArmor [Paint] ', 'Pauldrons', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (249, 'Vest.WitchHunterArmor [Paint] ', 'Vest', 'tăng chỉ số', 0, '0', 10, 15, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (250, 'Belt.VikingLightArmor3', 'Belt', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (251, 'Gloves.WitchHunterArmor [Paint] ', 'Gloves', 'tăng chỉ số', 0, '0', 15, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (252, 'Boots.WitchHunterArmor [Paint] ', 'Boots', 'tăng chỉ số', 0, '0', 10, 10, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (253, 'Pauldrons.WitchHunterArmor [Paint] ', 'Pauldrons', 'tăng chỉ số', 0, '0', 10, 10, 10, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (254, 'Helmet.AdvancedDipperHat', 'Helmet', 'tang ch? s?', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (255, 'Helmet.AizenHeavyHelm', 'Helmet', 'tang ch? s?', 0, '0', 35, 35, 45, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (256, 'Helmet.AmateurDipperHat', 'Helmet', 'tang ch? s?', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (257, 'Helmet.ArcaneHat', 'Helmet', 'tang ch? s?', 0, '0', 35, 35, 45, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (258, 'Helmet.ArchHelm', 'Helmet', 'tang ch? s?', 0, '0', 35, 35, 45, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (259, 'Helmet.ArcherHat', 'Helmet', 'tang ch? s?', 0, '0', 10, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (260, 'Helmet.Arrows[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 25, 25, 20, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (261, 'Helmet.AssassinHood[Paint]', 'Helmet', 'tang ch? s?', 0, '0', 25, 25, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (262, 'Helmet.AsteraHood', 'Helmet', 'tang ch? s?', 0, '0', 25, 20, 20, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (263, 'Helmet.BattleguardHelm', 'Helmet', 'tang ch? s?', 0, '0', 25, 25, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (264, 'Helmet.BerserkHelm', 'Helmet', 'tang ch? s?', 0, '0', 25, 20, 30, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (265, 'Helmet.BishopHat[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 25, 25, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (266, 'Helmet.BlazewingHelmet', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (267, 'Helmet.BloodiedSpiritHelmet', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (268, 'Helmet.Blossom[Paint][FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (269, 'Helmet.BootlegHat', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (270, 'Helmet.BowmanHat [Paint]', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (271, 'Helmet.Brainz[Paint][FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 35, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (272, 'Helmet.BunnyEarsA1[Paint][FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 25, 30, 25, 25, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (273, 'Helmet.DestroyerHelm[Paint]', 'Helmet', 'tang ch? s?', 0, '0', 25, 30, 20, 25, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (274, 'Helmet.DraconicHelmet', 'Helmet', 'tang ch? s?', 0, '0', 25, 30, 20, 25, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (275, 'Helmet.DragonTempestSteelGuard', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 60, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (276, 'Helmet.DrifterForehead', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 60, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (277, 'Helmet.ElegantArcherHood', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 60, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (278, 'Helmet.ElementalHelmet', 'Helmet', 'tang ch? s?', 0, '0', 60, 60, 45, 66, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (279, 'Helmet.ElementalMageHood', 'Helmet', 'tang ch? s?', 0, '0', 60, 60, 45, 66, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (280, 'Helmet.EnchantedScarletHelm', 'Helmet', 'tang ch? s?', 0, '0', 60, 60, 45, 66, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (281, 'Helmet.HawkHelm', 'Helmet', 'tang ch? s?', 0, '0', 5, 5, 5, 5, 5);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (282, 'Helmet.HelmetOfCryingDemon', 'Helmet', 'tang ch? s?', 0, '0', 60, 60, 45, 66, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (283, 'Helmet.HelmetOfForgottenFoe[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 30, 30, 35, 20, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (284, 'Helmet.HelmetOfLoneWolf', 'Helmet', 'tang ch? s?', 0, '0', 30, 30, 35, 20, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (285, 'Helmet.HelmetOfRagingCrux', 'Helmet', 'tang ch? s?', 0, '0', 40, 40, 40, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (286, 'Helmet.HelmetOfRelentlessNightmares[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 80, 80, 80, 80, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (287, 'Helmet.HelmetOfTheMadGod', 'Helmet', 'tang ch? s?', 0, '0', 80, 80, 80, 80, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (288, 'Helmet.HelmetOfTheMadGodTypeB', 'Helmet', 'tang ch? s?', 0, '0', 100, 100, 100, 120, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (289, 'Helmet.HelmetOfBrynhildr[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 80, 80, 80, 80, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (290, 'Helmet.HighCrusaderHelm', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 70, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (291, 'Helmet.IndraGemmedCrown[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 70, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (292, 'Helmet.JesterHat', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 70, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (293, 'Helmet.KingCrown', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 70, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (294, 'Helmet.OzeriasHood', 'Helmet', 'tang ch? s?', 0, '0', 50, 55, 40, 70, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (295, 'Helmet.StormcallerHelmet[FullHair]', 'Helmet', 'tang ch? s?', 0, '0', 100, 120, 150, 120, 140);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (296, 'Helmet.VoidDragonHelmet', 'Helmet', 'tang ch? s?', 0, '0', 100, 120, 150, 120, 140);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (297, 'Bow.CurvedBow', 'Bow', 'tang ch? s?', 0, '0', 20, 10, 15, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (298, 'Bow.FlameSalamanderBow', 'Bow', 'tang ch? s?', 0, '0', 25, 10, 15, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (299, 'Bow.HeavyBow', 'Bow', 'tang ch? s?', 0, '0', 25, 10, 10, 10, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (300, 'Bow.RedwoodBow', 'Bow', 'tang ch? s?', 0, '0', 25, 10, 10, 15, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (301, 'Bow.RetributionBow', 'Bow', 'tang ch? s?', 0, '0', 25, 10, 20, 10, 15);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (302, 'Bow.TwinEdgeBow', 'Bow', 'tang ch? s?', 0, '0', 25, 10, 20, 20, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (303, 'Bow.RisenBow', 'Bow', 'tang ch? s?', 0, '0', 30, 20, 25, 15, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (304, 'Bow.WyvernHornBow', 'Bow', 'tang ch? s?', 0, '0', 30, 20, 20, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (305, 'Bow.CurvedBow', 'Bow', 'tang ch? s?', 0, '0', 40, 25, 30, 20, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (306, '.Bow.BattleBow', 'Bow', 'tang ch? s?', 0, '0', 40, 25, 25, 20, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (307, 'Bow.FamilyBow', 'Bow', 'tang ch? s?', 0, '0', 40, 25, 30, 20, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (308, 'Bow.GoldDragon', 'Bow', 'tang ch? s?', 0, '0', 40, 25, 30, 20, 10);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (309, 'Bow.HornBow', 'Bow', 'tang ch? s?', 0, '0', 55, 20, 45, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (310, 'Bow.HunterBow', 'Bow', 'tang ch? s?', 0, '0', 55, 20, 45, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (311, 'Bow.HunterShortBow', 'Bow', 'tang ch? s?', 0, '0', 55, 20, 45, 20, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (312, 'Bow.MarauderBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 40, 25, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (313, 'Bow.OrcBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 40, 25, 20);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (314, 'Bow.PathFinderBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 45, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (315, 'Bow.RangerBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 50, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (316, 'Bow.RoyalArcherBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 50, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (317, 'Bow.ScoutBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 55, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (318, 'Bow.ScoutShortBow', 'Bow', 'tang ch? s?', 0, '0', 60, 25, 55, 25, 25);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (319, 'Bow.SniperBow', 'Bow', 'tang ch? s?', 0, '0', 70, 35, 55, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (320, 'Bow.WitchHunterBow', 'Bow', 'tang ch? s?', 0, '0', 70, 35, 55, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (321, 'Bow.SamuraiBow1', 'Bow', 'tang ch? s?', 0, '0', 70, 35, 55, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (322, 'Bow.SamuraiBow2', 'Bow', 'tang ch? s?', 0, '0', 70, 35, 55, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (323, 'Bow.SamuraiBow3', 'Bow', 'tang ch? s?', 0, '0', 70, 35, 55, 30, 30);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (324, 'Bow.BowOfCompassion', 'Bow', 'tang ch? s?', 0, '0', 100, 40, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (325, 'Bow.BowOfGrace', 'Bow', 'tang ch? s?', 0, '0', 100, 40, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (326, 'Bow.CherufeArc', 'Bow', 'tang ch? s?', 0, '0', 100, 40, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (327, 'Bow.ElementalBow', 'Bow', 'tang ch? s?', 0, '0', 100, 40, 70, 40, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (328, 'Bow.ElementalBow2', 'Bow', 'tang ch? s?', 0, '0', 120, 45, 75, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (329, 'Bow.EsmeraldaArco', 'Bow', 'tang ch? s?', 0, '0', 120, 45, 75, 55, 55);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (330, 'Bow.LakeDragonBow', 'Bow', 'tang ch? s?', 0, '0', 120, 45, 65, 45, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (331, 'Bow.LakeDragonBow2', 'Bow', 'tang ch? s?', 0, '0', 120, 65, 65, 55, 55);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (332, 'Bow.LamentBow', 'Bow', 'tang ch? s?', 0, '0', 140, 45, 70, 45, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (333, 'Bow.MagnetismBow', 'Bow', 'tang ch? s?', 0, '0', 140, 45, 70, 45, 45);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (334, 'Bow.PhoenixBow', 'Bow', 'tang ch? s?', 0, '0', 150, 60, 80, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (335, 'Bow.SteampunkBow', 'Bow', 'tang ch? s?', 0, '0', 200, 80, 120, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (336, 'Bow.VeracityColdbow', 'Bow', 'tang ch? s?', 0, '0', 250, 60, 150, 50, 50);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (337, 'MeleeWeapon1H.HabakiriHitenSword', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 230, 70, 120, 50, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (338, 'MeleeWeapon1H.ObsidianEdge', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 230, 70, 120, 50, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (339, 'MeleeWeapon1H.ObsidianSword', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 230, 70, 120, 50, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (340, 'MeleeWeapon1H.SwordOfAjero', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 230, 70, 120, 50, 40);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (341, 'MeleeWeapon1H.SwordOfDemise', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 300, 100, 200, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (342, 'MeleeWeapon1H.SwordOfHallowTypeB', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 300, 100, 200, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (343, 'MeleeWeapon1H.SwordOfTheSnow', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 300, 100, 200, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (344, 'MeleeWeapon1H.TachyonSword', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 300, 100, 200, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (345, 'MeleeWeapon1H.TerraSword', 'MeleeWeapon1H', 'tang ch? s?', 0, '0', 300, 100, 200, 70, 70);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (346, 'MeleeWeapon2H.SamuraiSword3 [Paint]', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 120, 55, 75, 30, 60);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (347, 'MeleeWeapon2H.ShadowSword', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 120, 55, 75, 30, 60);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (348, 'MeleeWeapon2H.SunriseSword', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 120, 55, 75, 30, 60);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (349, 'MeleeWeapon2H.NemeanMace', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 180, 55, 80, 40, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (350, 'MeleeWeapon2H.FrozenDragonAxe', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 180, 55, 80, 40, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (351, 'MeleeWeapon2H.LotusPrinceLance', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 220, 70, 110, 40, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (352, 'MeleeWeapon2H.SteampunkSword', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 250, 70, 140, 40, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (353, 'MeleeWeapon2H.TerraLongsword', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 250, 70, 170, 40, 80);
INSERT INTO `Items` (`Item_ID`, `Name`, `Type`, `Description`, `Value`, `Rarity`, `Strength`, `Defense`, `Agility`, `Intelligence`, `Vitality`) VALUES (354, 'MeleeWeapon2H.VoidDragonLance', 'MeleeWeapon2H', 'tang ch? s?', 0, '0', 300, 70, 150, 40, 80);

-- ====================== NpcShopItem ======================
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (1, 2, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (2, 7, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (3, 13, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (4, 18, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (5, 23, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (6, 28, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (7, 39, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (8, 45, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (9, 75, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (10, 81, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (11, 86, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (12, 110, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (13, 125, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (14, 174, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (15, 179, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (16, 184, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (17, 189, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (18, 194, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (19, 199, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (20, 204, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (21, 209, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (22, 214, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (23, 219, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (24, 224, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (25, 229, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (26, 234, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (27, 239, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (28, 244, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (29, 249, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (30, 4, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (31, 9, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (32, 15, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (33, 20, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (34, 25, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (35, 30, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (36, 41, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (37, 47, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (38, 77, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (39, 83, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (40, 88, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (41, 112, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (42, 127, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (43, 176, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (44, 181, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (45, 186, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (46, 191, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (47, 196, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (48, 201, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (49, 206, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (50, 211, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (51, 216, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (52, 221, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (53, 226, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (54, 231, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (55, 236, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (56, 241, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (57, 251, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (58, 5, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (59, 10, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (60, 16, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (61, 21, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (62, 26, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (63, 31, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (64, 42, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (65, 48, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (66, 78, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (67, 84, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (68, 89, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (69, 113, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (70, 128, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (71, 177, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (72, 182, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (73, 187, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (74, 192, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (75, 197, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (76, 202, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (77, 207, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (78, 212, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (79, 217, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (80, 222, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (81, 227, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (82, 232, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (83, 237, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (84, 242, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (85, 246, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (86, 252, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (87, 3, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (88, 8, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (89, 14, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (90, 19, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (91, 24, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (92, 29, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (93, 40, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (94, 46, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (95, 76, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (96, 82, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (97, 87, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (98, 111, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (99, 126, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (100, 175, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (101, 180, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (102, 185, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (103, 190, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (104, 195, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (105, 200, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (106, 205, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (107, 210, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (108, 215, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (109, 220, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (110, 225, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (111, 230, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (112, 235, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (113, 240, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (114, 245, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (115, 250, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (116, 6, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (117, 11, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (118, 17, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (119, 22, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (120, 27, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (121, 32, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (122, 43, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (123, 49, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (124, 79, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (125, 85, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (126, 90, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (127, 114, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (128, 129, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (129, 178, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (130, 183, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (131, 188, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (132, 193, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (133, 198, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (134, 203, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (135, 208, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (136, 213, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (137, 218, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (138, 223, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (139, 228, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (140, 233, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (141, 238, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (142, 243, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (143, 247, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (144, 248, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (145, 253, 10000, 2);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (146, 1, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (147, 12, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (148, 38, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (149, 44, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (150, 50, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (151, 51, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (152, 52, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (153, 53, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (154, 54, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (155, 55, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (156, 56, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (157, 57, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (158, 58, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (159, 59, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (160, 60, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (161, 61, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (162, 62, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (163, 63, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (164, 64, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (165, 65, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (166, 66, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (167, 67, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (168, 68, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (169, 69, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (170, 70, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (171, 71, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (172, 72, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (173, 73, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (174, 74, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (175, 80, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (176, 91, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (177, 92, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (178, 93, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (179, 108, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (180, 109, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (181, 33, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (182, 34, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (183, 35, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (184, 36, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (185, 37, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (186, 94, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (187, 95, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (188, 96, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (189, 97, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (190, 98, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (191, 99, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (192, 100, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (193, 101, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (194, 102, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (195, 103, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (196, 104, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (197, 105, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (198, 106, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (199, 107, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (200, 115, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (201, 116, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (202, 117, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (203, 118, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (204, 119, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (205, 120, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (206, 121, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (207, 122, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (208, 123, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (209, 124, 10000, 1);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (210, 130, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (211, 131, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (212, 132, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (213, 133, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (214, 134, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (215, 135, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (216, 136, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (217, 137, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (218, 138, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (219, 139, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (220, 140, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (221, 141, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (222, 142, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (223, 143, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (224, 144, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (225, 145, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (226, 146, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (227, 147, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (228, 148, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (229, 149, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (230, 150, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (231, 151, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (232, 152, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (233, 153, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (234, 154, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (235, 155, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (236, 156, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (237, 157, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (238, 158, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (239, 159, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (240, 160, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (241, 161, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (242, 162, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (243, 163, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (244, 164, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (245, 165, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (246, 166, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (247, 167, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (248, 168, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (249, 169, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (250, 170, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (251, 171, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (252, 172, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (253, 173, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (254, 254, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (255, 255, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (256, 256, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (257, 257, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (258, 258, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (259, 259, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (260, 260, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (261, 261, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (262, 262, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (263, 263, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (264, 264, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (265, 265, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (266, 266, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (267, 267, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (268, 268, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (269, 269, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (270, 270, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (271, 271, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (272, 272, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (273, 273, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (274, 274, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (275, 275, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (276, 276, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (277, 277, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (278, 278, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (279, 279, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (280, 280, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (281, 281, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (282, 282, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (283, 283, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (284, 284, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (285, 285, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (286, 286, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (287, 287, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (288, 288, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (289, 289, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (290, 290, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (291, 291, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (292, 292, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (293, 293, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (294, 294, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (295, 295, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (296, 296, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (297, 297, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (298, 298, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (299, 299, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (300, 300, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (301, 301, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (302, 302, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (303, 303, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (304, 304, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (305, 305, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (306, 306, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (307, 307, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (308, 308, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (309, 309, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (310, 310, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (311, 311, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (312, 312, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (313, 313, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (314, 314, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (315, 315, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (316, 316, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (317, 317, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (318, 318, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (319, 319, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (320, 320, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (321, 321, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (322, 322, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (323, 323, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (324, 324, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (325, 325, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (326, 326, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (327, 327, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (328, 328, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (329, 329, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (330, 330, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (331, 331, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (332, 332, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (333, 333, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (334, 334, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (335, 335, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (336, 336, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (337, 337, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (338, 338, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (339, 339, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (340, 340, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (341, 341, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (342, 342, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (343, 343, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (344, 344, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (345, 345, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (346, 346, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (347, 347, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (348, 348, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (349, 349, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (350, 350, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (351, 351, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (352, 352, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (353, 353, 10000, 3);
INSERT INTO `NpcShopItem` (`NpcShopItem_ID`, `Item_ID`, `Price`, `Npc_ID`) VALUES (354, 354, 10000, 3);

-- Sync AUTO_INCREMENT after explicit IDs
SET @max_item_id = (SELECT IFNULL(MAX(`Item_ID`), 0) + 1 FROM `Items`);
SET @sql_item = CONCAT('ALTER TABLE `Items` AUTO_INCREMENT = ', @max_item_id);
PREPARE stmt_item FROM @sql_item;
EXECUTE stmt_item;
DEALLOCATE PREPARE stmt_item;

SET @max_npcshop_id = (SELECT IFNULL(MAX(`NpcShopItem_ID`), 0) + 1 FROM `NpcShopItem`);
SET @sql_npcshop = CONCAT('ALTER TABLE `NpcShopItem` AUTO_INCREMENT = ', @max_npcshop_id);
PREPARE stmt_npcshop FROM @sql_npcshop;
EXECUTE stmt_npcshop;
DEALLOCATE PREPARE stmt_npcshop;

SELECT VERSION();
SHOW DATABASES;
USE knightChibi;
SHOW TABLES;
SELECT COUNT(*) FROM Account;
-- Kiểm tra nhanh sau import
-- SELECT COUNT(*) AS item_count FROM `Items`;
-- SELECT COUNT(*) AS npcshopitem_count FROM `NpcShopItem`;
-- SELECT COUNT(*) AS npc_count FROM `NPC`;
