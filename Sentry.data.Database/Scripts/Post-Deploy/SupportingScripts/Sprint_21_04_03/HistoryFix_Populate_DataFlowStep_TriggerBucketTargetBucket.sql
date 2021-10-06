/***********************
*  History fix to fill in the new TriggerBucket and TargetBucket columns on the DataFlowStep table
***********************/
SELECT 'Updating TriggerBucket column on DataFlowStep...'

UPDATE dfs
SET dfs.TriggerBucket=da.TargetStorageBucket
FROM DataFlowStep dfs
JOIN DataAction da on dfs.Action_Id = da.Id

SELECT 'Updating TargetBucket column on DataFlowStep...'

UPDATE dfs
SET dfs.TargetBucket=da.TargetStorageBucket
FROM DataFlowStep dfs
JOIN DataAction da on dfs.Action_Id = da.Id
WHERE dfs.DataAction_Type_Id in (2,3,5) --only for RawStorage, QueryStorage, and CovertParquet steps