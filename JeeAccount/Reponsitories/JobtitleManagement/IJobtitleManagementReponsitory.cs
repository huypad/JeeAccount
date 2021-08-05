using JeeAccount.Models.Common;
using JeeAccount.Models.JeeHR;
using JeeAccount.Models.JobtitleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.JobtitleManagement
{
    public interface IJobtitleManagementReponsitory
    {
        Task<IEnumerable<JobtitleDTO>> GetListJobtitleDefaultAsync(long custormerID, string where = "", string orderBy = "");

        Task<IEnumerable<JeeHRChucVuFromDB>> GetListJobtitleIsJeeHRAsync(long custormerID, string where = "", string orderBy = "");

        ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);

        void CreateJobtitle(JobtitleModel JobtitleModel, long CustomerID, string Username);

        bool CheckJobtitleExist(long CustomerID, string connectionString);
    }
}