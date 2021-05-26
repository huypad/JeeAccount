using JeeAccount.Models.DatabaseManagement;

namespace JeeAccount.Services.DatabaseManagementService
{
    public interface IDatabaseManagementService
    {
        DatabaseListDTO GetDBByCustomerIDAppCode(long CustomerID, string appCode);
    }
}