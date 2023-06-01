BEGIN TRAN 
	BEGIN TRY 
		
		MERGE INTO Category AS Target 
		USING (VALUES 
									(1, 'Claim',					NULL, 'clcDkBlue',	'DS',	NULL),
									(2, 'Industry',					NULL, 'clcDkBlue',	'DS',	NULL),
									(3, 'Government',				NULL, 'clcDkBlue',	'DS',	NULL),
									(4, 'Geographic',				NULL, 'clcDkBlue',	'DS',	NULL),
									(5, 'Weather',					NULL, 'clcDkBlue',	'DS',	NULL),
									(7, 'Commercial Lines',			NULL, 'blueGray',	'RPT',	'CL'),
									(8, 'Personal Lines',			NULL, 'orange',		'RPT',	'PL'),
									(9, 'Claims',					NULL, 'gold',		'RPT',	NULL),
									(10, 'Corporate',				NULL, 'green',		'RPT',	NULL),
									(11, 'IT',						NULL, 'plum',		'RPT',	NULL),
									(12, 'Life and Annuities',		NULL, 'blue',		'RPT',	'LA'),
									(13, 'Human Resources',			NULL, 'clcDkBlue',	'DS',	'HR'),
									(14, 'Investment',				NULL, 'clcDkBlue',	'DS',	NULL),
									(15, 'Commercial Lines',		NULL, 'clcDkBlue',	'DS',	NULL),
									(16, 'Customer Communication',	NULL, 'clcDkBlue',	'DS',	NULL),
									(17, 'Enterprise',				NULL, 'clcDkBlue',	'DS',	NULL),
									(18, 'Information Technology',	NULL, 'clcDkBlue',	'DS',	NULL),
									(19, 'Personal Lines',			NULL, 'clcDkBlue',	'DS',	NULL),
									(20, 'Web Portals',				NULL, 'clcDkBlue',	'DS',	NULL)
								)
								AS Source ([Id], [Name], ParentCategory, Color, Object_TYP, AbbreviatedName) 

		ON Target.[Id] = Source.[Id]
		WHEN MATCHED THEN 
			-- update matched rows 
			UPDATE SET 
				[Id] = Source.[Id],  
				[Name] = Source.[Name],
				ParentCategory = Source.ParentCategory,
				Color = Source.Color,
				Object_TYP = Source.Object_TYP,
				AbbreviatedName = Source.AbbreviatedName

		WHEN NOT MATCHED BY TARGET THEN 
			-- insert new rows 
			INSERT ([Id], [Name], ParentCategory, Color, Object_TYP, AbbreviatedName) 
			VALUES ([Id], [Name], ParentCategory, Color, Object_TYP, AbbreviatedName)  
					  
		WHEN NOT MATCHED BY SOURCE THEN 
			-- delete rows that are in the target but not the source 
			DELETE;


	END TRY 
	BEGIN CATCH 
		DECLARE @Merge_Category_ErrorMessage NVARCHAR(4000); 
		DECLARE @Merge_Category_ErrorSeverity INT; 
		DECLARE @Merge_Category_ErrorState INT; 
  
		SELECT 
			@Merge_Category_ErrorMessage = ERROR_MESSAGE(), 
			@Merge_Category_ErrorSeverity = ERROR_SEVERITY(), 
			@Merge_Category_ErrorState = ERROR_STATE(); 
  
		RAISERROR (@Merge_Category_ErrorMessage, 
				   @Merge_Category_ErrorSeverity, 
				   @Merge_Category_ErrorState 
				   ); 
  
		ROLLBACK TRAN 
		RETURN
	END CATCH 
  
COMMIT TRAN