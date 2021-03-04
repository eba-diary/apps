ALTER PROCEDURE [dbo].[usp_DALE_BaseScanAction_UPDATE]
      @sensitiveBlob	NVARCHAR(MAX)			= NULL

/**********************************************************************************
Update History:

Date			Author          Comments
----------		------          --------
07.28.2020		ALL				UPDATE BaseScanAction
10.27.2020		ALL				Fix too update all Base_IDs when a ScanActionUpdate Dale Row is updated/inserted
03.04.2021		Dennis			Add extra update at end


DROP PROCEDURE IF EXISTS dbo.usp_DALE_BaseScanAction_UPDATE;
GO
exec usp_DALE_BaseScanAction_UPDATE '[{"BaseColumnId":4222300,"IsSensitive":false,"IsOwnerVerified":false}]'


select * from ScanAction
select * from BaseScanAction where Base_ID IN (4222300)
select * from #SENSITIVE
select top 10 * from Sensitive_v where Base_ID IN (4222300)
select * from Column_v where BaseColumn_ID IN (4222300)
select * from BaseColumn_v where Column_ID IN (4222300)
select * from BASE where Base_ID IN (4222300)

select * from column_v where column_nme = 'DriverDOB' 

***********************************************************************************/
AS
BEGIN	
    SET NOCOUNT ON

	--CREATE ScanAction_ID because its an IDENTITY and could differ per environment
	DECLARE @ScanAction_ID_IsSensitive     INT
	SELECT @ScanAction_ID_IsSensitive = ScanAction_ID FROM ScanAction WHERE Alert_NME = 'IsSensitive' AND Source_NME = 'DALE' 

	
	--CREATE #SENSITIVE too HOLD ALL ROWS FOR INSERT/UPDATE
	CREATE TABLE #SENSITIVE
	(	
		BaseColumnId INT
		,ScanAction_ID INT
		,IsSensitive BIT
		,IsOwnerVerified BIT
	)

	INSERT INTO #SENSITIVE (BaseColumnId, ScanAction_ID, IsSensitive, IsOwnerVerified)
	SELECT BaseColumnId, @ScanAction_ID_IsSensitive, CASE IsSensitive WHEN 'true' THEN 1 ELSE 0 END,CASE IsOwnerVerified WHEN 'true' THEN 1 ELSE 0 END
	FROM OPENJSON(@sensitiveBlob)
	WITH (BaseColumnId int,	IsSensitive nvarchar(max),IsOwnerVerified nvarchar(max))

	--INSERT/UPDATE #SENSITIVE:  Update SENSITIVE ROWS FOR DALE ScanAction_ID ONLY
	MERGE	BaseScanAction		AS BASESCANACTION
	USING	#SENSITIVE			AS SENSITIVE
		ON	BASESCANACTION.Base_ID				= SENSITIVE.BaseColumnId
			AND	BASESCANACTION.ScanAction_ID	= SENSITIVE.ScanAction_ID
	
	WHEN MATCHED THEN 
		UPDATE SET	BASESCANACTION.ScanAction_ID = SENSITIVE.ScanAction_ID
					,BASESCANACTION.Sensitive_FLG = SENSITIVE.IsSensitive
					,BASESCANACTION.OwnerVerify_FLG = SENSITIVE.IsOwnerVerified
	WHEN NOT MATCHED THEN
		INSERT (Base_ID,ScanAction_ID,Sensitive_FLG,OwnerVerify_FLG) 
		VALUES (SENSITIVE.BaseColumnId,SENSITIVE.ScanAction_ID,SENSITIVE.IsSensitive,SENSITIVE.IsOwnerVerified);		


	--UPDATE BaseScanAction:  update ALL OTHER ScanAction_ID rows because User updates trump all automated
	UPDATE BASESCANACTION
	SET	BASESCANACTION.Sensitive_FLG	= SENSITIVE.IsSensitive
		,BASESCANACTION.OwnerVerify_FLG = SENSITIVE.IsOwnerVerified
	FROM BaseScanAction BASESCANACTION
	JOIN #SENSITIVE			AS SENSITIVE
		ON	BASESCANACTION.Base_ID				= SENSITIVE.BaseColumnId
			

	--UPDATE BASE
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

  -- Don't forget to refresh the BaseTag values for each column
  declare @ColumnBase_ID int;
  select @ColumnBase_ID=min(BaseColumnId)
  from #SENSITIVE;
  while @ColumnBase_ID is not null
  begin
    exec [dbo].[ReloadBaseTagScans_usp] @ColumnBase_ID=@ColumnBase_ID;
    select @ColumnBase_ID=min(BaseColumnId)
    from #SENSITIVE
    where BaseColumnId>@ColumnBase_ID
  end;

END
GO