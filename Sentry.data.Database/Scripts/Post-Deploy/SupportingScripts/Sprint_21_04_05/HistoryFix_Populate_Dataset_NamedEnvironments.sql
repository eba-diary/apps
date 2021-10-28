/***********************
*  History fix to fill in the new NamedEnvironment and NamedEnvironmentType columns on the DataFlow table
***********************/

DECLARE @DSCNamedEnvironment2 VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
DECLARE @NamedEnvironment2 VARCHAR(10)
DECLARE @NamedEnvironment2Type VARCHAR(10)

if (@DSCNamedEnvironment2 = 'DEV')
BEGIN
	SET @NamedEnvironment2 = 'DEV'
	SET @NamedEnvironment2Type = 'NonProd'
END
else if (@DSCNamedEnvironment2 = 'NRDEV')
BEGIN
	SET @NamedEnvironment2 = 'NRDEV'
	SET @NamedEnvironment2Type = 'NonProd'
END
else if (@DSCNamedEnvironment2 = 'TEST')
BEGIN
	SET @NamedEnvironment2 = 'TEST'
	SET @NamedEnvironment2Type = 'NonProd'
END
else if (@DSCNamedEnvironment2 = 'NRTEST')
BEGIN
	SET @NamedEnvironment2 = 'NRTEST'
	SET @NamedEnvironment2Type = 'NonProd'
END
else if (@DSCNamedEnvironment2 = 'QUAL')
BEGIN
	SET @NamedEnvironment2 = 'QUAL'
	SET @NamedEnvironment2Type = 'NonProd'
END
else if (@DSCNamedEnvironment2 = 'PROD')
BEGIN
	SET @NamedEnvironment2 = 'PROD'
	SET @NamedEnvironment2Type = 'Prod'
END
else
BEGIN
	SET @NamedEnvironment2 = 'DEV'
	SET @NamedEnvironment2Type = 'NonProd'
END

PRINT 'Updating NamedEnvironment to ' + @NamedEnvironment2 + ' and NamedEnvironmentType to ' + @NamedEnvironment2Type

UPDATE Dataset
SET NamedEnvironment = @NamedEnvironment2,
    NamedEnvironmentType = @NamedEnvironment2Type
WHERE NamedEnvironment IS NULL