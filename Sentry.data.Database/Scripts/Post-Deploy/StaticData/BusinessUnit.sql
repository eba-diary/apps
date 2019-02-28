BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO BusinessUnit AS Target 
		USING (VALUES 
									(1, 'Direct Writer', 'DW', 1),
									(2, 'National Accounts', 'NA', 2),
									(3, 'Regional', 'RG', 3),
									(4, 'Transportation', 'TR', 4),
									(5, 'Hortica', 'HRT', 5),
									(6, 'Life and Health', 'LH', 6),
									(7, 'Parker Stevens Agency', 'PSA', 7),
									(8, 'Motorcycle', 'MC', 8), 
									(9, 'NonStandard Auto', 'NSA', 9)
								)
								AS Source ([BusinessUnit_Id], [Name], [AbbreviatedName], [Sequence]) 

		ON Target.[BusinessUnit_Id] = Source.[BusinessUnit_Id]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[BusinessUnit_Id] = Source.[BusinessUnit_Id],  
				[Name] = Source.[Name],
				[AbbreviatedName] = Source.[AbbreviatedName],
				[Sequence] = Source.[Sequence]

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([BusinessUnit_Id], [Name], [AbbreviatedName], [Sequence]) 
			VALUES ([BusinessUnit_Id], [Name], [AbbreviatedName], [Sequence])  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_BusinessUnit_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_BusinessUnit_ErrorSeverity INT; 
		DECLARE @Merge_BusinessUnit_ErrorState INT; 
  
		SELECT 
			@Merge_BusinessUnit_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_BusinessUnit_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_BusinessUnit_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_BusinessUnit_ErrorMessage, 
				   @Merge_BusinessUnit_ErrorSeverity, 
				   @Merge_BusinessUnit_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN