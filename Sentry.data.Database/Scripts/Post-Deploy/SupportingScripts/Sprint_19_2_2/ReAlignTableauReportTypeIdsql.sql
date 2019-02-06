UPDATE DatasetFileConfigs
Set FileType_ID = 3
from (
	select Config_ID as 'ID' from DatasetFileConfigs DFC
	join Dataset DS on
		DFC.Dataset_ID = DS.Dataset_ID
	where DS.Dataset_TYP = 'RPT' and DFC.FileType_ID = 0
	) x
where Config_ID = x.id