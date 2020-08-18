CREATE PROCEDURE dbo.usp_DALE_BaseScanAction_UPDATE
      @sensitiveBlob	NVARCHAR(MAX)			= NULL

/**********************************************************************************
Update History:

Date			Author          Comments
----------		------          --------
07.28.2020		ALL				UPDATE BaseScanAction


DROP PROCEDURE IF EXISTS dbo.usp_DALE_BaseScanAction_UPDATE;
GO
exec usp_DALE_BaseScanAction_UPDATE '[{"BaseColumnId":7627742,"IsSensitive":true,"IsOwnerVerified":true},{"BaseColumnId":7627741,"IsSensitive":true,"IsOwnerVerified":true}]'


select * from ScanAction
select * from BaseScanAction where Base_ID IN (7627742,7627741,29420501)
select * from #SENSITIVE
select top 10 * from Sensitive_v where Base_ID IN (7627742,7627741,29420501)
select * from Column_v where BaseColumn_ID IN (7627742,7627741,29420501)
select * from BASE where Base_ID IN (7627742,7627741,29420501)

***********************************************************************************/
AS
BEGIN	
    SET NOCOUNT ON

	DECLARE @ScanAction_ID_IsSensitive     INT

	--create var for ScanAction_ID because its an IDENTITY and could differ per environment
	SELECT @ScanAction_ID_IsSensitive = ScanAction_ID FROM ScanAction WHERE Alert_NME = 'IsSensitive' AND Source_NME = 'DALE' 


	CREATE TABLE #SENSITIVE
	(	
		BaseColumnId INT
		,IsSensitive BIT
		,IsOwnerVerified BIT
	)

	INSERT INTO #SENSITIVE
	SELECT BaseColumnId, CASE IsSensitive WHEN 'true' THEN 1 ELSE 0 END,CASE IsOwnerVerified WHEN 'true' THEN 1 ELSE 0 END
	FROM OPENJSON( @sensitiveBlob)
	WITH (BaseColumnId int,	IsSensitive nvarchar(max),IsOwnerVerified nvarchar(max))

	MERGE	BaseScanAction		AS BASESCANACTION
	USING	#SENSITIVE			AS SENSITIVE
		ON BASESCANACTION.Base_ID				= SENSITIVE.BaseColumnId
	WHEN MATCHED THEN 
		UPDATE SET	BASESCANACTION.ScanAction_ID = @ScanAction_ID_IsSensitive
					,BASESCANACTION.Sensitive_FLG = SENSITIVE.IsSensitive
					,BASESCANACTION.OwnerVerify_FLG = SENSITIVE.IsOwnerVerified
	WHEN NOT MATCHED THEN
		INSERT (Base_ID,ScanAction_ID,Sensitive_FLG,OwnerVerify_FLG) 
		VALUES (SENSITIVE.BaseColumnId,@ScanAction_ID_IsSensitive,SENSITIVE.IsSensitive,SENSITIVE.IsOwnerVerified);		

	UPDATE BASE 
	SET 
     	BASE.Sensitive_FLG			= ISNULL(SENSITIVE_V.Sensitive_FLG, 0),
     	BASE.UserVerify_FLG			= ISNULL(SENSITIVE_V.UserVerify_FLG, 0),
     	BASE.OwnerVerify_FLG		= ISNULL(SENSITIVE_V.OwnerVerify_FLG, 0)
	FROM [dbo].[Base] BASE
	JOIN #SENSITIVE SENSITIVE 
		ON BASE.Base_ID				= SENSITIVE.BaseColumnId
    LEFT JOIN [dbo].[Sensitive_v] SENSITIVE_V 
		ON BASE.Base_ID				= SENSITIVE_V.Base_ID
    WHERE	BASE.Sensitive_FLG		<> ISNULL(SENSITIVE_V.Sensitive_FLG, 0)
      		OR BASE.UserVerify_FLG	<> ISNULL(SENSITIVE_V.UserVerify_FLG, 0)
      		OR BASE.OwnerVerify_FLG	<> ISNULL(SENSITIVE_V.OwnerVerify_FLG, 0); 

END