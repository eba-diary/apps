/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
	07.27.2021		ALL		Delete Deprecated DATA ELEMENT TABLES because DACPAC doesn't remove tables that do not exist in DACPAC
--------------------------------------------------------------------------------------
*/

DROP TABLE DataElementDetail
DROP TABLE DataElement
DROP TABLE DataObjectDetail
DROP TABLE DataObjectFieldDetail
DROP TABLE DataObjectField
DROP TABLE DataObject