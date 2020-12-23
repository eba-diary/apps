/****************************************************
   Assign guid to all retriever jobs which have a 
     Null value
****************************************************/
update RetrieverJob
set Job_Guid = NEWID()
where Job_Guid IS NULL