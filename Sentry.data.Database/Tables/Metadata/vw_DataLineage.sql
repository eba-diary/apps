CREATE VIEW [dbo].[vw_DataLineage] AS
  SELECT 
  NEWID() AS DataLineage_ID,
  DA.DataAsset_ID,
  DA.Line_CDE,
  DA.Model_NME,
  LL.Element as DataElement_NME,
  LL.[ElementType] as DataElement_TYP,
  LL.[Object] as DataObject_NME,
  LL.[ObjectType] as DataObjectCode_DSC,
  LL.[ObjectField] as DataObjectField_NME,
  LL.[SourceElement] as SourceElement_NME,
  LL.[SourceObject] as SourceObject_NME,
  LL.[SourceField] as SourceField_NME,
  LL.[SourceFieldName] as Source_TXT,
  LL.[TransformationText] as Transformation_TXT,
  LL.[DisplayIndicator] as Display_IND,
  BTD.Description as BusTerm_DSC
  FROM LineageLoad LL LEFT OUTER JOIN DataAsset DA ON LL.Asset = DA.MetadataRepositoryAsset_NME
  LEFT OUTER JOIN BusinessTermDescriptions BTD ON BTD.Element = LL.Element
  AND LL.[ElementType] = 'Business Term'
