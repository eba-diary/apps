--Insert all DataActions into the DataAction table. 

Insert into DataAction select NEWID(), 'S3 Drop', 'temp-file/s3drop/', 'sentry-dataset-management-np-nr', 'S3Drop'
Insert into DataAction select NEWID(), 'Raw Storage', 'data/', 'sentry-dataset-management-np-nr', 'RawStorage'
Insert into DataAction select NEWID(), 'Query Storage', 'querystorage/', 'sentry-dataset-management-np-nr', 'QueryStorage'
Insert into DataAction select NEWID(), 'Schema Load', 'temp-file/schemaload/', 'sentry-dataset-management-np-nr', 'SchemaLoad'