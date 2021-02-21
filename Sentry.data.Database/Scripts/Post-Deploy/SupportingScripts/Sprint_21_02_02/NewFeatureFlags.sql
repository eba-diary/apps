IF (SELECT count(*) FROM FeatureEntity where [Key] = 'CLA2671_DFSEVENTEventHandler') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_DFSEVENTEventHandler', 'false', null, 'Refatoring GOLDENEYE processing to DSCEVENTSERVICE: false=GOLDENEYE, true=DSCEVENTSERVICE'
END

IF (SELECT count(*) FROM FeatureEntity where [Key] = 'CLA2671_S3DropEventHandler') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_S3DropEventHandler', 'false', null, 'Refatoring GOLDENEYE processing to DSCEVENTSERVICE: false=GOLDENEYE, true=DSCEVENTSERVICE'
END

IF (SELECT count(*) FROM FeatureEntity where [Key] = 'CLA2671_RawStorageEventHandler') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_RawStorageEventHandler', 'false', null, 'Refatoring GOLDENEYE processing to DSCEVENTSERVICE: false=GOLDENEYE, true=DSCEVENTSERVICE'
END

IF (SELECT count(*) FROM FeatureEntity where [Key] = 'CLA2671_QueryStorageEventHandler') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_QueryStorageEventHandler', 'false', null, 'Refatoring GOLDENEYE processing to DSCEVENTSERVICE: false=GOLDENEYE, true=DSCEVENTSERVICE'
END

IF (SELECT count(*) FROM FeatureEntity where [Key] = 'CLA2671_SchemaMapEventHandler') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_SchemaMapEventHandler', 'false', null, 'Refatoring GOLDENEYE processing to DSCEVENTSERVICE: false=GOLDENEYE, true=DSCEVENTSERVICE'
END

IF (SELECT count(*) FROM FeatureEntity where [Key] = 'CLA2671_SchemaLoadEventHandler') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_SchemaLoadEventHandler', 'false', null, 'Refatoring GOLDENEYE processing to DSCEVENTSERVICE: false=GOLDENEYE, true=DSCEVENTSERVICE'
END