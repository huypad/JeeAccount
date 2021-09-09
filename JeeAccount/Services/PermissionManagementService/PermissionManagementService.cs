using DPSinfra.Kafka;
using DPSinfra.UploadFile;
using DPSinfra.Utils;
using DpsLibs.Data;
using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models;
using JeeAccount.Models.AccountManagement;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using JeeAccount.Reponsitories.PermissionManagement;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JeeAccount.Services.PermissionManagementService
{
    public class PermissionManagementService : IPermissionManagementService
    {
        private IPermissionManagementRepository _reponsitory;
        private readonly string _connectionString;
        private IConfiguration _configuration;
        private readonly IProducer _producer;
        private readonly string TOPIC_UPDATEADMIN;

        public PermissionManagementService(IPermissionManagementRepository reponsitory, IConfiguration configuration, IProducer producer)
        {
            _reponsitory = reponsitory;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _configuration = configuration;
            _producer = producer;
            TOPIC_UPDATEADMIN = configuration.GetValue<string>("KafkaConfig:TopicProduce:JeeplatformUpdateAdmin");
        }

        public async Task<IEnumerable<AccountManagementDTO>> GetListAccountAdminAppNotAdminHeThong(QueryParams query, long customerid, int AppID)
        {
            string orderByStr = "AccountList.UserID asc";
            string whereStr = " AccountList.Disable != 1 ";

            Dictionary<string, string> sortableFields = new Dictionary<string, string>
                        {
                            { "nhanvien", "AccountList.LastName"},
                            { "tendangnhap", "AccountList.Username"},
                            { "tinhtrang", "AccountList.IsActive"},
                            { "chucvuid", "AccountList.JobtitleID"},
                            { "phongbanid", "AccountList.DepartmentID" },
                            {"quanlytructiep", "AccountList.DirectManager" }
                        };

            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);

            if (!string.IsNullOrEmpty(query.sortField) && sortableFields.ContainsKey(query.sortField))
            {
                orderByStr = sortableFields[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
            }

            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                if (!checkusedjeehr)
                {
                    whereStr += $@" and (AccountList.LastName + ' ' + AccountList.FirstName like N'%{query.filter["keyword"]}%'
                                or JobtitleList.JobtitleName like N'%{query.filter["keyword"]}%'
                                or AccountList.Username like N'%{query.filter["keyword"]}%'
                                or DepartmentList.DepartmentName like N'%{query.filter["keyword"]}%')";
                }
                else
                {
                    whereStr += $@" and (AccountList.LastName + ' ' + AccountList.FirstName like N'%{query.filter["keyword"]}%'
                                    or AccountList.Jobtitle like N'%{query.filter["keyword"]}%'
                                    or AccountList.Username like N'%{query.filter["keyword"]}%'
                                    or AccountList.Department like N'%{query.filter["keyword"]}%')";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["username"]))
            {
                whereStr += $" and (AccountList.Username like '%{query.filter["username"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["tennhanvien"]))
            {
                whereStr += $" and (AccountList.FirstName like N'%{query.filter["tennhanvien"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                if (!checkusedjeehr)
                {
                    whereStr += $" and (DepartmentList.DepartmentName like N'%{query.filter["phongban"]}%') ";
                }
                else
                {
                    whereStr += $" and (AccountList.Department like N'%{query.filter["phongban"]}%') ";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                whereStr += $" and (AccountList.DepartmentID  in ({query.filter["phongbanid"]})) ";
            }

            if (!string.IsNullOrEmpty(query.filter["chucvu"]))
            {
                if (!checkusedjeehr)
                {
                    whereStr += $" and (JobtitleList.Jobtitle like N'%{query.filter["chucvu"]}%') ";
                }
                else
                {
                    whereStr += $" and (AccountList.Jobtitle like N'%{query.filter["chucvu"]}%') ";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["chucvuid"]))
            {
                whereStr += $" and (AccountList.JobtitleID like N'%{query.filter["chucvuid"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["dakhoa"]))
            {
                if (Convert.ToBoolean(query.filter["dakhoa"]))
                {
                    whereStr += $" and (AccountList.IsActive = 0) ";
                }
            }

            if (!string.IsNullOrEmpty(query.filter["email"]))
            {
                whereStr += $" and (AccountList.Email like N'%{query.filter["email"]}%') ";
            }

            var lst = Enumerable.Empty<AccountManagementDTO>();
            if (!checkusedjeehr)
            {
                var res = await _reponsitory.GetListAccountAdminAppNotAdminHeThongDefaultAsync(customerid, AppID, whereStr, orderByStr);
                lst = res;
            }
            else
            {
                var res = await _reponsitory.GetListAccountAdminAppNotAdminHeThongJeeHRAsync(customerid, AppID, whereStr, orderByStr);
                lst = res;
            }
            return lst;
        }

        public async Task CreateAdminHeThong(long userid, string username, long customerid, long UpdateBy)
        {
            try
            {
                await _reponsitory.CreateAdminHeThong(userid, customerid, UpdateBy);
                var lstApp = GeneralReponsitory.GetListAppByUserID(_connectionString, userid, customerid, true);
                var AppCodes = lstApp.Select(item => item.AppCode);
                foreach (var appCode in AppCodes)
                {
                    string objectSUpdateAdmin = JsonConvert.SerializeObject(ObjectUpdateAdminKafka(appCode, customerid, userid, username));
                    await _producer.PublishAsync(TOPIC_UPDATEADMIN, objectSUpdateAdmin).ConfigureAwait(false);
                }
                var identityServerController = new IdentityServerController();
                var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid);
                var jeeaccountObjCustom = identityServerController.JeeAccountCustomData(AppCodes.ToList(), userid, customerid, common.StaffID);
                var update = await identityServerController.UpdateCustomDataInternal(GeneralService.GetInternalToken(_configuration), username, jeeaccountObjCustom);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateAdminApp(long userid, string username, long customerid, long UpdateBy, List<int> AppID)
        {
            try
            {
                await _reponsitory.CreateAdminApp(userid, customerid, UpdateBy, AppID);
                var lstApp = GeneralReponsitory.GetListAppByUserID(_connectionString, userid, customerid, true);
                var AppCodes = lstApp.Where(item => AppID.Contains(item.AppID)).Select(item => item.AppCode);
                foreach (var appCode in AppCodes)
                {
                    string objectSUpdateAdmin = JsonConvert.SerializeObject(ObjectUpdateAdminKafka(appCode, customerid, userid, username));
                    await _producer.PublishAsync(TOPIC_UPDATEADMIN, objectSUpdateAdmin);
                }
                var identityServerController = new IdentityServerController();
                var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid);
                var jeeaccountObjCustom = identityServerController.JeeAccountCustomData(AppCodes.ToList(), userid, customerid, common.StaffID);
                var update = await identityServerController.UpdateCustomDataInternal(GeneralService.GetInternalToken(_configuration), username, jeeaccountObjCustom);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private object ObjectUpdateAdminKafka(string appCode, long customerid, long userid, string username)
        {
            var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid, username);
            if (common.StaffID == 0)
            {
                return new { CustomerID = customerid, UserID = userid, Username = username, AppCode = appCode, Action = "setadmin" };
            }
            else
            {
                return new { CustomerID = customerid, UserID = userid, Username = username, StaffID = common.StaffID, AppCode = appCode, Action = "setadmin" };
            }
        }

        public async Task RemoveAdminHeThong(long userid, string username, long customerid, long UpdateBy)
        {
            try
            {
                await _reponsitory.RemoveAdminHeThong(userid, customerid, UpdateBy);
                var lstApp = GeneralReponsitory.GetListAppByUserID(_connectionString, userid, customerid, true);
                var AppCodes = lstApp.Select(item => item.AppCode);
                foreach (var appCode in AppCodes)
                {
                    string objectSUpdateAdmin = JsonConvert.SerializeObject(ObjectRemoveAdminKafka(appCode, customerid, userid, username));
                    await _producer.PublishAsync(TOPIC_UPDATEADMIN, objectSUpdateAdmin);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private object ObjectRemoveAdminKafka(string appCode, long customerid, long userid, string username)
        {
            var common = GeneralReponsitory.GetCommonInfo(_connectionString, userid, username);
            if (common.StaffID == 0)
            {
                return new { CustomerID = customerid, UserID = userid, Username = username, AppCode = appCode, Action = "remove" };
            }
            else
            {
                return new { CustomerID = customerid, UserID = userid, Username = username, StaffID = common.StaffID, AppCode = appCode, Action = "remove" };
            }
        }

        public async Task RemoveAdminApp(long userid, string username, long customerid, long UpdateBy, List<int> AppID)
        {
            try
            {
                await _reponsitory.RemoveAdminApp(userid, customerid, UpdateBy, AppID);
                var lstApp = GeneralReponsitory.GetListAppByUserID(_connectionString, userid, customerid, true);
                var AppCodes = lstApp.Where(item => AppID.Contains(item.AppID)).Select(item => item.AppCode);
                foreach (var appCode in AppCodes)
                {
                    string objectSUpdateAdmin = JsonConvert.SerializeObject(ObjectRemoveAdminKafka(appCode, customerid, userid, username));
                    await _producer.PublishAsync(TOPIC_UPDATEADMIN, objectSUpdateAdmin).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}