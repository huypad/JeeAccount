using JeeAccount.Models.Common;
using JeeAccount.Models.StructureManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.StructureManagementService
{
    public interface IStructureManagementService
    {
        Task<IEnumerable<StructureDTO>> GetOrgStructure([FromQuery] QueryParams query);

        Task<IEnumerable<StructureDTO>> Sysn_Structure(long CustomerID);
    }
}