using JeeAccount.Models.Common;
using JeeAccount.Models.StructureManagement;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public interface IStructureManagementReponsitory
    {
        Task<IEnumerable<StructureDTO>> GetOrgStructure([FromQuery] QueryParams query);
    }
}
