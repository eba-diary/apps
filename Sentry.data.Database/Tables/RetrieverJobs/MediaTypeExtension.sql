CREATE TABLE [dbo].[MediaTypeExtension]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [MediaType] NVARCHAR(MAX) NOT NULL, 
    [FileExtension] NVARCHAR(50) NOT NULL
)
