using JeeAccount.Models.Common;
using JeeAccount.Models.JobtitleManagement;
using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.JobtitleManagementService
{
    public interface IJobtitleManagementService

    {
        Task<object> GetDSChucvu(QueryParams query, long customerid, string token);

        void CreateJobtitle(JobtitleModel JobtitleModel, long CustomerID, string Username);

        bool CheckJobtitleExist(long CustomerID, string connectionString);

        ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin);

        JobtitleModel GetJobtitle(int rowid, long CustomerID);

        void UpdateJobtitle(JobtitleModel job, long CustomerID, string Username, bool isJeeHR);

        void DeleteJobtile(string DeletedBy, long customerID, int departmemntID);
    }
}