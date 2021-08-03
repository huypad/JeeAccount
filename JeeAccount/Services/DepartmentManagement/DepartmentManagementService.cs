using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using JeeAccount.Reponsitories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DepartmentManagement
{
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;
        private readonly IDepartmentManagementReponsitory _reponsitory;

        public DepartmentManagementService(IConfiguration configuration, IDepartmentManagementReponsitory reponsitory)
        {
            _config = configuration;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _reponsitory = reponsitory;
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            return _reponsitory.ChangeTinhTrang(customerID, RowID, Note, UserIdLogin);
        }

        public bool CheckDepartmentExist(long CustomerID, string connectionString)
        {
            return _reponsitory.CheckDepartmentExist(CustomerID, connectionString);
        }

        public void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username)
        {
            _reponsitory.CreateDepartment(departmentModel, CustomerID, Username);
        }

        public async Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID)
        {
            return await _reponsitory.GetListDepartmentDefaultAsync(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRtAsync(long custormerID)
        {
            return await _reponsitory.GetListDepartmentIsJeeHRtAsync(custormerID).ConfigureAwait(false);
        }
    }
}