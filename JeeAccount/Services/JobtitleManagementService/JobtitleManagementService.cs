﻿using JeeAccount.Models.Common;
using JeeAccount.Models.JobtitleManagement;
using JeeAccount.Models.JeeHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JeeAccount.Reponsitories.JobtitleManagement;
using Microsoft.Extensions.Configuration;
using JeeAccount.Reponsitories;
using JeeAccount.Controllers;
using JeeAccount.Classes;
using Newtonsoft.Json;

namespace JeeAccount.Services.JobtitleManagementService
{
    public class JobtitleManagementService : IJobtitleManagementService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;
        private readonly IJobtitleManagementReponsitory _reponsitory;
        private readonly string HOST_JEEHR_API;

        public JobtitleManagementService(IConfiguration configuration, IJobtitleManagementReponsitory reponsitory)
        {
            _config = configuration;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _reponsitory = reponsitory;
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
        }

        public async Task<object> GetDSChucvu(QueryParams query, long customerid, string token, bool isShowPage = false)
        {
            query = query == null ? new QueryParams() : query;
            PageModel pageModel = new PageModel();

            string orderByStrDefault = "JobtitleList.RowID asc";
            string orderByStrJeeHR = "AccountList.jobtitleid asc";
            string whereStrDefault = " (JobtitleList.Disable != 1 or JobtitleList.Disable is null)";
            string whereStrJeeHR = " (AccountList.Disable != 1 or AccountList.Disable is null)";

            Dictionary<string, string> sortableFieldsDefault = new Dictionary<string, string>
                        {
                            { "jobtitileid", "JobtitleList.RowID"},
                            { "JobtitleName", "JobtitleList.JobtitleName"},
                            { "tinhtrang", "JobtitleList.IsActive"},
                        };

            Dictionary<string, string> sortableFieldsJeeHR = new Dictionary<string, string>
                        {
                            { "jobtitileid", "AccountList.JobtitleID"},
                            { "Jobtitlename", "AccountList.JobtitleName"},
                        };

            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);
            if (!checkusedjeehr)
            {
                if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsDefault.ContainsKey(query.sortField))
                {
                    orderByStrDefault = sortableFieldsDefault[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
                }

                whereStrDefault = CreateWhereStrDefault(query);
            }
            else
            {
                if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsJeeHR.ContainsKey(query.sortField))
                {
                    orderByStrJeeHR = sortableFieldsJeeHR[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
                }

                whereStrJeeHR = CreateWhereStrJeeHR(query);
            }
            var donotcallapijeehr = query.donotcallapijeehr;
            if (checkusedjeehr)
            {
                if (!donotcallapijeehr)
                {
                    var cocauid = "0";
                    var chucdanhid = "0";
                    if (!string.IsNullOrEmpty(query.filter["cocauid"]))
                    {
                        cocauid = query.filter["cocauid"];
                    }
                    if (!string.IsNullOrEmpty(query.filter["chucdanhid"]))
                    {
                        chucdanhid = query.filter["chucdanhid"];
                    }
                    var jeehrController = new JeeHRController(HOST_JEEHR_API);
                    var list = await jeehrController.GetDSChucVu(token, cocauid, chucdanhid).ConfigureAwait(false);
                    if (list.status == 1)
                    {
                        var ds_jeehr = FilterLstJeeHRChucVu(list.data, query);

                        pageModel.TotalCount = ds_jeehr.Count;
                        if (ds_jeehr.Count() == 0) throw new KhongCoDuLieuException();
                        pageModel.AllPage = (int)Math.Ceiling(ds_jeehr.Count / (decimal)query.record);
                        pageModel.Size = query.record;
                        pageModel.Page = query.page;
                        if (query.more)
                        {
                            query.page = 1;
                            pageModel.AllPage = 1;
                            pageModel.Size = 1;
                            query.record = pageModel.TotalCount;
                        }
                        ds_jeehr = ds_jeehr.Skip((query.page - 1) * query.record).Take(query.record).ToList();
                        if (!isShowPage)
                        {
                            return ds_jeehr;
                        }
                        else
                        {
                            return new { data = ds_jeehr, panigator = pageModel };
                        }
                    }
                    else
                    {
                        throw new JeeHRException(list.error);
                    }
                }
                else
                {
                    var lst_jeehr = await _reponsitory.GetListJobtitleIsJeeHRAsync(customerid, whereStrJeeHR, orderByStrJeeHR);

                    pageModel.TotalCount = lst_jeehr.Count();
                    if (lst_jeehr.Count() == 0) throw new KhongCoDuLieuException();
                    pageModel.AllPage = (int)Math.Ceiling(lst_jeehr.Count() / (decimal)query.record);
                    pageModel.Size = query.record;
                    pageModel.Page = query.page;
                    if (query.more)
                    {
                        query.page = 1;
                        pageModel.AllPage = 1;
                        pageModel.Size = 1;
                        query.record = pageModel.TotalCount;
                    }
                    lst_jeehr = lst_jeehr.Skip((query.page - 1) * query.record).Take(query.record).ToList();
                    if (!isShowPage)
                    {
                        return lst_jeehr;
                    }
                    else
                    {
                        return new { data = lst_jeehr, panigator = pageModel };
                    }
                }
            }
            else
            {
                var lst_default = await _reponsitory.GetListJobtitleDefaultAsync(customerid, whereStrDefault, orderByStrDefault);

                pageModel.TotalCount = lst_default.Count();
                if (lst_default.Count() == 0) throw new KhongCoDuLieuException();
                pageModel.AllPage = (int)Math.Ceiling(lst_default.Count() / (decimal)query.record);
                pageModel.Size = query.record;
                pageModel.Page = query.page;
                if (query.more)
                {
                    query.page = 1;
                    pageModel.AllPage = 1;
                    pageModel.Size = 1;
                    query.record = pageModel.TotalCount;
                }
                lst_default = lst_default.Skip((query.page - 1) * query.record).Take(query.record).ToList();
                if (!isShowPage)
                {
                    return lst_default;
                }
                else
                {
                    return new { data = lst_default, panigator = pageModel };
                }
            }
        }

        public async Task<IEnumerable<JobtitleDTO>> GetListJobtitleDefaultAsync(long custormerID)
        {
            return await _reponsitory.GetListJobtitleDefaultAsync(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<JeeHRChucVuFromDB>> GetListJobtitleIsJeeHRtAsync(long custormerID)
        {
            return await _reponsitory.GetListJobtitleIsJeeHRAsync(custormerID).ConfigureAwait(false);
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            return _reponsitory.ChangeTinhTrang(customerID, RowID, Note, UserIdLogin);
        }

        public bool CheckJobtitleExist(long CustomerID, string connectionString)
        {
            return _reponsitory.CheckJobtitleExist(CustomerID, connectionString);
        }

        public void CreateJobtitle(JobtitleModel JobtitleModel, long CustomerID, string Username)
        {
            _reponsitory.CreateJobtitle(JobtitleModel, CustomerID, Username);
        }

        private string CreateWhereStrDefault(QueryParams query)
        {
            string whereStrDefault = "";
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                whereStrDefault += $" and JobtitleList.JobtitleName like N'%{query.filter["keyword"]}%')";
            }

            if (!string.IsNullOrEmpty(query.filter["chucvu"]))
            {
                whereStrDefault += $" and (JobtitleList.Jobtitle like N'%{query.filter["chucvu"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["chucvuid"]))
            {
                whereStrDefault += $" and (JobtitleList.RowID  = {query.filter["chucvuid"]}) ";
            }

            if (!string.IsNullOrEmpty(query.filter["dakhoa"]))
            {
                if (Convert.ToBoolean(query.filter["dakhoa"]))
                {
                    whereStrDefault += $" and (JobtitleList.IsActive = 0) ";
                }
            }
            return whereStrDefault;
        }

        private string CreateWhereStrJeeHR(QueryParams query)
        {
            string whereStrJeeHR = "";
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                whereStrJeeHR += $" and AccountList.Jobtitle like N'%{query.filter["keyword"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["chucvu"]))
            {
                whereStrJeeHR += $" and (AccountList.Jobtitle like N'%{query.filter["chucvu"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["chucvuid"]))
            {
                whereStrJeeHR += $" and (AccountList.JobtitleID  = {query.filter["chucvuid"]}) ";
            }
            return whereStrJeeHR;
        }

        private List<JeeHRChucVuToJeeHRFromDB> FilterLstJeeHRChucVu(List<JeeHRChucVu> lst, QueryParams query)
        {
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                lst = lst.AsEnumerable().Where(item => item.Title.Contains(query.filter["keyword"])).ToList();
            }

            if (!string.IsNullOrEmpty(query.filter["chucvu"]))
            {
                lst = lst.AsEnumerable().Where(item => item.Title.Contains(query.filter["chucvu"])).ToList();
            }

            if (!string.IsNullOrEmpty(query.filter["chucvuid"]))
            {
                lst = lst.AsEnumerable().Where(item => item.ID.ToString() == query.filter["chucvuid"]).ToList();
            }
            var list = TranferDataHelper.LstJeeHRChucVuToJeeHRFromDBFromLstJeeHRChucvu(lst);
            return list;
        }
    }
}