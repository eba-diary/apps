CREATE VIEW [dbo].[vw_Schema] AS
select 
--DS.Dataset_ID,
--DS.Dataset_NME,
--DFC.Config_ID,
--DFC.Config_NME, 
--DE.DataElement_ID, 
--DE.DataElement_NME,
--DED_filetype.DataElementDetailType_VAL as 'FileType',
--DED_revision.DataElementDetailType_VAL as 'Revision',
--DED_delimiter.DataElementDetailType_VAL as 'Delimiter',
--DO.DataObject_ID,
--DO.DataObject_NME,
--DOF.DataObjectField_ID,
--DOF.DataObjectField_NME,
--DOFD_Ordinal.DataObjectFieldDetailType_VAL as 'OrdinalPosition',
--DOFD_DataType.DataObjectFieldDetailType_VAL as 'DataType',
--DOFD_length.DataObjectFieldDetailType_VAL as 'Length'
--from DataElement DE
--join DatasetFileConfigs DFC on
--	DE.Config_ID = DFC.Config_ID
--join Dataset DS on
--	DFC.Dataset_ID = DS.Dataset_ID
--left join DataElementDetail DED_filetype on
--	DE.DataElement_ID = DED_filetype.DataElement_ID and
--	DED_filetype.DataElementDetailType_CDE = 'FileFormat_TYP'
--left join DataElementDetail DED_revision on
--	DE.DataElement_ID = DED_revision.DataElement_ID and
--	DED_revision.DataElementDetailType_CDE = 'Revision_CDE'
--left join DataElementDetail DED_delimiter on
--	DE.DataElement_ID = DED_delimiter.DataElement_ID and
--	DED_delimiter.DataElementDetailType_CDE = 'FileDelimiter_TYP'
--left join DataObject DO on
--	DE.DataElement_ID = DO.DataElement_ID
--left join DataObjectField DOF on
--	DO.DataObject_ID = DOF.DataObject_ID
--left join DataObjectFieldDetail DOFD_Ordinal on
--	DOF.DataObjectField_ID = DOFD_Ordinal.DataObjectField_ID and
--	DOFD_Ordinal.DataObjectFieldDetailType_CDE = 'OrdinalPosition_CDE'
--left join DataObjectFieldDetail DOFD_DataType on
--	DOF.DataObjectField_ID = DOFD_DataType.DataObjectField_ID and
--	DOFD_DataType.DataObjectFieldDetailType_CDE = 'Datatype_TYP'
--left join DataObjectFieldDetail DOFD_length on
--	DOF.DataObjectField_ID = DOFD_length.DataObjectField_ID and
--	DOFD_length.DataObjectFieldDetailType_CDE = 'Length_AMT'
null as 'Dataset_ID',
null as 'Dataset_NME',
DFC.Config_ID,
DFC.Config_NME, 
null as 'DataElement_ID', 
null as 'DataElement_NME',
null as 'FileType',
null as 'Revision',
null as 'Delimiter',
null as 'DataObject_ID',
null as 'DataObject_NME',
null as 'DataObjectField_ID',
null as 'DataObjectField_NME',
null as 'OrdinalPosition',
null as 'DataType',
null as 'Length'
from DatasetFileConfigs DFC 