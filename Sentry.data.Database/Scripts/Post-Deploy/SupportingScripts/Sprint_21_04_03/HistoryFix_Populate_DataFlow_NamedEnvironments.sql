/***********************
*  History fix to fill in the new NamedEnvironment and NamedEnvironmentType columns on the DataFlow table
***********************/

DECLARE @DSCNamedEnvironment VARCHAR(10) = (select CAST(value as VARCHAR(10)) from sys.extended_properties where NAME = 'NamedEnvironment')
DECLARE @NamedEnvironment VARCHAR(10)
DECLARE @NamedEnvironmentType VARCHAR(10)

if (@DSCNamedEnvironment = 'DEV')
BEGIN
	SET @NamedEnvironment = 'DEV'
	SET @NamedEnvironmentType = 'NonProd'
END
else if (@DSCNamedEnvironment = 'NRDEV')
BEGIN
	SET @NamedEnvironment = 'NRDEV'
	SET @NamedEnvironmentType = 'NonProd'
END
else if (@DSCNamedEnvironment = 'TEST')
BEGIN
	SET @NamedEnvironment = 'TEST'
	SET @NamedEnvironmentType = 'NonProd'
END
else if (@DSCNamedEnvironment = 'NRTEST')
BEGIN
	SET @NamedEnvironment = 'NRTEST'
	SET @NamedEnvironmentType = 'NonProd'
END
else if (@DSCNamedEnvironment = 'QUAL')
BEGIN
	SET @NamedEnvironment = 'QUAL'
	SET @NamedEnvironmentType = 'NonProd'
END
else if (@DSCNamedEnvironment = 'PROD')
BEGIN
	SET @NamedEnvironment = 'PROD'
	SET @NamedEnvironmentType = 'Prod'
END

SELECT 'Updating NamedEnvironment to ' + @NamedEnvironment + ' and NamedEnvironmentType to ' + @NamedEnvironmentType

UPDATE DataFlow
SET NamedEnvironment = @NamedEnvironment,
    NamedEnvironmentType = @NamedEnvironmentType