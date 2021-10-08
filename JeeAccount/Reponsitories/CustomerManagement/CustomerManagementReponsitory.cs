using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.CustomerManagement;
using JeeAccount.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories.CustomerManagement
{
    public class CustomerManagementReponsitory : ICustomerManagementReponsitory
    {
        private readonly string _connectionString;
        private IConfiguration _config;

        public CustomerManagementReponsitory(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _config = configuration;
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
                DateTime start = DateTime.ParseExact(customerModel.RegisterDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                for (var index = 0; index < customerModel.AppID.Count; index++)
                {
                    Hashtable val = new Hashtable();
                    val.Add("CustomerID", CustomerID);
                    val.Add("AppID", customerModel.AppID[index]);
                    if (customerModel.CurrentDBID[index] != -1)
                    {
                        val.Add("DatabaseID", customerModel.CurrentDBID[index]);
                    }
                    else
                    {
                        string sqlCurrentDB = $"select CurrentDatabaseID from AppList where AppID = {customerModel.AppID[index]}";
                        var dt = cnn.CreateDataTable(sqlCurrentDB);
                        if (dt.Rows[0]["CurrentDatabaseID"] != DBNull.Value)
                        {
                            val.Add("DatabaseID", Int32.Parse(dt.Rows[0]["CurrentDatabaseID"].ToString()));
                        }
                    }
                    val.Add("StartDate", start);
                    val.Add("CreatedDate", DateTime.Now.ToUniversalTime());
                    val.Add("CreatedBy", 0);
                    val.Add("Status", 1);
                    val.Add("IsDefaultApply", 1);
                    val.Add("SoLuongNhanSu", customerModel.SoLuongNhanSu[index]);
                    val.Add("PackageID", customerModel.GoiSuDung[index]);
                    if (!string.IsNullOrEmpty(customerModel.DeadlineDate))
                    {
                        DateTime end = DateTime.ParseExact(customerModel.DeadlineDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        val.Add("EndDate", end);
                    }
                    int x = cnn.Insert(val, "Customer_App");
                    if (x <= 0)
                    {
                        return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_EXCEPTION);
                    }
                }
                return new ReturnSqlModel();
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
        }

        public ReturnSqlModel CreateCustomer(DpsConnection cnn, CustomerModel customerModel)
        {
            Hashtable val = new Hashtable();
            try
            {
                #region val data

                val.Add("RowID", customerModel.RowID);
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
                string username = customerModel.Username;
                val.Add("Gender", customerModel.Gender);
                val.Add("RegisterDate", DateTime.Now.ToUniversalTime());

                #endregion val data

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
            }
            else
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

        public async Task<ReturnSqlModel> UpdateCustomerAppAddNumberStaff(CustomerAppAddNumberStaffModel model)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                cnn.BeginTransaction();

                foreach (var item in model.LstCustomerAppDTO)
                {
                    var res = UpdateCustomerAppAddNumberStaffByCustomerIDAppID(item.CustomerID, item.AppID, item.SoLuongNhanSu, cnn);
                    if (!res.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        await Task.FromResult(res);
                    }
                }
                cnn.EndTransaction();
                return await Task.FromResult(new ReturnSqlModel());
            }
        }

        public async Task InsertCustomerApp(CustomerAppAddNumberStaffModel model)
        {
            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                cnn.BeginTransaction();

                foreach (var item in model.LstCustomerAppDTO)
                {
                    var res = UpdateCustomerAppAddNumberStaffByCustomerIDAppID(item.CustomerID, item.AppID, item.SoLuongNhanSu, cnn);
                    if (!res.Susscess)
                    {
                        cnn.RollbackTransaction();
                        cnn.EndTransaction();
                        await Task.FromResult(res);
                    }
                }
                cnn.EndTransaction();
            }
        }

        private ReturnSqlModel UpdateCustomerAppAddNumberStaffByCustomerIDAppID(long CustomerID, long AppID, int numberSoLuong, DpsConnection cnn)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            try
            {
                conds.Add("CustomerID", CustomerID);
                conds.Add("AppID", AppID);

                val.Add("SoLuongNhanSu", numberSoLuong);

                int x = cnn.Update(val, conds, "Customer_App");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_SQL);
                }
                return new ReturnSqlModel();
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
        }

        public async Task<ReturnSqlModel> UpdateCustomerAppGiaHanModelCnn(CustomerAppGiaHanModel model, DpsConnection cnn)
        {
            cnn.BeginTransaction();
            foreach (var app in model.LstAppCustomerID)
            {
                var updateApp = this.UpdateEndDateAppByCustomerIDAppID(model.CustomerID, app, model.EndDate, cnn);
                if (!updateApp.Susscess)
                {
                    cnn.RollbackTransaction();
                    cnn.EndTransaction();
                    await Task.FromResult(updateApp);
                }
            }
            cnn.EndTransaction();
            return await Task.FromResult(new ReturnSqlModel());
        }

        private ReturnSqlModel UpdateEndDateAppByCustomerIDAppID(long CustomerID, long AppID, string EndDate, DpsConnection cnn)
        {
            Hashtable val = new Hashtable();
            SqlConditions conds = new SqlConditions();
            try
            {
                DateTime date = DateTime.ParseExact(EndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                conds.Add("CustomerID", CustomerID);
                conds.Add("AppID", AppID);

                val.Add("EndDate", date);
                int x = cnn.Update(val, conds, "Customer_App");
                if (x <= 0)
                {
                    return new ReturnSqlModel(cnn.LastError.ToString(), Constant.ERRORCODE_SQL);
                }
                return new ReturnSqlModel();
            }
            catch (Exception ex)
            {
                return new ReturnSqlModel(ex.Message, Constant.ERRORCODE_EXCEPTION);
            }
        }

        public string CompanyCode(long customerid)
        {
            DataTable dt = new DataTable();
            string sql = $"select Code from CustomerList where RowID = {customerid}";

            using (DpsConnection cnn = new DpsConnection(_connectionString))
            {
                dt = cnn.CreateDataTable(sql);
                return dt.Rows[0][0].ToString();
            }
        }

        public void UpdateCustomerAddDeletAppModelCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, string CreatedBy, List<CommonInfo> commonInfos)
        {
            try
            {
                // Customer_App
                AddCustomerAppNewCnn(cnn, customerModel, CreatedBy);
                DeleteCustomerAppCnn(cnn, customerModel);

                //Account_App
                DeleteAccountAppCnn(cnn, customerModel, CreatedBy);
                AddAccountAppNewCnn(cnn, customerModel, commonInfos, CreatedBy);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AddCustomerAppNewCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, string CreatedBy)
        {
            try
            {
                for (var index = 0; index < customerModel.LstAddAppID.Count; index++)
                {
                    AddCustomer_AppCnn(cnn, customerModel, CreatedBy, index);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AddCustomer_AppCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, string CreatedBy, int index)
        {
            string sql = $"select * from Customer_App where CustomerID = {customerModel.CustomerID} and AppID = {customerModel.LstAddAppID[index]}";

            var dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                Hashtable val = new Hashtable();
                val.Add("StartDate", DateTime.UtcNow);
                val.Add("CreatedDate", DateTime.UtcNow);
                val.Add("CreatedBy", CreatedBy);
                val.Add("Status", 1);
                val.Add("SoLuongNhanSu", customerModel.SoLuongNhanSu[index]);
                val.Add("PackageID", customerModel.GoiSuDung[index]);
                val.Add("DatabaseID", customerModel.CurrentDBID[index]);

                SqlConditions conds = new SqlConditions();
                conds.Add("CustomerID", customerModel.CustomerID);
                conds.Add("AppID", customerModel.LstAddAppID[index]);

                if (!string.IsNullOrEmpty(customerModel.EndDate))
                {
                    DateTime end = DateTime.ParseExact(customerModel.EndDate, "dd/MM/yyyy", null);
                    val.Add("EndDate", end);
                }
                int x = cnn.Update(val, conds, "Customer_App");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
            else
            {
                Hashtable val = new Hashtable();
                val.Add("CustomerID", customerModel.CustomerID);
                val.Add("AppID", customerModel.LstAddAppID[index]);
                val.Add("StartDate", DateTime.UtcNow);
                val.Add("CreatedDate", DateTime.UtcNow);
                val.Add("CreatedBy", CreatedBy);
                val.Add("Status", 1);
                val.Add("IsDefaultApply", 1);
                val.Add("PackageID", customerModel.GoiSuDung[index]);
                val.Add("SoLuongNhanSu", customerModel.SoLuongNhanSu[index]);
                val.Add("DatabaseID", customerModel.CurrentDBID[index]);

                if (!string.IsNullOrEmpty(customerModel.EndDate))
                {
                    DateTime end = DateTime.ParseExact(customerModel.EndDate, "dd/MM/yyyy", null);
                    val.Add("EndDate", end);
                }
                int x = cnn.Insert(val, "Customer_App");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
        }

        private void DeleteCustomerAppCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel)
        {
            try
            {
                for (var index = 0; index < customerModel.LstDeleteAppID.Count; index++)
                {
                    DeleteCustomer_AppCnn(cnn, customerModel, index);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteCustomer_AppCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, int index)
        {
            string sql = $"select * from Customer_App where CustomerID = {customerModel.CustomerID}";
            var dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                SqlConditions conds = new SqlConditions();
                conds.Add("CustomerID", customerModel.CustomerID);
                conds.Add("AppID", customerModel.LstDeleteAppID[index]);
                int x = cnn.Delete(conds, "Customer_App");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
        }

        private void AddAccountAppNewCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, List<CommonInfo> commonInfos, string CreatedBy)
        {
            try
            {
                for (var index = 0; index < customerModel.LstDeleteAppID.Count; index++)
                {
                    foreach (var commonInfo in commonInfos)
                    {
                        AddAccount_AppCnn(cnn, customerModel, CreatedBy, index, commonInfo);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AddAccount_AppCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, string CreatedBy, int index, CommonInfo commonInfo)
        {
            string sql = $"select * from Account_App where CustomerID = {customerModel.CustomerID} and UserID = {commonInfo.UserID}";

            var dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                Hashtable val = new Hashtable();
                val.Add("AppID", customerModel.LstAddAppID[index]);
                val.Add("Disable", 0);
                val.Add("ActivatedDate", DateTime.UtcNow);
                val.Add("UpdatedBy", CreatedBy);
                val.Add("ActivatedBy", CreatedBy);

                if (commonInfo.IsAdminHeThong)
                {
                    val.Add("IsAdmin", 1);
                }
                else
                {
                    val.Add("IsAdmin", 0);
                }

                val.Add("IsActive", 1);

                SqlConditions conds = new SqlConditions();
                conds.Add("UserID", commonInfo.UserID);
                conds.Add("AppID", customerModel.LstAddAppID[index]);

                int x = cnn.Update(val, conds, "Account_App");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
            else
            {
                Hashtable val = new Hashtable();
                val.Add("UserID", commonInfo.UserID);
                val.Add("AppID", customerModel.LstAddAppID[index]);
                val.Add("Disable", 0);
                val.Add("CreatedDate", DateTime.UtcNow);
                val.Add("CreatedBy", CreatedBy);
                if (commonInfo.IsAdminHeThong)
                {
                    val.Add("IsAdmin", 1);
                }
                else
                {
                    val.Add("IsAdmin", 0);
                }

                val.Add("IsActive", 1);

                int x = cnn.Insert(val, "Account_App");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
        }

        private void DeleteAccountAppCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, string CreatedBy)
        {
            try
            {
                for (var index = 0; index < customerModel.LstDeleteAppID.Count; index++)
                {
                    DeleteAccount_AppCnn(cnn, customerModel, CreatedBy, index);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DeleteAccount_AppCnn(DpsConnection cnn, CustomerAddDeletAppModel customerModel, string CreatedBy, int index)
        {
            string sql = $"select * from Account_App where CustomerID = {customerModel.CustomerID}";
            var dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                Hashtable val = new Hashtable();
                val.Add("Disable", 1);
                val.Add("IsActive", 0);
                val.Add("UpdatedBy", CreatedBy);

                SqlConditions conds = new SqlConditions();
                conds.Add("CustomerID", customerModel.CustomerID);
                conds.Add("AppID", customerModel.LstDeleteAppID[index]);

                int x = cnn.Update(val, conds, "Account_App");
                if (x <= 0)
                {
                    throw cnn.LastError;
                }
            }
        }
    }
}