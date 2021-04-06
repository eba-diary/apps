CREATE VIEW dbo.vw_WeeklyDatasetCheck AS
SELECT
C.Name as Category_NME,
D.Dataset_NME as Dataset_NME,
DatasetID as Dataset_ID,
S.Schema_NME as Schema_NME,
DFC.SchemaID as Schema_ID,
S.StorageCode as Storage_CDE,
MAX(CreateDTM) as LastFileConversion_DTE,
GETDATE() as Current_DTM,
CASE WHEN FORMAT(MAX(CreateDTM),'yyyy-MM-dd') BETWEEN FORMAT(DATEADD(DAY,-7,GETDATE()),'yyyy-MM-dd') and FORMAT(GETDATE(),'yyyy-MM-dd') 
THEN  'Y'
ELSE 'N'
END as Success_IND
FROM DatasetFileConverted DFC JOIN Dataset D on d.Dataset_ID = DFC.DatasetID
JOIN [schema] s on dfc.SchemaID = s.Schema_Id
JOIN DatasetCategory DC on D.Dataset_ID = DC.Dataset_ID
JOIN Category C on DC.Category_ID = C.ID
--Eventually replace this section with a join to the DSC metadata for these datasets
WHERE D.Dataset_NME in 
('FNOL',
'CCC',
'CustomerOneLinking',
'PowerBeat',
'SSPO')
AND D.ObjectStatus = 1 /* Only "Active" datasets */
AND D.Display_IND = 1
--These Schemas are not loaded weekly
AND S.Schema_NME NOT IN ('CL IdentRcvy','PL Property')
GROUP BY name,dataset_nme, schema_nme, DatasetID,SchemaID,StorageCode
UNION
SELECT
C.Name as Category_NME,
D.Dataset_NME as Dataset_NME,
DatasetID as Dataset_ID,
S.Schema_NME as Schema_NME,
DFC.SchemaID as Schema_ID,
S.StorageCode as Storage_CDE,
MAX(CreateDTM) as LastFileConversion_DTE,
GETDATE() as Current_DTM,
CASE WHEN FORMAT(MAX(CreateDTM),'yyyy-MM-dd') BETWEEN FORMAT(DATEADD(DAY,-7,GETDATE()),'yyyy-MM-dd') and FORMAT(GETDATE(),'yyyy-MM-dd') 
THEN  'Y'
ELSE 'N'
END as Success_IND
FROM DatasetFileConverted DFC JOIN Dataset D on d.Dataset_ID = DFC.DatasetID
JOIN [schema] s on dfc.SchemaID = s.Schema_Id
JOIN DatasetCategory DC on D.Dataset_ID = DC.Dataset_ID
JOIN Category C on DC.Category_ID = C.ID
--Eventually replace this section with a join to the DSC metadata for these datasets
WHERE D.Dataset_NME in 
('Bitbucket')
AND D.ObjectStatus = 1 /* Only "Active" datasets */
AND D.Display_IND = 1
--These schemas are weekly while the other bitbucket schemas are daily
AND S.Schema_NME IN ('Bitbucket_BaseActivity_Detail','Bitbucket_Changeset_Detail')
GROUP BY name,dataset_nme, schema_nme, DatasetID,SchemaID,StorageCode