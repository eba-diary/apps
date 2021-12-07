--SELECT * FROM [DataInventory].[dbo].[ScanAction]

/****************************************************************************************************************************************
HISTORY FIX TO:
	1. Add new WebUse_FLG to tell DSC API to include row or not in API
****************************************************************************************************************************************/

--******************************ADD WebUse_FLG
ALTER TABLE ScanAction
ADD  WebUse_FLG BIT NULL DEFAULT 0
GO

--******************************SET FLAG FOR EACH CATEGORY
UPDATE ScanAction
SET WebUse_FLG = 1
WHERE Alert_NME IN ('Bank Account','Credit Card Number','Drivers License','Social Security Number','TIN/FEIN','User ID and Password','Account','IsSensitive','CreditCard','SocialSecurity')

UPDATE ScanAction
SET WebUse_FLG = 0
WHERE Alert_NME IN ('Encrypted')

