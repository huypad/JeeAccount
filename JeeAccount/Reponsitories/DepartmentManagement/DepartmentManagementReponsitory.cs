using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public class DepartmentManagementReponsitory : IDepartmentManagementReponsitory
    {
        private readonly string _connectionString;

        public DepartmentManagementReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
        }

        public async Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = "select RowID, IsActive, DepartmentManager, DepartmentName, Note from DepartmentList " +
                " where CustomerID=@CustomerID and (Disable != 1 or Disable is null)";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);
                var result = dt.AsEnumerable().Select(row => new DepartmentDTO
                {
                    RowID = long.Parse(row["RowID"].ToString()),
                    IsActive = Convert.ToBoolean((bool)row["IsActive"]),
                    DepartmentManager = row["DepartmentManager"].ToString(),
                    DepartmentName = row["DepartmentName"].ToString(),
                    Note = row["Note"].ToString(),
                });
                return result;
            }
        }

        public async Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRtAsync(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = "select DISTINCT DepartmentID, Department, Disable from AccountList" +
                " where CustomerID=@CustomerID and DepartmentID is not null and (Disable != 1 or Disable is null)";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);
                var result = dt.AsEnumerable().Select(row => new JeeHRCoCauToChucModelFromDB
                {
                    RowID = int.Parse(row["DepartmentID"].ToString()),
                    Title = row["Department"].ToString()
                });
                return result;
            }
        }

        public void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username)
        {
            Hashtable val = new Hashtable();
            try
            {
                using (DpsConnection cnn = new DpsConnection(_connectionString))
                {
                    #region val data

                    val.Add("DepartmentName", departmentModel.DepartmentName);
                    if (!string.IsNullOrEmpty(departmentModel.DepartmentManager)) val.Add("DepartmentManager", departmentModel.DepartmentManager);
                    if (!string.IsNullOrEmpty(departmentModel.Note)) val.Add("Note", departmentModel.Note);
                    val.Add("IsActive", 1);
                    val.Add("Disable", 0);
                    val.Add("CreatedDate", DateTime.Now);
                    val.Add("CreatedBy", Username);
                    val.Add("CustomerID", CustomerID);

                    #endregion val data

                    cnn.BeginTransaction();
                    int x = cnn.Insert(val, "DepartmentList");
                    if (x <= 0)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        throw cnn.LastError;
                    }
                    if (departmentModel.ThanhVien is not null)
                    {
                        if (departmentModel.ThanhVien.Count() > 0)
                        {
                            var id = Int32.Parse(cnn.ExecuteScalar("SELECT IDENT_CURRENT ('DepartmentList') AS Current_Identity;").ToString());
                            foreach (string username in departmentModel.ThanhVien)
                            {
                                UpdateDepartmentForAccountList(cnn, id, username);
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

        private void UpdateDepartmentForAccountList(DpsConnection cnn, int RowIdDepartment, string AccountUsername)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            conds.Add("Username", AccountUsername);
            val.Add("DepartmentID", RowIdDepartment);
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
            string sql = $"select IsActive from DepartmentList where CustomerID=@CustomerID and RowID=@RowID";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                DataTable dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                {
                    return new ReturnSqlModel("RowID không tồn tại", Constant.ERRORCODE_NOTEXIST);
                }
                string sqlGetUsernameLogin = $"select Username from AccountList where CustomerID=@CustomerID and UserID=@UserID";
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

                int x = cnn.Update(val, Conds, "DepartmentList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            return new ReturnSqlModel();
        }

        public bool CheckDepartmentExist(long CustomerID, string connectionString)
        {
            string sql = $"select * from DepartmentList where CustomerID = {CustomerID} and (Disable = 0 or Disable is null)";
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