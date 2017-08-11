CREATE TABLE [dbo].[ComponentElement]
(
	[Element_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Element_NME] VARCHAR(50) NOT NULL, 
    [Link_URL] VARCHAR(1024) NULL, 
    [Parent_ID] INT NULL, 
    [CLC_ID] INT NULL, 
    CONSTRAINT [FK_ComponentElement_ConsumptionLayerComponent] FOREIGN KEY ([CLC_ID]) REFERENCES [ConsumptionLayerComponent]([CLC_ID]), 
    CONSTRAINT [FK_ComponentElement_ComponentElement] FOREIGN KEY ([Parent_ID]) REFERENCES [ComponentElement]([Element_ID])
)
