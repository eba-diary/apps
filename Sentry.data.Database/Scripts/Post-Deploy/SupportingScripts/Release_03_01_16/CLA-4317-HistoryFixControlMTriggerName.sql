SET @ScriptVersion = 'CLA-4317-HistoryFixControlMTriggerName'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --

    
	--Schema_NME CLEANSE
	;WITH RECUR_Schema_NME
	as 
	(
 		SELECT DISTINCT 
				S.Schema_Id
				,S.Schema_NME AS Schema_NME_ORIG 
				,CAST(S.Schema_NME AS VARCHAR(100)) AS Schema_NME, PATINDEX('%[^A-Z0-9]%', S.Schema_NME) AS BadCharIndex_Schema_NME
		FROM dataset D
		JOIN DatasetFileConfigs DFG
			ON DFG.Dataset_ID = D.Dataset_ID
		JOIN [Schema] S on DFG.Schema_Id = S.Schema_Id
    
		UNION ALL
    
		SELECT	
				Schema_Id
				,Schema_NME_ORIG
				,CAST(Schema_NME AS VARCHAR(100)) AS Schema_NME, PATINDEX('%[^A-Z0-9]%', Schema_NME) AS BadCharIndex
		FROM 
		(
			SELECT 
				Schema_Id
				,Schema_NME_ORIG
				,CASE WHEN BadCharIndex_Schema_NME > 0 
					THEN REPLACE(Schema_NME, SUBSTRING(Schema_NME, BadCharIndex_Schema_NME, 1), '')
					ELSE Schema_NME 
				END AS Schema_NME
			FROM RECUR_Schema_NME
			WHERE BadCharIndex_Schema_NME > 0

		) badCharFinder
	)

	UPDATE S
		SET S.ControlMTriggerName = 'DATA_' + UPPER(D.NamedEnvironment) + '_' + UPPER(D.Short_NME) + '_' + UPPER(R.Schema_NME) + '_COMPLETED'
	--SELECT S.Schema_Id, 'DATA_' + D.NamedEnvironment + '_' + D.Short_NME + '_' + R.Schema_NME + '_COMPLETED'
	FROM dataset D
		JOIN DatasetFileConfigs DFG
			ON DFG.Dataset_ID = D.Dataset_ID
		JOIN [Schema] S on DFG.Schema_Id = S.Schema_Id
		JOIN RECUR_Schema_NME R ON S.Schema_Id = R.Schema_Id
	WHERE BadCharIndex_Schema_NME = 0



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

