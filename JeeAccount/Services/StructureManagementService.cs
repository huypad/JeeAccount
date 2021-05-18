using JeeAccount.Models.Common;
using JeeAccount.Models.StructureManagement;
using JeeAccount.Reponsitories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services
{
    public class StructureManagementService
    {
        private IStructureManagementReponsitory StructureManagementReponsitory;

        public StructureManagementService(string connectionString)
        {
            this.StructureManagementReponsitory = new StructureManagementReponsitory(connectionString);
        }

        public Task<IEnumerable<StructureDTO>> GetOrgStructure([FromQuery] QueryParams query)
        {
            var departs = StructureManagementReponsitory.GetOrgStructure(query);
            return departs;
        }
    }
}
