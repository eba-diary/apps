CREATE VIEW dbo.vw_WeeklyDatasetCheck AS
SELECT
C.Name as Category_NME,
D.Dataset_NME as Dataset_NME,
DFC.DatasetID as Dataset_ID,
S.Schema_NME as Schema_NME,
DFC.SchemaID as Schema_ID,
S.StorageCode as Storage_CDE,
MAX(DFC.CreateDTM) as LastFileConversion_DTE,
GETDATE() as Current_DTM,
CASE WHEN FORMAT(MAX(CreateDTM),'yyyy-MM-dd') BETWEEN FORMAT(DATEADD(DAY,-7,GETDATE()),'yyyy-MM-dd') and FORMAT(GETDATE(),'yyyy-MM-dd') 
THEN  'Y'
ELSE 'N'
END as Success_IND
FROM DatasetFileParquet DFC JOIN Dataset D on d.Dataset_ID = DFC.DatasetID
JOIN [schema] s on dfc.SchemaID = s.Schema_Id
JOIN DatasetCategory DC on D.Dataset_ID = DC.Dataset_ID
JOIN Category C on DC.Category_ID = C.ID
JOIN ADTDatasetClassification ADC on ADC.Dataset_ID = DFC.DatasetID and ADC.Schema_ID = DFC.SchemaID
WHERE D.ObjectStatus = 1 /* Only "Active" datasets */
AND D.Display_IND = 1
AND ADC.Delivery_TYP = 'Weekly'
GROUP BY name,D.dataset_nme, S.schema_nme, DFC.DatasetID,DFC.SchemaID,S.StorageCode