using JeeAccount.Models.Common;
using JeeAccount.Models.StructureManagement;
using JeeAccount.Reponsitories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.StructureManagementService
{
    public class StructureManagementService : IStructureManagementService
    {
        private IStructureManagementReponsitory StructureManagementReponsitory;

        public StructureManagementService(IStructureManagementReponsitory structureManagementReponsitory)
        {
            this.StructureManagementReponsitory = structureManagementReponsitory;
        }

        public Task<IEnumerable<StructureDTO>> GetOrgStructure([FromQuery] QueryParams query)
        {
            var departs = StructureManagementReponsitory.GetOrgStructure(query);
            return departs;
        }

        public Task<IEnumerable<StructureDTO>> Sysn_Structure(long CustomerID)
        {
            var departs = StructureManagementReponsitory.Sysn_Structure(CustomerID);
            return departs;
        }
    }
}