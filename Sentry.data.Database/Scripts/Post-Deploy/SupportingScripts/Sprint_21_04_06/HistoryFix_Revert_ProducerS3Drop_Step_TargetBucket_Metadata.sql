UPDATE DataFlowStep 
SET TargetBucket = null
WHERE DataAction_Type_Id = 12 /* ProducerS3Drop action type */