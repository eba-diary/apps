--SELECT * FROM [DataInventory].[dbo].[ScanAction]
--select top 1000 * from Column_v	where column_NME = 'ACHRouting_NBR'/*columnSAIDList_NME IS NOT NULL*/
--select top 1000 * from ColumnSensitivityCurrent_v	where SAIDList_NME IS NOT NULL 

/****************************************************************************************************************************************
HISTORY FIX TO:
	1. UPDATE SAIDExposure_NME to NEW SAID CATEGORIES
	2. UPDATE Short_NME to just be the Scan Type
****************************************************************************************************************************************/


--******************************STEP #1:  UPDATE SAIDExposure_NME to NEW SAID CATEGORIES
UPDATE ScanAction
SET SAIDExposure_NME = 'Financial Personal Information'
WHERE Alert_NME IN ('Bank Account','Account')

UPDATE ScanAction
SET SAIDExposure_NME = 'PCI'
WHERE Alert_NME IN ('Credit Card Number','CreditCard')

UPDATE ScanAction
SET SAIDExposure_NME = 'Financial Personal Information'
WHERE Alert_NME IN ('Drivers License','Social Security Number','TIN/FEIN','SocialSecurity')

UPDATE ScanAction
SET SAIDExposure_NME = 'Authentication Verifier'
WHERE Alert_NME = 'User ID and Password'


--******************************STEP #2:  HISTORY FIX Short_NME 
UPDATE ScanAction
SET Short_NME = 'Name'
WHERE Alert_NME IN ('Bank Account','Credit Card Number','Drivers License','Social Security Number','TIN/FEIN','User ID and Password','Account')

UPDATE ScanAction
SET Short_NME = 'User'
WHERE Alert_NME IN ('IsSensitive')

UPDATE ScanAction
SET Short_NME = 'Data'
WHERE Alert_NME IN ('CreditCard','SocialSecurity')


--******************************STEP #3:  run Dennis PROCs TO UPDATE
exec ReloadBaseTagScans_usp
exec ReloadBaseTag_usp

