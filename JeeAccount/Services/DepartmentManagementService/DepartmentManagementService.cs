using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Reponsitories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DepartmentManagementService
{
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private IDepartmentManagementReponsitory departmentManagementReponsitory;

        public DepartmentManagementService(IDepartmentManagementReponsitory departmentManagementReponsitory)
        {
            this.departmentManagementReponsitory = departmentManagementReponsitory;
        }

        public Task<IEnumerable<DepartmentDTO>> GetListDepartment(long CustomerID)
        {
            var departs = departmentManagementReponsitory.GetListDepartment(CustomerID);
            return departs;
        }

        public ReturnSqlModel CreateDepartment(DepartmentModel departmentModel, long CustomerID, long UserID)
        {
            var create = departmentManagementReponsitory.CreateDepartment(departmentModel, CustomerID, UserID);
            return create;
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            var update = departmentManagementReponsitory.ChangeTinhTrang(customerID, RowID, Note, UserIdLogin);
            return update;
        }
    }
}