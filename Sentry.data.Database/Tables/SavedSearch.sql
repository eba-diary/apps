CREATE TABLE [dbo].[SavedSearch]
(
	[SavedSearchId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SearchType] VARCHAR(50) NOT NULL,
    [SearchName] VARCHAR(250) NOT NULL, 
    [SearchText] VARCHAR(4096) NULL, 
    [FilterCategoriesJson] NVARCHAR(MAX) NULL, 
    [AssociateId] CHAR(6) NOT NULL, 
    [ResultConfigurationJson] NVARCHAR(MAX) NULL
)
