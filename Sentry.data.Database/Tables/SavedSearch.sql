CREATE TABLE [dbo].[SavedSearch]
(
	[SavedSearchId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SearchName] VARCHAR(50) NOT NULL, 
    [SearchText] VARCHAR(4096) NULL, 
    [FilterCategoriesJson] NVARCHAR(MAX) NULL
)
