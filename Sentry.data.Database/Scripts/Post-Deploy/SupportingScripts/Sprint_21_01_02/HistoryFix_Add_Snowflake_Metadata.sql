/**********************************************
	Sets all snowflake metadata for existing schema
***********************************************/

DECLARE @ENV VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')

update [Schema]
SET
	SnowflakeDatabase = x.DBName,
	SnowflakeSchema = x.SWSchemaNME,
	SnowflakeTable = x.TableNME,
	SnowflakeStatus =x.SWStatus
from (
	select 
		'DATA_' + @ENV as 'DBName',
		UPPER(CAT.Name) as 'SWSchemaNME',
		UPPER(REPLACE(REPLACE(REPLACE(DS.Dataset_NME, ' ', ''), '_', ''), '-', '') + '_' + REPLACE(REPLACE(REPLACE(SC.Schema_NME, ' ', ''), '_', ''), '-', '')) as 'TableNME',
		'NameReserved' as 'SWStatus',
		SC.Schema_Id as 'SCId'
	from [schema] SC
	join DatasetFileConfigs DFC on
		SC.Schema_ID = DFC.Schema_ID
	join Dataset DS on
		DFC.Dataset_ID = DS.Dataset_ID
	join DatasetCategory DC on
		DS.Dataset_ID = DC.Dataset_Id
	join Category CAT on
		DC.Category_Id = CAT.Id
) x
where Schema_Id = x.SCId