using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public interface IDepartmentManagementReponsitory
    {
        Task<IEnumerable<DepartmentDTO>> GetListDepartment(long custormerID);

        void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username);

        bool CheckDepartmentExist(long CustomerID, string connectionString);

        ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);

        Task ApiGoi1lanSaveDepartmentID(long customerid, List<string> usernames);
    }
}