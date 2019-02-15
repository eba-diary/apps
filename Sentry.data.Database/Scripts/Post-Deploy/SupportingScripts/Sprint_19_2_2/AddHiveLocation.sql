/*
	Only adding HiveLocation to DataElement records 
	- linked to DatasetFileConfig
	- have HiveTable defined
	- have schema defined (associated dataobject record)
	- HiveLocation is null
*/

INSERT INTO DataElementDetail 
select *
from (
	select DE.DataElement_ID as 'DataElement_ID', 
	GETDATE() as 'DataElementDetailCreate_DTM', 
	GETDATE() as 'DataElementDetailChange_DTM', 
	'HiveLocation' as 'DataElementDetailType_CDE',
	Case
		WHEN ((UPPER(@@SERVERNAME) = 'FIT-N-SHARDB-12' OR UPPER(@@SERVERNAME) = 'FIT-N-SHARDB-11') AND DB_NAME() = 'SentryDatasets') THEN 'sentry-dataset-management-np/'
		WHEN ((UPPER(@@SERVERNAME) = 'FIT-N-SHARDB-12' OR UPPER(@@SERVERNAME) = 'FIT-N-SHARDB-11') AND DB_NAME() = 'SentryDatasets_NR') THEN 'sentry-dataset-management-np-nr/'
		ELSE 'sentry-dataset-management/'
	END
	+ 'parquet/' + 
	Case
		WHEN UPPER(@@SERVERNAME) = 'FIT-N-SHARDB-12' THEN 'data-dev/'
		WHEN UPPER(@@SERVERNAME) = 'FIT-N-SHARDB-11' THEN 'data-test/'
		ELSE 'data/'
	END
	+ DED_StorageCde.DataElementDetailType_VAL AS 'DataElementDetailType_VAL', 
	GETDATE() as 'LastUpdt_DTM',
	null as 'BusElementKey'
	from DataElement DE
	left join DataObject Dobj on
		DE.DataElement_ID = Dobj.DataElement_ID
	left join DataElementDetail DED_HiveLoc on
		DE.DataElement_ID = DED_HiveLoc.DataElement_ID and
		DED_HiveLoc.DataElementDetailType_CDE = 'HiveLocation'
	left join DataElementDetail DED_HiveTable on
		DE.DataElement_ID = DED_HiveTable.DataElement_ID and
		DED_HiveTable.DataElementDetailType_CDE = 'HiveTable_NME'
	left join DataElementDetail DED_StorageCde on
		DE.DataElement_ID = DED_StorageCde.DataElement_ID and
		DED_StorageCde.DataElementDetailType_CDE = 'Storage_CDE'
	where 
		DE.DataElement_CDE = 'F' and 
		DE.Config_ID is not null and
		DED_HiveTable.DataElementDetailType_CDE is not null and
		Dobj.DataObject_ID is not null and
		DED_HiveLoc.DataElementDetailType_VAL is null
) x