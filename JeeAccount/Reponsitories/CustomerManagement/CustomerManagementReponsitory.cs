using JeeAccount.Models.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.CustomerManagement
{
    public class CustomerManagementReponsitory: ICustomerManagementReponsitory
    {
        private readonly string _connectionString;
        public CustomerManagementReponsitory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<string> AppCodes(DpsConnection cnn, long CustomerID)
        {
            List<string> appcodes = new List<string>();
            DataTable dt = new DataTable();
            string sql = @"select AppCode from Customer_App 
join AppList on Customer_App.AppID = AppList.AppID where CustomerID = @CustomerID";
            SqlConditions conds = new SqlConditions();
            conds.Add("CustomerID", CustomerID);
            dt = cnn.CreateDataTable(sql, conds);
            for (var index = 0; index < dt.Rows.Count; index++)
            {
                appcodes.Add(dt.Rows[index][0].ToString());
            }
            return appcodes;    
        }

        public bool checkTrungCode(string Code)
        {
            DataTable dt = new DataTable();
            string sql = @"select Code from CustomerList where Code=@Code";
            SqlConditions conds = new SqlConditions();
            conds.Add("Code", Code);
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
            }
            return true;
        }

        public ReturnSqlModel CreateAppCode(DpsConnection cnn, CustomerModel customerModel, long CustomerID)
        {
            try
            {
                DateTime start = DateTime.ParseExact(customerModel.RegisterDate, "dd/MM/yyyy", null);
                for (var index = 0; index < customerModel.AppID.Count; index++)
                {
                    Hashtable val = new Hashtable();
                    val.Add("CustomerID", CustomerID);
                    val.Add("AppID", customerModel.AppID[index]);
                    val.Add("StartDate", start);
                    val.Add("CreatedDate", DateTime.Now);
                    val.Add("CreatedBy", 0);
                    val.Add("Status", 1);
                    val.Add("IsDefaultApply", 1);
                    val.Add("SoLuongNhanSu", customerModel.SoLuongNhanSu[index]);
                    if (!string.IsNullOrEmpty(customerModel.DeadlineDate))
                    {
                        DateTime end = DateTime.ParseExact(customerModel.DeadlineDate, "dd/MM/yyyy", null);
                        val.Add("EndDate", end);
                    }
                    int x = cnn.Insert(val, "Customer_App");
                    if (x <= 0)
                    {
                        return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
            return new ReturnSqlModel();
        }

        public ReturnSqlModel CreateCustomer(DpsConnection cnn, CustomerModel customerModel)
        {
            Hashtable val = new Hashtable();
            try
            {
                #region val data
                val.Add("Code", customerModel.Code);
                val.Add("CompanyName", customerModel.CompanyName);
                val.Add("RegisterName", customerModel.RegisterName);
                val.Add("Address", customerModel.Address);
                val.Add("Phone", customerModel.Phone);
                val.Add("Disable", 0);
                if (!string.IsNullOrEmpty(customerModel.Note))
                {
                    val.Add("Note", customerModel.Note);
                }
                string username = customerModel.Code + ".admin";
                val.Add("Gender", customerModel.Gender);
                val.Add("RegisterDate", DateTime.Now);

                #endregion
                int x = cnn.Insert(val, "CustomerList");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                }
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
            return new ReturnSqlModel();
        }

        public long GetlastCustomerID(DpsConnection cnn)
        {
            long customerID = -1;
            var rowid = cnn.ExecuteScalar("SELECT IDENT_CURRENT ('CustomerList') AS Current_Identity;");
            customerID = long.Parse(rowid.ToString());
            return customerID;
        }

        public IEnumerable<AppListDTO> GetListApp()
        {
            DataTable dt = new DataTable();

            string sql = @"select * from AppList";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new AppListDTO
                {
                    AppID = Int32.Parse(row["AppID"].ToString()),
                    APIUrl = row["APIUrl"].ToString(),
                    AppCode = row["AppCode"].ToString(),
                    AppName = row["AppName"].ToString(),
                    BackendURL = row["BackendURL"].ToString(),
                    CurrentVersion = row["CurrentVersion"].ToString(),
                    Description = row["Description"].ToString(),
                    LastUpdate = row["LastUpdate"].ToString(),
                    Note = row["Note"].ToString(),
                    ReleaseDate = row["ReleaseDate"].ToString(),
                    IsDefaultApp = false,
                });
            }
        }

        public IEnumerable<CustomerModelDTO> GetListCustomer()
        {
            DataTable dt = new DataTable();

            string sql = @"select * from CustomerList
                           where RowId=@RowId";
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new CustomerModelDTO
                {
                    Address = row["Address"].ToString(),
                    Code = row["Code"].ToString(),
                    CompanyName = row["CompanyName"].ToString(),
                    Note = row["Note"].ToString(),
                    Phone = row["Phone"].ToString(),
                    RegisterDate = row["RegisterDate"].ToString(),
                    RegisterName = row["RegisterName"].ToString(),
                    RowID = Int32.Parse(row["RowID"].ToString()),
                    Status = Int32.Parse(row["Status"].ToString()),
                });
            }
        }

        public IEnumerable<CustomerModelDTO> GetListCustomer(string whereSrt, string orderByStr)
        {
            DataTable dt = new DataTable();
            string sql = "";
            if (string.IsNullOrEmpty(whereSrt))
            {
                sql = $@"select * from CustomerList order by {orderByStr}";
            } else
            {
                sql = $@"select * from CustomerList where {whereSrt} order by {orderByStr}";
            }

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new CustomerModelDTO
                {
                    Address = row["Address"].ToString(),
                    Code = row["Code"].ToString(),
                    CompanyName = row["CompanyName"].ToString(),
                    Note = row["Note"].ToString(),
                    Phone = row["Phone"].ToString(),
                    RegisterDate = row["RegisterDate"].ToString(),
                    RegisterName = row["RegisterName"].ToString(),
                    RowID = Int32.Parse(row["RowID"].ToString()),
                    Status = Int32.Parse(row["Status"].ToString()),
                });
            }        
        }
    }
}
