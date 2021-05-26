using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DepartmentManagementService
{
    public interface IDepartmentManagementService
    {
        Task<IEnumerable<DepartmentDTO>> GetListDepartment(long CustomerID);

        ReturnSqlModel CreateDepartment(DepartmentModel departmentModel, long CustomerID, long UserID);

        ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);
    }
}