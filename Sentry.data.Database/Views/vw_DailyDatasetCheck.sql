CREATE VIEW dbo.vw_DailyDatasetCheck as
SELECT
C.Name as Category_NME,
D.Dataset_NME as Dataset_NME,
DatasetID as Dataset_ID,
S.Schema_NME as Schema_NME,
DFC.SchemaID as Schema_ID,
S.StorageCode as Storage_CDE,
MAX(CreateDTM) as LastFileConversion_DTE,
GETDATE() as Current_DTM,
CASE WHEN FORMAT(MAX(CreateDTM),'yyyy-MM-dd') = FORMAT(GETDATE(),'yyyy-MM-dd') THEN  'Y'
ELSE 'N'
END as Success_IND
FROM DatasetFileConverted DFC JOIN Dataset D on d.Dataset_ID = DFC.DatasetID
JOIN [schema] s on dfc.SchemaID = s.Schema_Id
JOIN DatasetCategory DC on D.Dataset_ID = DC.Dataset_ID
JOIN Category C on DC.Category_ID = C.ID
--Eventually replace this section with a join to the DSC metadata for these datasets
WHERE D.Dataset_NME in 
('ActNow',
'ClaimIQ',
'DecisionPoint',
'AmazonTelem',
'AmazonTrace',
'bitbucket',
'c1activitystream',
'change record',
'echo claims',
'epic return mail',
'eventfeed',
'google analytics dairyland',
'hcmu',
'lex_utterance_detected',
'lex_utterance_missed',
'pladmin_cmc_data',
'pladmin_pam_payplanrestrictions',
'pladminrtriscore',
'pl ncrf',
'pl pc hold data',
'PLRateProduct',
'pl vin data',
'print to mail isl file data',
'quartermaster',
'ratify data',
'said',
'seer',
'sentrydocs',
'wfm_adherence_actual_detail',
'wfm_adherence_high_level',
'wfm_adherence_scheduled_detail'
)
AND D.ObjectStatus = 1 /* Only "Active" datasets */
AND D.Display_IND = 1 
AND S.Schema_NME NOT IN ('Bitbucket_BaseActivity_Detail',
'Bitbucket_Changeset_Detail',
'Full Vin')
GROUP BY name,dataset_nme, schema_nme, DatasetID,SchemaID,StorageCode
