using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Models.JeeHR;
using JeeAccount.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JeeAccount.Reponsitories
{
    public static class GeneralReponsitory
    {
        public static bool IsExist(string sql, string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
                return true;
            }
        }

        public static bool IsExistCnn(string sql, DpsConnection cnn)
        {
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsExistCnn(string sql, DpsConnection cnn, SqlConditions conds)
        {
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql, conds);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static object GetUsernameByUserIDCnn(DpsConnection cnn, string UserID)
        {
            string sql = $"select username from AccountList where UserID = {UserID}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static object GetUsernameByUserID(string connectionString, string UserID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select username from AccountList where UserID = {UserID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static UserNameDTO GetUsernameAndUserIDByStaffID(string connectionString, long Staffid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select Username, UserID from AccountList where StaffID = {Staffid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new UserNameDTO
                {
                    Username = row["Username"].ToString(),
                    UserId = Convert.ToInt64(row["UserID"]),
                    StaffID = Staffid
                }).SingleOrDefault();
            }
        }

        public static List<UserNameDTO> GetListUserNameDTOByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select Username, UserID, StaffID from AccountList where CustomerID = {customerid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                return dt.AsEnumerable().Select(row => new UserNameDTO
                {
                    Username = row["Username"].ToString(),
                    UserId = Convert.ToInt64(row["UserID"]),
                    StaffID = Convert.ToInt64(row["StaffID"])
                }).ToList<UserNameDTO>();
            }
        }

        public static List<UserNameDTO> GetListUserNameDTOByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            string sql = $"select Username, UserID, StaffID from AccountList where CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            return dt.AsEnumerable().Select(row => new UserNameDTO
            {
                Username = row["Username"].ToString(),
                UserId = Convert.ToInt64(row["UserID"]),
                StaffID = row["StaffID"] != DBNull.Value ? Convert.ToInt64(row["StaffID"]) : 0
            }).ToList<UserNameDTO>();
        }

        public static UserNameDTO GetUsernameAndUserIDByStaffIDCnn(DpsConnection cnn, long Staffid)
        {
            string sql = $"select Username, UserID from AccountList where StaffID = {Staffid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            return dt.AsEnumerable().Select(row => new UserNameDTO
            {
                Username = row["Username"].ToString(),
                UserId = Convert.ToInt64(row["UserID"]),
                StaffID = Staffid
            }).SingleOrDefault();
        }

        public static object GetCustomerIDByUserID(string connectionString, string UserID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select CustomerID from AccountList where UserID = {UserID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetStaffIDByUserID(string connectionString, string UserID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select StaffID from AccountList where UserID = {UserID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetUserIDByStaffID(string connectionString, string StaffID)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select UserID from AccountList where StaffID = {StaffID}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetUserIDByStaffIDCnn(DpsConnection cnn, string StaffID)
        {
            string sql = $"select UserID from AccountList where StaffID = {StaffID}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static bool CheckIsAdminByUserIDCnn(DpsConnection cnn, string UserId)
        {
            string sql = $"select * from AccountList where UserId = {UserId} and IsAdmin = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsActiveByUserIDCnn(DpsConnection cnn, string UserId)
        {
            string sql = $"select * from AccountList where UserId = {UserId} and IsActive = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsActiveByUsernameCnn(DpsConnection cnn, string username)
        {
            string sql = $"select * from AccountList where username = '{username}' and IsActive = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool CheckIsAdminByUsernameCnn(DpsConnection cnn, string username)
        {
            string sql = $"select * from AccountList where username = '{username} 'and IsAdmin = 1";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static object GetStaffIDByUsername(string connectionString, string Username)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select StaffID from AccountList where Username = '{Username}'";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetCustomerIDByUsername(string connectionString, string Username)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select CustomerID from AccountList where Username = '{Username}'";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return dt.Rows[0][0].ToString();
            }
        }

        public static object GetUserIDByUsernameCnn(DpsConnection cnn, string Username)
        {
            string sql = $"select UserID from AccountList where Username = '{Username}'";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return Int32.Parse(dt.Rows[0][0].ToString());
        }

        public static object GetStaffIDByUserIDCnn(DpsConnection cnn, string UserID)
        {
            string sql = $"select StaffID from AccountList where UserID = {UserID}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static object GetStaffIDByUsernameCnn(DpsConnection cnn, string Username)
        {
            string sql = $"select StaffID from AccountList where Username = '{Username}'";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
            return dt.Rows[0][0].ToString();
        }

        public static object GetUserIDByUsername(string connectionString, string Username)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                string sql = $"select UserID from AccountList where Username = '{Username}'";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                if (string.IsNullOrEmpty(dt.Rows[0][0].ToString())) return null;
                return Int32.Parse(dt.Rows[0][0].ToString());
            }
        }

        public static List<long> GetLstCustomerid(string connectionString)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var ListCustomerid = new List<long>();
                string sql = $"select RowID from CustomerList where RowID > 0";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    ListCustomerid.Add(GeneralService.ConvertToLong(dr["RowID"]));
                }
                return ListCustomerid;
            }
        }

        public static List<long> GetLstCustomeridCnn(DpsConnection cnn)
        {
            var ListCustomerid = new List<long>();
            string sql = $"select RowID from CustomerList where RowID > 0";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                ListCustomerid.Add(GeneralService.ConvertToLong(dr["RowID"]));
            }
            return ListCustomerid;
        }

        public static List<long> GetLstUserIDByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var ListCustomerid = new List<long>();
                string sql = $"select UserID from AccountList where CustomerID = {customerid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    ListCustomerid.Add(GeneralService.ConvertToLong(dr["UserID"]));
                }
                return ListCustomerid;
            }
        }

        public static List<long> GetLstUserIDByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var ListCustomerid = new List<long>();
            string sql = $"select UserID from AccountList where CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                ListCustomerid.Add(GeneralService.ConvertToLong(dr["UserID"]));
            }
            return ListCustomerid;
        }

        public static List<string> GetLstUsernameByCustomerid(string connectionString, long customerid)
        {
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                var lst = new List<string>();
                string sql = $"select Username from AccountList where CustomerID = {customerid}";
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return null;
                foreach (DataRow dr in dt.Rows)
                {
                    lst.Add(dr["Username"].ToString());
                }
                return lst;
            }
        }

        public static List<string> GetLstUsernameByCustomeridCnn(DpsConnection cnn, long customerid)
        {
            var lst = new List<string>();
            string sql = $"select Username from AccountList where CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return null;
            foreach (DataRow dr in dt.Rows)
            {
                lst.Add(dr["Username"].ToString());
            }
            return lst;
        }

        public static bool IsUsedJeeHRCustomeridCnn(DpsConnection cnn, long customerid)
        {
            string sql = $"select * from Customer_App where AppID = 1 and CustomerID = {customerid}";
            DataTable dt = new DataTable();
            dt = cnn.CreateDataTable(sql);
            if (dt.Rows.Count == 0) return false;
            return true;
        }

        public static bool IsUsedJeeHRCustomerid(string connectionString, long customerid)
        {
            string sql = $"select * from Customer_App where AppID = 1 and CustomerID = {customerid}";
            using (DpsConnection cnn = new DpsConnection(connectionString))
            {
                DataTable dt = new DataTable();
                dt = cnn.CreateDataTable(sql);
                if (dt.Rows.Count == 0) return false;
            }
            return true;
        }

        public static PersonalInfoCustomData GetPersonalInfoCustomData(long UserID, long CustomerID, string connectionString)
        {
            try
            {
                DataTable dt = new DataTable();
                SqlConditions Conds = new SqlConditions();
                using (DpsConnection cnn = new DpsConnection(connectionString))
                {
                    Conds.Add("CustomerID", CustomerID);
                    Conds.Add("UserID", UserID);

                    string sql = @"select UserID, Username, Email, LastName +' ' + FirstName as FullName
                           , FirstName as Name, AvartarImgURL as Avatar, JobtitleList.JobtitleName, JobtitleID , DepartmentID,
                           DepartmentList.DepartmentName, PhoneNumber, AccountList.CustomerID, cocauid, ChucVuID, Birthday, DirectManager
                           from AccountList
left join DepartmentList on DepartmentList.RowID = AccountList.DepartmentID
left join JobtitleList on JobtitleList.RowID = AccountList.JobtitleID
                                where CustomerID = @CustomerID and UserID = @UserID";

                    dt = cnn.CreateDataTable(sql, Conds);
                    if (dt.Rows.Count == 0)
                        throw new ArgumentNullException("PeronalInfo");

                    return new PersonalInfoCustomData
                    {
                        Avatar = dt.Rows[0]["Avatar"].ToString(),
                        Birthday = (dt.Rows[0]["Birthday"] != DBNull.Value) ? ((DateTime)dt.Rows[0]["Birthday"]).ToString("dd/MM/yyyy") : "",
                        Departmemt = dt.Rows[0]["Department"].ToString(),
                        Fullname = dt.Rows[0]["Fullname"].ToString(),
                        Jobtitle = dt.Rows[0]["Jobtitle"].ToString(),
                        Name = dt.Rows[0]["Name"].ToString(),
                        Phonenumber = dt.Rows[0]["PhoneNumber"].ToString(),
                        StructureID = dt.Rows[0]["cocauid"].ToString(),
                        BgColor = GeneralService.GetColorNameUser(dt.Rows[0]["Name"].ToString().Substring(0, 1)),
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}