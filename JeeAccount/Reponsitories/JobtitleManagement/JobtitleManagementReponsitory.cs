using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.JeeHR;
using JeeAccount.Models.JobtitleManagement;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.JobtitleManagement
{
    public class JobtitleManagementReponsitory : IJobtitleManagementReponsitory
    {
        private readonly string _connectionString;

        private const string SQL_DSJOBTITLE_DEFAULT = @"select * from JobtitleList";

        private const string SQL_DSJOBTITLE_JEEHR = @"select DISTINCT JobtitleID, Jobtitle from AccountList";

        public JobtitleManagementReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
        }

        public async Task<IEnumerable<JobtitleDTO>> GetListJobtitleDefaultAsync(long custormerID, string where = "", string orderBy = "")
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string where_order = "";
            if (!string.IsNullOrEmpty(where))
            {
                where += " and JobtitleList.CustomerID = @CustomerID and (JobtitleList.Disable != 1 or JobtitleList.Disable is null)";
            }
            else
            {
                where += " JobtitleList.CustomerID = @CustomerID and (JobtitleList.Disable != 1 or JobtitleList.Disable is null)";
            }
            where_order += $"where {where} ";

            if (!string.IsNullOrEmpty(orderBy)) where_order += $"order by {orderBy}";

            string sql = @$"{SQL_DSJOBTITLE_DEFAULT} { where_order}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                var result = dt.AsEnumerable().Select(row => new JobtitleDTO
                {
                    RowID = long.Parse(row["RowID"].ToString()),
                    IsActive = Convert.ToBoolean((bool)row["IsActive"]),
                    Title = row["JobtitleName"] != DBNull.Value ? row["JobtitleName"].ToString() : "",
                    Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : "",
                });
                return await Task.FromResult(result).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<JeeHRChucVuFromDB>> GetListJobtitleIsJeeHRAsync(long custormerID, string where = "", string orderBy = "")
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string where_order = "";
            if (!string.IsNullOrEmpty(where))
            {
                where += " and AccountList.CustomerID = @CustomerID and AccountList.Disable != 1";
            }
            else
            {
                where += " AccountList.CustomerID = @CustomerID and AccountList.Disable != 1";
            }

            where_order += $"where {where} ";

            if (!string.IsNullOrEmpty(orderBy)) where_order += $"order by {orderBy}";

            string sql = @$"{SQL_DSJOBTITLE_JEEHR} { where_order}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);
                var result = dt.AsEnumerable().Select(row => new JeeHRChucVuFromDB
                {
                    RowID = int.Parse(row["JobtitleID"].ToString()),
                    Title = row["Jobtitle"].ToString()
                });
                return result;
            }
        }

        public void CreateJobtitle(JobtitleModel JobtitleModel, long CustomerID, string Username)
        {
            Hashtable val = new Hashtable();
            try
            {
                using (DpsConnection cnn = new DpsConnection(_connectionString))
                {
                    #region val data

                    val.Add("JobtitleName", JobtitleModel.JobtitleName);
                    if (!string.IsNullOrEmpty(JobtitleModel.Note)) val.Add("Note", JobtitleModel.Note);
                    val.Add("IsActive", 1);
                    val.Add("Disable", 0);
                    val.Add("CreatedDate", DateTime.Now);
                    val.Add("CreatedBy", Username);
                    val.Add("CustomerID", CustomerID);

                    #endregion val data

                    cnn.BeginTransaction();
                    int x = cnn.Insert(val, "JobtitleList");
                    if (x <= 0)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        throw cnn.LastError;
                    }
                    if (JobtitleModel.ThanhVien is not null)
                    {
                        if (JobtitleModel.ThanhVien.Count() > 0)
                        {
                            var id = Int32.Parse(cnn.ExecuteScalar("SELECT IDENT_CURRENT ('JobtitleList') AS Current_Identity;").ToString());
                            foreach (string username in JobtitleModel.ThanhVien)
                            {
                                updateJobtitleForAccountList(cnn, id, username);
                            }
                        }
                    }

                    cnn.EndTransaction();
                }
            }
            catch
            {
                throw;
            }
        }

        private void updateJobtitleForAccountList(DpsConnection cnn, int RowIdJobtitle, string AccountUsername)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            conds.Add("Username", AccountUsername);
            val.Add("JobtitleID", RowIdJobtitle);
            int x = cnn.Update(val, conds, "AccountList");
            if (x <= 0)
            {
                cnn.RollbackTransaction();
                cnn.EndTransaction();
                throw cnn.LastError;
            }
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            Hashtable val = new Hashtable();
            if (!string.IsNullOrEmpty(Note))
            {
                val.Add("Note", Note);
            }
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("RowID", RowID);
            string sql = $"select IsActive from JobtitleList where CustomerID=@CustomerID and RowID=@RowID and (Disable = 0 or Disable is null)";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                DataTable dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                {
                    return new ReturnSqlModel("RowID không tồn tại", Constant.ERRORCODE_NOTEXIST);
                }
                string sqlGetUsernameLogin = $"select Username from AccountList where CustomerID=@CustomerID and UserID=@UserID and (Disable = 0 or Disable is null)";
                SqlConditions CondsLogin = new SqlConditions();
                CondsLogin.Add("UserID", UserIdLogin);
                CondsLogin.Add("CustomerID", customerID);
                DataTable dtGetUsernameLogin = new DataTable();
                dtGetUsernameLogin = cnn.CreateDataTable(sqlGetUsernameLogin, CondsLogin);
                var isTinhTrang = Convert.ToBoolean((bool)dt.Rows[0][0]);
                if (isTinhTrang)
                {
                    val.Add("DeActiveDate", DateTime.Now);
                    val.Add("DeActiveBy", dtGetUsernameLogin.Rows[0][0]);
                }
                else
                {
                    val.Add("ActiveDate", DateTime.Now);
                    val.Add("ActiveBy", dtGetUsernameLogin.Rows[0][0]);
                }
                val.Add("IsActive", !isTinhTrang);

                int x = cnn.Update(val, Conds, "JobtitleList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            return new ReturnSqlModel();
        }

        public bool CheckJobtitleExist(long CustomerID, string connectionString)
        {
            string sql = $"select * from JobtitleList where CustomerID = {CustomerID}";
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}