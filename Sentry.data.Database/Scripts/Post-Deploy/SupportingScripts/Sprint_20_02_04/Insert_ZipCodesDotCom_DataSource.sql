/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
IF NOT EXISTS (Select * from DataSource where [Source_NME] = 'ZipCodesDotCom')
BEGIN	
	INSERT INTO DataSource
	(
		Source_NME,
		Source_DSC,
		BaseUri,
		IsUriEditable_IND,
		SourceType_IND,
		SourceAuth_ID,
		KeyCode_CDE,
		Created_DTM,
		Modified_DTM,
		Bucket_NME,
		PortNumber,
		HostFingerPrintKey,
		IsUserPassRequired,
		AuthHeaderName,
		AuthHeaderValue,
		IVKey,
		RequestHeaders,
		Options,
		CurrentToken,
		CurrentTokenExp,
		ClientID,
		ClientPrivateID,
		Scope,
		TokenUrl,
		TokenExp,
		PrimaryContact_ID,
		PrimaryOwner_ID,
		IsSecured_IND,
		Security_ID
	)
	VALUES
	(
	'ZipCodesDotCom',
	null,
	'ftp://ftp.zip-codes.com/',
	1,
	'FTP',
	(select Auth_ID from AuthenticationType where AuthType_CDE = 'BasicAuth'),
	'd55dc06d-7669',
	GETDATE(),
	GETDATE(),
	null,
	21,
	null,
	0,
	null,
	null,
	null,
	null,
	null,
	null,
	null,
	null,
	null,
	null,
	null,
	null,
	'072984',
	'072984',
	0,
	null
	);
END
ELSE
BEGIN
	UPDATE DataSource
	SET Source_DSC = null,
		BaseUri = 'ftp://ftp.zip-codes.com/',
		IsUriEditable_IND = 1,
		SourceType_IND =  'FTP',
		SourceAuth_ID =  (select Auth_ID from AuthenticationType where AuthType_CDE = 'BasicAuth'),
		KeyCode_CDE = 'd55dc06d-7669',
		Modified_DTM =  GETDATE(),
		PortNumber = 21,
		IsUserPassRequired = 0,
		PrimaryContact_ID = '072984',
		PrimaryOwner_ID = '072984',
		IsSecured_IND = 0,
		Security_ID = null
	WHERE [Source_NME] = 'ZipCodesDotCom'
END