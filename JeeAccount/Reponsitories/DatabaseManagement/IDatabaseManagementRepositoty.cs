using JeeAccount.Models.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.DatabaseManagement
{
    interface IDatabaseManagementRepositoty
    {
        DatabaseListDTO GetDBByCustomerIDAppCode(long customerID, string appCode);
    }
}
