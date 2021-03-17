CREATE TABLE [dbo].[FeatureEntity]
(
    [Key] VARCHAR(50) NOT NULL,
    [Value] VARCHAR(MAX) NOT NULL,
    [Name] VARCHAR(50) NULL,
    [Description] VARCHAR(MAX) NULL,
    CONSTRAINT [PK_FeatureEntity] PRIMARY KEY CLUSTERED ([Key])
)
