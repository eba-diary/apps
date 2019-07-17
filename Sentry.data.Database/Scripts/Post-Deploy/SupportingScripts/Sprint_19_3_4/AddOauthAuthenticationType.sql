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
IF NOT EXISTS (Select * from AuthenticationType where [AuthType_CDE] = 'OAuth')
BEGIN
	insert into AuthenticationType
	SELECT
		4,
		'OAuth',
		'OAuth 2.0 Authentication',
		'Utilizes OAuth flow to retrieve accesstoken to pull data from source'
END
ELSE
BEGIN
	UPDATE AuthenticationType
	SET [Display_NME] = 'OAuth 2.0 Authentication', [Description] = 'OAuth 2.0 Authentication'
	WHERE [AuthType_CDE] = 'OAuth'
END