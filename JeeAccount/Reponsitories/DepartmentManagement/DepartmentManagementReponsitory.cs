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

        private const string SQL_DSDEPARMENT_DEFAULT = @"select DepartmentList.*, UserID as DepartmentManagerUserID,
                                                    Username as DepartmentManagerUsername, AccountList.LastName +' '+ AccountList.FirstName
                                                    as DepartmentManagerName from DepartmentList
                                                    left join AccountList on AccountList.Username = DepartmentList.DepartmentManager";

        private const string SQL_DSDEPARTMENT_JEEHR = @"select DISTINCT DepartmentID, Department from AccountList";

        public DepartmentManagementReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
        }

        public async Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID, string where = "", string orderBy = "")
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string where_order = "";
            if (!string.IsNullOrEmpty(where))
            {
                where += " and DepartmentList.CustomerID = @CustomerID ";
            }
            else
            {
                where += " DepartmentList.CustomerID = @CustomerID and (DepartmentList.Disable != 1 or DepartmentList.Disable is null)";
            }
            where_order += $"where {where} ";

            if (!string.IsNullOrEmpty(orderBy)) where_order += $"order by {orderBy}";

            string sql = @$"{SQL_DSDEPARMENT_DEFAULT} { where_order}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = await cnn.CreateDataTableAsync(sql, Conds).ConfigureAwait(false);
                var result = dt.AsEnumerable().Select(row => new DepartmentDTO
                {
                    RowID = long.Parse(row["RowID"].ToString()),
                    IsActive = Convert.ToBoolean((bool)row["IsActive"]),
                    DepartmentManager = row["DepartmentManagerName"] != DBNull.Value ? row["DepartmentManagerName"].ToString() : "",
                    DepartmentManagerUserID = row["DepartmentManagerUserID"] != DBNull.Value ? long.Parse(row["DepartmentManagerUserID"].ToString()) : 0,
                    DepartmentManagerUsername = row["DepartmentManagerUsername"] != DBNull.Value ? row["DepartmentManagerUsername"].ToString() : "",
                    DepartmentName = row["DepartmentName"] != DBNull.Value ? row["DepartmentName"].ToString() : "",
                    Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : "",
                    Description = row["Description"] != DBNull.Value ? row["Description"].ToString() : "",
                });
                return result;
            }
        }

        public async Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRAsync(long custormerID, string where = "", string orderBy = "")
        {
            DataTable dt = new DataTable();
            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", custormerID);

            string where_order = "";
            if (!string.IsNullOrEmpty(where))
            {
                where += " and AccountList.CustomerID = @CustomerID ";
            }
            else
            {
                where += " AccountList.CustomerID = @CustomerID and AccountList.Disable != 1";
            }

            where_order += $"where {where} ";

            if (!string.IsNullOrEmpty(orderBy)) where_order += $"order by {orderBy}";

            string sql = @$"{SQL_DSDEPARTMENT_JEEHR} { where_order}";

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
                    if (!string.IsNullOrEmpty(departmentModel.Description)) val.Add("Description", departmentModel.Description);

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
                                UpdateDepartmentForAccountList(cnn, departmentModel, username);
                            }
                        }
                    }

                    cnn.EndTransaction();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateDepartment(DepartmentModel departmentModel, long CustomerID, string Username, bool isJeeHR = false)
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
                    val.Add("LastModified", DateTime.Now);
                    val.Add("ModifiedBy", Username);
                    if (!string.IsNullOrEmpty(departmentModel.Description)) val.Add("Description", departmentModel.Description);

                    #endregion val data

                    SqlConditions conds = new SqlConditions();
                    conds.Add("RowID", departmentModel.RowID);

                    cnn.BeginTransaction();
                    int x = cnn.Update(val, conds, "DepartmentList");
                    if (x <= 0)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        throw cnn.LastError;
                    }
                    if (departmentModel.ThanhVienDelete is not null)
                    {
                        if (departmentModel.ThanhVienDelete.Count() > 0)
                        {
                            foreach (string username in departmentModel.ThanhVienDelete)
                            {
                                DeleteDepartmentForAccountList(cnn, departmentModel, username);
                            }
                        }
                    }
                    if (departmentModel.ThanhVien is not null)
                    {
                        if (departmentModel.ThanhVien.Count() > 0)
                        {
                            foreach (string username in departmentModel.ThanhVien)
                            {
                                UpdateDepartmentForAccountList(cnn, departmentModel, username);
                            }
                        }
                    }

                    cnn.EndTransaction();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DepartmentModel GetDepartment(int rowid, long CustomerID)
        {
            try
            {
                using (DpsConnection cnn = new DpsConnection(_connectionString))
                {
                    SqlConditions conds = new SqlConditions();
                    conds.Add("RowID", rowid);
                    conds.Add("CustomerID", CustomerID);

                    string sql = "select * from DepartmentList where RowID = @RowID and CustomerID = @CustomerID";
                    var dt = cnn.CreateDataTable(sql, conds);
                    var result = dt.AsEnumerable().Select(row => new DepartmentDTO
                    {
                        RowID = long.Parse(row["RowID"].ToString()),
                        DepartmentManager = row["DepartmentManager"] != DBNull.Value ? row["DepartmentManager"].ToString() : "",
                        DepartmentName = row["DepartmentName"] != DBNull.Value ? row["DepartmentName"].ToString() : "",
                        Note = row["Note"] != DBNull.Value ? row["Note"].ToString() : "",
                    }).SingleOrDefault();

                    if (result == null) throw new KhongCoDuLieuException();
                    var department = new DepartmentModel();
                    department.RowID = result.RowID;
                    department.DepartmentName = result.DepartmentManager;
                    department.DepartmentManager = result.DepartmentManager;
                    department.Note = result.Note;
                    string sql2 = "select Username, UserID from AccountList where DepartmentID = @RowID and CustomerID = @CustomerID";
                    var dt2 = cnn.CreateDataTable(sql2, conds);
                    var lst = dt2.AsEnumerable().Select(row => new CommonInfo
                    {
                        Username = row["Username"].ToString(),
                        UserID = long.Parse(row["UserID"].ToString()),
                    }).ToList();
                    if (lst.Count > 0)
                        department.ThanhVien = lst.Select(item => item.Username).ToList();
                    return department;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateDepartmentForAccountList(DpsConnection cnn, DepartmentModel departmentModel, string Username, bool isJeeHR = false)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            conds.Add("Username", Username);
            val.Add("DepartmentID", departmentModel.RowID);
            if (isJeeHR) val.Add("DepartmentName", departmentModel.DepartmentName);

            int x = cnn.Update(val, conds, "AccountList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }

        private void DeleteDepartmentForAccountList(DpsConnection cnn, DepartmentModel departmentModel, string Username)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            conds.Add("Username", Username);
            val.Add("DepartmentID", DBNull.Value);
            val.Add("Department", DBNull.Value);
            int x = cnn.Update(val, conds, "AccountList");
            if (x <= 0)
            {
                throw cnn.LastError;
            }
        }

        public void ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            try
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
                        throw new KhongCoDuLieuException("Deparment");
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
                        throw cnn.LastError;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
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

        public void UpdateDepartmentManager(string UsernameModifiedBy, long customerID, string DirectManagerUsername, int departmemntID)
        {
            Hashtable val = new Hashtable();
            val.Add("DepartmentManager", DirectManagerUsername);
            val.Add("ModifiedBy", UsernameModifiedBy);
            val.Add("LastModified", DateTime.Now);

            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("RowID", departmemntID);
            string sql = $"select RowID from DepartmentList where CustomerID=@CustomerID and RowID=@RowID";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                DataTable dt = cnn.CreateDataTable(sql, Conds);
                if (dt.Rows.Count == 0)
                {
                    throw new KhongCoDuLieuException("Department");
                }

                int x = cnn.Update(val, Conds, "DepartmentList");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
        }

        public void DeleteDepartmentManager(string DeletedBy, long customerID, int departmemntID)
        {
            string sqlUserInDepartment = $"select DepartmentID from AccountList where DepartmentID = {departmemntID} and CustomerID = {customerID}";
            Hashtable val = new Hashtable();
            val.Add("DeletedBy", DeletedBy);
            val.Add("DeletedDate", DateTime.Now);
            val.Add("Disable", 1);
            val.Add("IsActive", 0);

            SqlConditions Conds = new SqlConditions();
            Conds.Add("CustomerID", customerID);
            Conds.Add("RowID", departmemntID);

            string sql = $"select RowID from DepartmentList where CustomerID=@CustomerID and RowID=@RowID";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                var dtCheck = cnn.CreateDataTable(sqlUserInDepartment);
                if (dtCheck.Rows.Count > 0) throw new KhongDuocXoaException();
                {
                    DataTable dt = cnn.CreateDataTable(sql, Conds);
                    if (dt.Rows.Count == 0)
                    {
                        throw new KhongCoDuLieuException("Department");
                    }

                    int x = cnn.Update(val, Conds, "DepartmentList");
                    if (x <= 0)
                    {
                        throw cnn.LastError;
                    }
                }
            }
        }
    }
}