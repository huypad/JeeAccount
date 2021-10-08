using JeeAccount.Models.DatabaseManagement;
using JeeAccount.Reponsitories.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DatabaseManagementService
{
    public class DatabaseManagementService : IDatabaseManagementService
    {
        private IDatabaseManagementRepositoty databaseManagementRepositoty;

        public DatabaseManagementService(IDatabaseManagementRepositoty databaseManagementRepositoty)
        {
            this.databaseManagementRepositoty = databaseManagementRepositoty;
        }

        public DatabaseListDTO GetDBByCustomerIDAppCode(long CustomerID, string appCode)
        {
            return databaseManagementRepositoty.GetDBByCustomerIDAppCode(CustomerID, appCode);
        }

        public IEnumerable<DatabaseDTO> GetDBDatabaseDTO()
        {
            return databaseManagementRepositoty.GetDBDatabaseDTO();
        }
    }
}