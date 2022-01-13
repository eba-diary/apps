SET @ScriptVersion = 'CLA-3729-HISTORY-FIX-EVENT-Reason'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --


/**************************************************************************************************************************************
This History fix will update Event.Reason to be more specific so reasons are created once and used on Home Page Feed and SpamFactory
select * from [Event]
***************************************************************************************************************************************/
	UPDATE EVENTME

		SET EVENTME.Reason =	
		
		--SELECT *,
		--SELECT 
		CASE	
						WHEN EVENTTYPE.Description = 'Created Dataset Schema'	THEN	'A new schema called ' + SCHEMAME.Schema_NME	+	' was created under '	+ DATASET.Dataset_NME + ' in ' + CATEGORY.Name
						WHEN EVENTTYPE.Description = 'Created Dataset'			THEN	'A new dataset called ' + DATASET.Dataset_NME	+	' was created in '		+ CATEGORY.Name
						WHEN EVENTTYPE.Description = 'Created Report'			THEN	'A new exhibit called ' + DATASET.Dataset_NME	+	' was Created in '		+ CATEGORY.Name
				END

	FROM  [Event] EVENTME

	JOIN EventType EVENTTYPE
		ON EVENTME.EventType = EVENTTYPE.Type_ID

	JOIN Dataset DATASET
		ON EVENTME.Dataset_ID = DATASET.Dataset_ID

	JOIN
	(
		SELECT Dataset_Id,MAX(Category_Id) AS Category_Id
		FROM DatasetCategory 
		GROUP BY Dataset_Id

	)DATASETCATEGORY_CLEAN 
		ON DATASET.Dataset_ID = DATASETCATEGORY_CLEAN.Dataset_Id

	JOIN Category CATEGORY 
		ON DATASETCATEGORY_CLEAN.Category_Id = CATEGORY.Id

	LEFT JOIN [Schema] SCHEMAME
		ON EVENTME.[Schema_Id] = SCHEMAME.[Schema_Id]

	WHERE	DATEDIFF(DAY,TimeCreated, GETDATE()) <= 30
			AND EVENTTYPE.Description IN ('Created Dataset Schema','Created Dataset','Created Report')
			




    -- END POST-DEPLOY SCRIPT --
    INSERT INTO VERSION (Version_CDE, AppliedOn_DTM) VALUES ( @ScriptVersion, GETDATE() ) 
END TRY 
BEGIN CATCH 
    SELECT 
        @ErrorMessage = ERROR_MESSAGE(), 
        @ErrorSeverity = ERROR_SEVERITY(), 
        @ErrorState = ERROR_STATE(); 
  
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState ); 
  
    ROLLBACK TRAN 
    RETURN
END CATCH 

COMMIT TRAN

