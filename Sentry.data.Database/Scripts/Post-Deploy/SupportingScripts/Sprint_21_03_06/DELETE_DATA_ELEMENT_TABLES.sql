/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
	07.27.2021		ALL		Delete Deprecated DATA ELEMENT TABLES because DACPAC doesn't remove tables that do not exist in DACPAC
	08.02.221		ALL		Add if exists like i should have done initially so can re-run in environments where they don't exist and no errors would happen
--------------------------------------------------------------------------------------
*/

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE	TABLE_NAME = 'DataElementDetail'))
BEGIN
    DROP TABLE DataElementDetail
END

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE	TABLE_NAME = 'DataElement'))
BEGIN
	DROP TABLE DataElement
END

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE	TABLE_NAME = 'DataObjectDetail'))
BEGIN
	DROP TABLE DataObjectDetail
END

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE	TABLE_NAME = 'DataObjectFieldDetail'))
BEGIN
	DROP TABLE DataObjectFieldDetail
END

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE	TABLE_NAME = 'DataObjectField'))
BEGIN
	DROP TABLE DataObjectField
END

IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES  WHERE	TABLE_NAME = 'DataObject'))
BEGIN
	DROP TABLE DataObject
END