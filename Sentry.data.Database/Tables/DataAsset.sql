CREATE TABLE [dbo].[DataAsset]
(
	[DataAsset_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DataAsset_NME] VARCHAR(50) NOT NULL, 
    [Display_NME] VARCHAR(50) NOT NULL, 
    [ArchDiagram_URL] VARCHAR(1024) NULL, 
    [DataModel_URL] VARCHAR(1024) NULL, 
    [Guide_URL] VARCHAR(1024) NULL, 
    [Contact_EML] VARCHAR(128) NULL, 
    [DataAsset_DSC] VARCHAR(MAX) NULL, 
    [MetadataRepositoryAsset_NME] VARCHAR(50) NULL,
	[Line_CDE] [varchar](5) NULL,
	[Model_NME] [varchar](50) NULL
)
