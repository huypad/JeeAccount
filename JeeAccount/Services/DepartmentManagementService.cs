using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Reponsitories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class DepartmentManagementService
    {
        private IDepartmentManagementReponsitory departmentManagementReponsitory;

        public DepartmentManagementService(string connectionString)
        {
            this.departmentManagementReponsitory = new DepartmentManagementReponsitory(connectionString);
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
    }
}
