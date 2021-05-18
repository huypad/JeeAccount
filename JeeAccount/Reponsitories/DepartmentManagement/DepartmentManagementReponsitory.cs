using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
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
        public DepartmentManagementReponsitory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<IEnumerable<StructureDTO>> GetListDepartment(long custormerID)
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string sql = "select RowID, IsActive, DepartmentManager, DepartmentName, Note from DepartmentList " +
                "where CustomerID=@CustomerID and (Disable != 1 or Disable is null)";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql, Conds);
                return dt.AsEnumerable().Select(row => new StructureDTO
                {
                    RowID = long.Parse(row["RowID"].ToString()),
                    IsActive = Convert.ToBoolean((bool)row["IsActive"]),
                    DepartmentManager = row["DepartmentManager"].ToString(),
                    DepartmentName = row["DepartmentName"].ToString(),
                    Note = row["Note"].ToString(),
                });
            }
        }

        public ReturnSqlModel CreateDepartment(StructureModel departmentModel, long CustomerID, long UserID)
        {
            Hashtable val = new Hashtable();
            ReturnSqlModel returnSql = new ReturnSqlModel();
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                try
                {
                    #region val data
                    val.Add("DepartmentName", departmentModel.DepartmentName);
                    val.Add("DepartmentManager", departmentModel.DepartmentManager);
                    val.Add("Note", departmentModel.Note);
                    val.Add("IsActive", 1);
                    val.Add("Disable", 0);
                    val.Add("ActiveDate", DateTime.Now);
                    val.Add("ActiveBy", UserID);
                    val.Add("CreatedDate", DateTime.Now);
                    val.Add("CreatedBy", UserID);
                    val.Add("CustomerID", CustomerID);
                    #endregion
                    cnn.BeginTransaction();
                    int x = cnn.Insert(val, "DepartmentList");
                    if (x <= 0)
                    {
                        return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                    }
                    var id = cnn.ExecuteScalar("SELECT IDENT_CURRENT ('DepartmentList') AS Current_Identity;");
                    foreach (string username in departmentModel.ThanhVien)
                    {
                        bool updateDepartForAcc = updateDepartmentForAccountList(cnn, CustomerID, id.ToString(), username);
                        if (updateDepartForAcc == false)
                        {
                            return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                        }
                    }
                    cnn.EndTransaction();
                }
                catch (Exception ex)
                {
                    cnn.EndTransaction();
                    return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
                }
            }
            return new ReturnSqlModel();
        }
        private bool updateDepartmentForAccountList(DpsConnection cnn, long CustomerID, string RowIdDepartment, string AccountUsername)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            conds.Add("Username", AccountUsername);
            conds.Add("CustomerID", CustomerID);

            val.Add("Department", RowIdDepartment);
            int x = cnn.Update(val, conds, "AccountList");
            if (x <= 0)
            {
                cnn.RollbackTransaction();
                cnn.EndTransaction();
                return false;
            }
            return true;
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
    }
}
