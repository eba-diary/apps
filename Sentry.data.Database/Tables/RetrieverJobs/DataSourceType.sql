CREATE TABLE [dbo].[DataSourceType]
(
	[Name] VARCHAR(50) NOT NULL PRIMARY KEY, 
    [Description] VARCHAR(250) NOT NULL, 
    [DiscrimatorValue] VARCHAR(50) NOT NULL
)
