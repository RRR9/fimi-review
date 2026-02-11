using System.Data.SqlClient;
using System.Data;

namespace Fimi.Db
{
    class DbContext
    {
        private readonly string _connect;

        public DbContext(string connect)
        {
            _connect = connect;
        }

        public DbContext()
        {
            _connect = Utils.DbConnection();
        }

        protected enum TypeReturn : int
        {
            DataTable = 0,
            SqlDataReader = 1,
            DataSet = 2,
            Empty = 3
        }

        protected enum TypeCommand : int
        {
            SqlQuery = 0,
            SqlStoredProcedure = 1
        }

        protected virtual SqlObjects? Exec(string sql, SqlParameter[] sqlParam, TypeReturn typeReturn, TypeCommand typeCommand)
        {
            SqlConnection sqlConnection = new SqlConnection(_connect);
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);

            if (typeCommand == TypeCommand.SqlQuery)
            {
                sqlCommand.CommandType = CommandType.Text;
            }
            else
            {
                sqlCommand.CommandType = CommandType.StoredProcedure;
            }

            if (sqlParam != null)
            {
                for (int i = 0; i < sqlParam.Length; i++)
                {
                    sqlCommand.Parameters.Add(sqlParam[i]);
                }
            }

            if (typeReturn == TypeReturn.Empty)
            {
                sqlCommand.ExecuteNonQuery();
                return null;
            }

            if (typeReturn == TypeReturn.SqlDataReader)
            {
                return new SqlObjects()
                {
                    Connection = sqlConnection,
                    Command = sqlCommand,
                    Reader = sqlCommand.ExecuteReader()
                };
            }

            if(typeReturn == TypeReturn.DataTable)
            {
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                DataTable dataTable = new DataTable();
                dataTable.Load(sqlDataReader);

                return new SqlObjects()
                {
                    Connection = sqlConnection,
                    Command = sqlCommand,
                    DataTable = dataTable,
                    Reader = sqlDataReader
                };
            }

            if(typeReturn == TypeReturn.DataSet)
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand);
                DataSet ds = new DataSet();
                dataAdapter.Fill(ds);

                return new SqlObjects()
                {
                    Connection = sqlConnection,
                    Command = sqlCommand,
                    DataSet = ds
                };
            }

            throw new ShukrMoliyaException("Unknown error");
        }

        public virtual int OperationCreate(int platformId, decimal? amount, int operationTypeId, string? extId = null)
        {
            string sql = "Operations_create";

            var list = new List<SqlParameter>()
            {
                new SqlParameter("@PlatformId", platformId),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@OperationTypeId", operationTypeId),
                new SqlParameter("@ExtId", extId)
            };

            using var sqlObject = Exec(sql, list.ToArray(), TypeReturn.SqlDataReader, TypeCommand.SqlStoredProcedure);

            var dataReader = sqlObject!.Reader;
            if(dataReader!.HasRows)
            {
                dataReader.Read();
                int resultCode = (int)dataReader["ResultCode"];
                if(resultCode == 0)
                {
                    return (int)dataReader["OperId"];
                }

                throw new ShukrMoliyaException(dataReader["ResultDesc"].ToString()!);
            }

            throw new ShukrMoliyaException("Internal error in procedure 'operation_create'");
        }

        public virtual void OperationModifyState(int platformId, int operationId, decimal? amount, string? extId, int operationType, DataBaseStates newState)
        {
            string sql = "Operations_modify_states_id";

            var sp = new SqlParameter[]
            {
                new SqlParameter("@OperationId", operationId),
                new SqlParameter("@NewState", newState),
                new SqlParameter("@ExtId", extId),
                new SqlParameter("@OperationType", operationType),
                new SqlParameter("@Platform", platformId),
                new SqlParameter("@Amount", amount)
            };

            using var sqlObject = Exec(sql, sp, TypeReturn.SqlDataReader, TypeCommand.SqlStoredProcedure);

            var dataReader = sqlObject!.Reader;
            if(dataReader!.HasRows)
            {
                dataReader.Read();
                int resultCode = Convert.ToInt32(dataReader["ResultCode"]);
                if(resultCode != 0)
                {
                    throw new ShukrMoliyaException(dataReader["ResultDesc"].ToString()!);
                }

                return;
            }

            throw new ShukrMoliyaException("Internal error in procedure 'operation_modify_state'");
        }

        public virtual int GetTransactionNumber(int operationId)
        {
            string sql = "POSRequestRqArg_get_transaction_number";

            var sp = new SqlParameter[]
            {
                new SqlParameter("@OperationId", operationId),
            };

            using var sqlObject = Exec(sql, sp, TypeReturn.SqlDataReader, TypeCommand.SqlStoredProcedure);

            var dataReader = sqlObject!.Reader;
            if (dataReader!.HasRows)
            {
                dataReader.Read();
                int resultCode = Convert.ToInt32(dataReader["ResultCode"]);
                if (resultCode == 0)
                {
                    return (int)dataReader["TransactionNumber"];
                }
                throw new ShukrMoliyaException(dataReader["ResultDesc"].ToString()!);
            }

            throw new ShukrMoliyaException("Internal error in procedure 'POSRequestRqArg_get_transaction_number'");
        }

        public virtual HashSet<string> GetRouteNames()
        {
            string sql = "OperationTypes_get_route_names";

            using var sqlObject = Exec(sql, new SqlParameter[] {}, TypeReturn.DataSet, TypeCommand.SqlStoredProcedure);

            var tables = sqlObject!.DataSet;
            var tableResult = tables!.Tables[0];

            var resultCode = Convert.ToInt32(tableResult.Rows[0]["ResultCode"]);
            if (resultCode != 0)
            {
                throw new ShukrMoliyaException(tableResult.Rows[0]["ResultDesc"].ToString()!);
            }

            var tableRouteNames = tables!.Tables[1];
            var r = new HashSet<string>();
            foreach(DataRow row in tableRouteNames.Rows)
            {
                r.Add(row["RouteName"].ToString()!);
            }

            return r;
        }

        public virtual int GetOperationTypeId(string routeName)
        {
            string sql = "OperationTypes_get_id";

            var sp = new SqlParameter[]
            {
                new SqlParameter("@routeName", routeName),
            };

            using var sqlObject = Exec(sql, sp, TypeReturn.SqlDataReader, TypeCommand.SqlStoredProcedure);

            var dataReader = sqlObject!.Reader;
            if (dataReader!.HasRows)
            {
                dataReader.Read();
                int resultCode = Convert.ToInt32(dataReader["ResultCode"]);
                if (resultCode == 0)
                {
                    return (int)dataReader["Id"];
                }
                throw new ShukrMoliyaException(dataReader["ResultDesc"].ToString()!);
            }

            throw new ShukrMoliyaException("Internal error in procedure 'OperationTypes_get_id'");
        }

        public class SqlObjects : IDisposable
        {
            private SqlConnection? _connection;
            private SqlCommand? _command;
            private SqlDataReader? _reader;
            private DataTable? _dataTable;
            private DataSet? _dataSet;
            private bool _disposed;

            public DataSet? DataSet
            {
                get { return _dataSet; }
                set { _dataSet = value; }
            }

            public SqlConnection? Connection
            {
                get { return _connection; }
                set { _connection = value; }
            }

            public SqlCommand? Command
            {
                get { return _command; }
                set { _command = value; }
            }

            public SqlDataReader? Reader
            {
                get { return _reader; }
                set { _reader = value; }
            }

            public DataTable? DataTable
            {
                get { return _dataTable; }
                set { _dataTable = value; }
            }

            public SqlObjects()
            {
                _disposed = false;
            }

            protected void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    _connection?.Dispose();
                    _command?.Dispose();
                    _dataTable?.Dispose();
                    _dataSet?.Dispose();
                }

                _reader?.Close();
                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            ~SqlObjects()
            {
                Dispose(false);
            }
        }
    }

    public enum DataBaseStates : int
    {
        New = 0,
        Await = 1,
        Accept = 2,
        Cancel = 3
    }
}
