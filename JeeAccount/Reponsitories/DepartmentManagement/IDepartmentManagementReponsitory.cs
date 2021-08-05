using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public interface IDepartmentManagementReponsitory
    {
        Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID, string where = "", string orderBy = "");

        Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRAsync(long custormerID, string where = "", string orderBy = "");

        void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username);

        bool CheckDepartmentExist(long CustomerID, string connectionString);

        ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);
    }
}