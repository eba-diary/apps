/*
	This script adds the GetLatest property to the Metadata column on 
	the datasets table.  This column is JSON, therefore, I am using 
	JSON_MODIFY.  This initializes this property to false.
*/

update Dataset
SET Metadata=JSON_MODIFY(Metadata,'$.ReportMetadata.GetLatest',0)
WHERE Dataset_TYP = 'RPT'