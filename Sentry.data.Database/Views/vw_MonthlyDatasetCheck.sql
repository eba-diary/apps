CREATE VIEW dbo.vw_MonthlyDatasetCheck AS
SELECT
C.Name as Category_NME,
D.Dataset_NME as Dataset_NME,
DatasetID as Dataset_ID,
S.Schema_NME as Schema_NME,
DFC.SchemaID as Schema_ID,
S.StorageCode as Storage_CDE,
MAX(CreateDTM) as LastFileConversion_DTE,
GETDATE() as Current_DTM,
CASE WHEN FORMAT(MAX(CreateDTM),'yyyy-MM-dd') BETWEEN FORMAT(DATEADD(DAY,-31,GETDATE()),'yyyy-MM-dd') and FORMAT(GETDATE(),'yyyy-MM-dd') 
THEN  'Y'
ELSE 'N'
END as Success_IND
FROM DatasetFileConverted DFC JOIN Dataset D on d.Dataset_ID = DFC.DatasetID
JOIN [schema] s on dfc.SchemaID = s.Schema_Id
JOIN DatasetCategory DC on D.Dataset_ID = DC.Dataset_ID
JOIN Category C on DC.Category_ID = C.ID
--Eventually replace this section with a join to the DSC metadata for these datasets
WHERE D.Dataset_NME in 
('Betterview',
'CAB Public Data',
'USPS ZIP Codes',
'Zipcodes')
AND D.ObjectStatus = 1 /* Only "Active" datasets */
AND D.Display_IND = 1 
GROUP BY name,dataset_nme, schema_nme, DatasetID,SchemaID,StorageCode




