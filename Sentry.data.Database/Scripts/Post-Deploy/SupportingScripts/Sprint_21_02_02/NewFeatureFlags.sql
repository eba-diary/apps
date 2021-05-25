IF (SELECT count(*) FROM FeatureEntity where [KeyCol] = 'CLA2671_RefactorEventsToJava') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_RefactorEventsToJava', '', null, 'White list for JAVA and Black List for .Net of which events have been refactor'
END

IF (SELECT count(*) FROM FeatureEntity where [KeyCol] = 'CLA2671_RefactoredDataFlows') = 0 
BEGIN
Insert into FeatureEntity select 'CLA2671_RefactoredDataFlows', '', null, 'Dataflows used for dark lauch testing of refactored events end to end'
END