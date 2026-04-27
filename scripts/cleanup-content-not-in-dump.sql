SET NAMES utf8mb4;
START TRANSACTION;

-- Clean content tables to match the curated dump.
DELETE FROM `poitranslations`
WHERE `Id` NOT IN (1,3,4,5,6,7,9,10,11,12,16,17,18,19,20,21,25,26,27);

DELETE FROM `shops`
WHERE `Id` NOT IN (6,9,10,12);

DELETE FROM `pois`
WHERE `Id` NOT IN (1,3,4,5,6,7,10,11,13);

DELETE FROM `categories`
WHERE `Id` NOT IN (1,2,3,4);

-- Intentionally skip ServicePackages cleanup here because Subscriptions may
-- still reference legacy package IDs and would fail FK checks if deleted.

COMMIT;
