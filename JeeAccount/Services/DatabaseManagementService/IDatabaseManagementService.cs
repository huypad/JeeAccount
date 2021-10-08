using JeeAccount.Models.DatabaseManagement;
using System.Collections.Generic;

namespace JeeAccount.Services.DatabaseManagementService
{
    public interface IDatabaseManagementService
    {
        DatabaseListDTO GetDBByCustomerIDAppCode(long CustomerID, string appCode);

        IEnumerable<DatabaseDTO> GetDBDatabaseDTO();
    }
}