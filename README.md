# SimpleService
This is a simple samle of BulkInsert of data.
It's the service use with the DataMigration Tool
https://github.com/skak2000/DataMigration

#Processing Logic
Prepare Data Table Layout
Uses BulkTool.GetDataTableLayout() to obtain a table schema.
Add the data to the datatable

- Select what is the keyColumns 
- Select what Columns that can be updated
- Select what Columns to return
  
Bulk Insert/Update
Run the BulkInsertUpdateAsync

Map the respont from the InsertUpdate to DTO and return the list. 

!!!
TenantId and InstanceId shut be extracted from a token and not as parameter.
