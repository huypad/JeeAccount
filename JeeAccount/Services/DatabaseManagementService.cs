using JeeAccount.Models.DatabaseManagement;
using JeeAccount.Reponsitories.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class DatabaseManagementService
    {
        private IDatabaseManagementRepositoty databaseManagementRepositoty;
        private readonly string ConnectionString;

        public DatabaseManagementService(string connectionString)
        {
            this.databaseManagementRepositoty = new DatabaseManagementRepositoty(connectionString);
        }

        public DatabaseListDTO GetDBByCustomerIDAppCode(long CustomerID, string appCode)
        {
            return databaseManagementRepositoty.GetDBByCustomerIDAppCode(CustomerID, appCode);
        }
    }
}
