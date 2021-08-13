using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DepartmentManagement
{
    public interface IDepartmentManagementService
    {
        Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID);

        Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRAsync(long custormerID);

        Task<object> GetDSPhongBan(QueryParams query, long customerid, string token);

        void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username);

        bool CheckDepartmentExist(long CustomerID, string connectionString);

        void ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);

        void UpdateDepartment(DepartmentModel departmentModel, long CustomerID, string Username, bool isJeeHR);

        DepartmentModel GetDepartment(int rowid, long CustomerID);

        void UpdateDepartmentManager(string UsernameModifiedBy, long customerID, string DirectManagerUsername, int departmemntID);

        void DeleteDepartmentManager(string DeletedBy, long customerID, int departmemntID);
    }
}