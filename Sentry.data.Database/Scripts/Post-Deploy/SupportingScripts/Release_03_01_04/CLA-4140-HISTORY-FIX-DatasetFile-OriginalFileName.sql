SET @ScriptVersion = 'CLA-4140-HISTORY-FIX-DatasetFile-OriginalFileName'

BEGIN TRAN 
IF NOT EXISTS (SELECT * FROM [Version] where Version_CDE=@ScriptVersion) 
BEGIN TRY 
    IF @ScriptVersion LIKE '***%' THROW 100001, 'Error running post-deploy script: the ScriptVersion was still set to the default value!', 1
    PRINT 'Running script "' + @ScriptVersion + '"...'
    -- BEGIN POST-DEPLOY SCRIPT --



	UPDATE DF
	SET 
		DF.OriginalFileName = 
		--SELECT DF.File_NME, A.SaidKey_CDE,
				CASE	
						--tested
						WHEN RIGHT(File_NME,9) = '.txt.json'
						THEN LEFT(File_NME,LEN(File_NME)-27) + '.txt'

						--tested
						WHEN RIGHT(File_NME,9) = '.zip.json'
						THEN LEFT(File_NME,LEN(File_NME)-27) + '.zip'

						--tested
						WHEN RIGHT(File_NME,9) = '.dat.json'
						THEN LEFT(File_NME,LEN(File_NME)-27) + '.dat'

						--tested
						WHEN RIGHT(File_NME,10) = '.json.json'
						THEN LEFT(File_NME,LEN(File_NME)-28) + '.json'

						--tested
						WHEN RIGHT(File_NME,9) = '.xml.json'
						THEN LEFT(File_NME,LEN(File_NME)-27) + '.xml'
		
						--tested
						WHEN RIGHT(File_NME,9) = '.json.trg'
						THEN LEFT(File_NME,LEN(File_NME)-27) + RIGHT(File_NME,9)

						--tested
						WHEN RIGHT(File_NME,8) = '.json.gz'
						THEN NULL

						--tested
						WHEN RIGHT(File_NME,11) IN ('.ndjson.trg')
						THEN LEFT(File_NME,LEN(File_NME)-29) + RIGHT(File_NME,11)

						--tested
						WHEN RIGHT(File_NME,8) = '.csv.trg'
						THEN LEFT(File_NME,LEN(File_NME)-26) + RIGHT(File_NME,8)

						--tested
						WHEN RIGHT(File_NME,5) IN ('.json')
						THEN LEFT(File_NME,LEN(File_NME)-23) + RIGHT(File_NME,5)

						--Tested
						WHEN RIGHT(File_NME,5) IN ('.xlsx')
						THEN NULL

						--tested
						WHEN RIGHT(File_NME,4) IN ('.txt','.xml','.zip','.csv','.xls','.rtf','.pdf','.bin','.dat') 
						THEN LEFT(File_NME,LEN(File_NME) - CASE WHEN LEN(File_NME) >= 22 THEN 22 ELSE 4 END) + RIGHT(File_NME,4)

						--tested
						WHEN RIGHT(File_NME,7) IN ('.ndjson')
						THEN LEFT(File_NME,LEN(File_NME)-25) + RIGHT(File_NME,7)

						--tested
						WHEN RIGHT(File_NME,3) IN ('.pm')
						THEN File_NME

						--tested
						WHEN RIGHT(File_NME,4) IN ('.prn')
						THEN File_NME
			
						--tested
						WHEN RIGHT(File_NME,9) IN ('.sas7bdat')
						THEN File_NME

						ELSE NULL
				END		

FROM DatasetFile DF
LEFT JOIN Dataset D ON DF.Dataset_ID = D.Dataset_ID
LEFT JOIN Asset A ON A.Asset_ID = D.Asset_ID
WHERE LEFT(DF.FileLocation,13) = 'rawquery/' + A.SaidKey_CDE		--V3 Criteria which includes CORVUS
		AND DF.OriginalFileName IS NULL							--allow for repeatability 
		--AND DF.File_NME LIKE '%.json.gz%'
		
		






    





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

