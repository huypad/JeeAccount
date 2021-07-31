using JeeAccount.Models.Common;
using JeeAccount.Models.JobtitleManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.JobtitleManagement
{
    public interface IJobtitleManagementReponsitory
    {
        Task<IEnumerable<JobtitleDTO>> GetListJobtitleAsync(long custormerID);

        Task ApiGoi1lanSaveJobtitleID(long customerid, List<string> usernames);

        ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);

        void CreateJobtitle(JobtitleModel JobtitleModel, long CustomerID, string Username);

        bool CheckJobtitleExist(long CustomerID, string connectionString);
    }
}