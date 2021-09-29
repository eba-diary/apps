UPDATE DataFlow
SET DatasetId = 0
WHERE DatasetId IS NULL

UPDATE DataFlow
SET SchemaId = 0
WHERE SchemaId IS NULL