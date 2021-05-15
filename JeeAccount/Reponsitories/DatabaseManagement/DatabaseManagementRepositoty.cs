using DpsLibs.Data;
using JeeAccount.Models.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.DatabaseManagement
{
    public class DatabaseManagementRepositoty : IDatabaseManagementRepositoty
    {
        private readonly string _connectionString;
        public DatabaseManagementRepositoty(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DatabaseListDTO GetDBByCustomerIDAppCode(long customerID, string appCode)
        {
            DataTable dt = new DataTable();
            string sql = $@"select DatabaseList.InstantName, DatabaseList.DatabaseName from DatabaseList
join Customer_App on Customer_App.DatabaseID = DatabaseList.RowID
join AppList on AppList.AppID = Customer_App.AppID
where DatabaseList.AppCode = '{appCode}' and Customer_App.CustomerID = @CustomerID";

            SqlConditions conds = new SqlConditions();
            conds.Add("CustomerID", customerID);

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, conds);
                return dt.AsEnumerable().Select(row => new DatabaseListDTO
                {
                    DatabaseName = row["DatabaseName"].ToString(),
                    InstantName = row["InstantName"].ToString()
                }).SingleOrDefault();
            }
        }
    }
}
