using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace SimpleService.Tools
{
    public class BulkTool
    {
        private string ConnectionString = string.Empty;

        public BulkTool(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection");
        }


        /// <summary>
        /// Insert all data, update only what match updateColumns
        /// </summary>
        /// <param name="tableName">Table to Insert/Update data</param>
        /// <param name="dataInput">Data to be inserted</param>
        /// <param name="keyColumns">They keys to identify uniq rows</param>
        /// <param name="updateColumns">Columns that need to be updated</param>
        public async Task<DataTable> BulkInsertUpdateAsync(string tableName, DataTable dataInput, string[] keyColumns, string[] updateColumns, string[] getColumns, bool returnData)
        {
            Stopwatch stopwatch = new Stopwatch();
            // Protect the database against SQL Injections
            ValidateName(tableName);
            ValidateColumnNames(keyColumns);
            ValidateColumnNames(updateColumns);
            ValidateColumnNames(getColumns);
            DataTable returnTable = new DataTable();

            string tableId = Guid.NewGuid().ToString("N");
            string stagingTableName = $"#staging_{tableId}_{tableName}";

            // We don't trust input datatable, we get the schema from the database
            DataTable schemaTable = GetDataTableLayout(tableName);
            string sqlTempTable = PrepareTempTable(schemaTable, stagingTableName);

            // Get all colums colums that match with the table
            string[] allColumns = schemaTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {                    
                    // Create temp table
                    using (SqlCommand cmd = new SqlCommand(sqlTempTable, connection, transaction))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Bulk insert into temp table
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        bulkCopy.DestinationTableName = stagingTableName;
                        bulkCopy.WriteToServer(dataInput);
                    }

                    Console.WriteLine("BulkInsert: " + stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();

                    //Insert / Update data set
                    string updateQuery = CreateInsertUpdateQuery(tableName, stagingTableName, allColumns, keyColumns, updateColumns);

                    using (SqlCommand command = new SqlCommand(updateQuery, connection, transaction))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine("CreateInsertUpdateQuery: " + stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
                    await transaction.CommitAsync();
                }

                // Return dataset?
                if (returnData)
                {
                    // Now we return inserted data and the public id's
                    // we don't update columns from the where clause
                    //string getColumnsSafe = string.Join(", ", getColumns.Select(c => $"[{c}]"));
                    string getColumnsSafe = string.Join(", ", getColumns.Select(c => $"dest.[{c}]"));
                    string[] keyColumnsSafe = keyColumns.Intersect(allColumns).ToArray();
                    string[] nonKeyColumns = allColumns.Except(keyColumnsSafe).ToArray();
                    string updateSetClause = string.Join(", ", nonKeyColumns.Select(c => $"staging.[{c}] = dest.[{c}]"));
                    string joinClause = string.Join(" AND ", keyColumnsSafe.Select(c => $"staging.[{c}] = dest.[{c}]"));

                    string query = $@"SELECT {getColumnsSafe} FROM [{tableName}] AS dest INNER JOIN [{stagingTableName}] AS staging ON {joinClause};";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(returnTable);
                    }

                    Console.WriteLine("Get data back: " + stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
                }
            }
            return returnTable;
        }

        private string CreateInsertUpdateQuery(string tableName, string stagingTableName, string[] allColumns, string[] keyColumns, string[] updateColumns)
        {
            // We cannot update Id and PublicId
            updateColumns = RemoveProtectedColumns(updateColumns, ["Id", "PublicId"]);                    

            // We can insert PublicId, but we cannot insert Id
            string[] protectedAllColumns = RemoveProtectedColumns(allColumns, ["Id"]);

            // Only correct column names is allowed in the query, remove columns that is not the database table.
            string[] keyColumnsSafe = keyColumns.Intersect(allColumns).ToArray();
            string[] updateColumnsSafe = updateColumns.Intersect(allColumns).ToArray();

            string onClause = string.Join(" AND ", keyColumnsSafe.Select(c => $"target.[{c}] = source.[{c}]"));
            string updateClause = string.Join(", ", updateColumnsSafe.Select(c => $"target.[{c}] = source.[{c}]"));
            string insertColumns = string.Join(", ", protectedAllColumns);
            string insertValues = string.Join(", ", protectedAllColumns.Select(c => $"source.[{c}]"));

            string mergeQuery = $@"MERGE INTO {tableName} AS target
                                    USING {stagingTableName} AS source
                                    ON {onClause}
                                    WHEN MATCHED THEN
                                        UPDATE SET {updateClause}
                                    WHEN NOT MATCHED BY TARGET THEN
                                        INSERT ({insertColumns})
                                        VALUES ({insertValues});";

            return mergeQuery;
        }

        private string CreateUpdateQuery(string tableName, string stagingTableName, string[] allColumns, string[] keyColumns, string[] updateColumns)
        {
            // We cannot update Id and PublicId
            updateColumns = RemoveProtectedColumns(updateColumns, ["Id", "PublicId"]);

            // Only correct column names allowed
            string[] keyColumnsSafe = keyColumns.Intersect(allColumns).ToArray();
            string[] updateColumnsSafe = updateColumns.Intersect(allColumns).ToArray();

            if (!keyColumnsSafe.Any() || !updateColumnsSafe.Any())
                return string.Empty;

            string setClause = string.Join(", ", updateColumnsSafe.Select(c => $"T.[{c}] = S.[{c}]"));
            string joinClause = string.Join(" AND ", keyColumnsSafe.Select(c => $"T.[{c}] = S.[{c}]"));

            string updateQuery = $@"UPDATE T 
                                    SET {setClause}
                                    FROM {tableName} AS T
                                    INNER JOIN {stagingTableName} AS S ON {joinClause};";

            return updateQuery;
        }

        private string CreateInsertQuery(string tableName, string stagingTableName, string[] allColumns, string[] keyColumns)
        {
            // We can insert PublicId, but not Id
            string[] insertableColumns = RemoveProtectedColumns(allColumns, ["Id"]);

            // Only valid key columns
            string[] keyColumnsSafe = keyColumns.Intersect(allColumns).ToArray();

            if (!keyColumnsSafe.Any() || !insertableColumns.Any())
                return string.Empty;

            string insertColumns = string.Join(", ", insertableColumns.Select(c => $"[{c}]"));
            string selectColumns = string.Join(", ", insertableColumns.Select(c => $"S.[{c}]"));
            string whereNotExistsClause = string.Join(" AND ", keyColumnsSafe.Select(c => $"T.[{c}] = S.[{c}]"));

            string insertQuery = $@"
                        INSERT INTO {tableName} ({insertColumns})
                        SELECT {selectColumns}
                        FROM {stagingTableName} AS S
                        WHERE NOT EXISTS (
                            SELECT 1 FROM {tableName} AS T
                            WHERE {whereNotExistsClause}
                        );";

            return insertQuery;
        }


        private string PrepareTempTable(DataTable schemaTable, string stagingTable)
        {
            List<string> columnDefinitions = new List<string>();

            foreach (DataColumn column in schemaTable.Columns)
            {
                string sqlType = GetSqlTypeFromDataColumn(column);
                string nullability = column.AllowDBNull ? "NULL" : "NOT NULL";
                string definition = $"{column.ColumnName} {sqlType} {nullability}";

                if (column.Unique && schemaTable.PrimaryKey.Contains(column))
                {
                    definition += " PRIMARY KEY";
                }
                columnDefinitions.Add(definition);
            }

            string sql = $"CREATE TABLE {stagingTable} ({string.Join(", ", columnDefinitions)});";
            return sql;
        }

        public DataTable GetDataTableLayout(string tableName)
        {
            ValidateName(tableName);

            DataTable table = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                // Select * is not a good thing, but in this cases is is very usefull to make the code dynamic/reusable 
                // We get the tabel layout for our DataTable
                string query = $"SELECT TOP 0 * FROM {tableName};";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                }
            }
            return table;
        }

        public List<KeyValue> GetMappings(List<Guid> publicIds, string tablename, Guid tenantId, Guid instanceId)
        {
            // Protect agains injections
            ValidateName(tablename);         
            
            List<KeyValue> res = new List<KeyValue>();
            publicIds = publicIds.Distinct().ToList();
            string guidTemp = Guid.NewGuid().ToString();
            string tempTable = $"#TempGuids_{guidTemp}";

            DataTable table = new DataTable();
            table.Columns.Add("Id", typeof(Guid));

            foreach (var id in publicIds)
            {
                table.Rows.Add(id);
            }

            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            // Create temp table
            using (var cmd = new SqlCommand($"CREATE TABLE [{tempTable}] (Id UNIQUEIDENTIFIER PRIMARY KEY);", connection))
            {
                cmd.ExecuteNonQuery();
            }

            using (var bulk = new SqlBulkCopy(connection))
            {
                bulk.DestinationTableName = tempTable;
                bulk.WriteToServer(table);
            }

            // Get mappings
            string query = $"SELECT dt.Id, dt.PublicId FROM [{tablename}] dt INNER JOIN [{tempTable}] temp ON dt.PublicId = temp.Id where dt.TenantId = '{tenantId}' AND dt.InstanceId = '{instanceId}';";

            using (var cmd = new SqlCommand(query, connection))
            {
                DataTable dataset = new DataTable();
                using var reader = cmd.ExecuteReader();

                dataset.Load(reader);
                res = MapTable(dataset);
            }
            return res;
        }

        private static List<KeyValue> MapTable(DataTable table)
        {
            List<KeyValue> res = new List<KeyValue>();

            foreach (DataRow row in table.Rows)
            {
                int id = row.Field<int>("Id");
                Guid publicId = row.Field<Guid>("PublicId");

                KeyValue map = new KeyValue(id, publicId);
                res.Add(map);
            }
            return res;
        }

        private static string GetSqlTypeFromDataColumn(DataColumn column)
        {
            Type dataType = column.DataType;

            if (dataType == typeof(string))
            {
                int maxLength = column.MaxLength;
                return maxLength > 0 ? $"NVARCHAR({maxLength})" : "NVARCHAR(MAX)";
            }

            if (dataType == typeof(int)) return "INT";
            if (dataType == typeof(Guid)) return "UNIQUEIDENTIFIER";
            if (dataType == typeof(DateTime)) return "DATETIME";
            if (dataType == typeof(bool)) return "BIT";
            if (dataType == typeof(decimal)) return "DECIMAL(18, 2)";
            if (dataType == typeof(double)) return "FLOAT";
            if (dataType == typeof(byte[])) return "VARBINARY(MAX)";

            throw new NotSupportedException($"Unsupported data type: {dataType.Name}");
        }

        /// <summary>
        /// Remove Columns that cannot be updated
        /// </summary>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        private static string[] RemoveProtectedColumns(string[] updateColumns, string[] protectedColumns)
        {
            List<string> columns = updateColumns.ToList();
            foreach (string item in protectedColumns)
            {
                columns.Remove(item);
            }
            return columns.ToArray();
        }

        private static bool IsSafeName(string name)
        {
            return name.All(c => char.IsLetterOrDigit(c) || c == '_') && !string.IsNullOrWhiteSpace(name);
        }

        private static void ValidateName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName) || !IsSafeName(tableName))
            {
                throw new ArgumentException("Invalid table name.");
            }
        }

        private static void ValidateColumnNames(string[] columns)
        {
            foreach (var column in columns)
            {
                if (!IsSafeName(column))
                {
                    throw new ArgumentException($"Invalid column name: {column}");
                }
            }
        }
    }
}
