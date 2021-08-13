using JeeAccount.Models.Common;
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

        public async Task<object> GetDSChucvu(QueryParams query, long customerid, string token)
        {
            query = query == null ? new QueryParams() : query;
            PageModel pageModel = new PageModel();

            string orderByStrDefault = "JobtitleList.JobtitleName asc";
            string orderByStrJeeHR = "AccountList.Jobtitle asc";
            string whereStrDefault = " (JobtitleList.Disable != 1 or JobtitleList.Disable is null)";
            string whereStrJeeHR = " (AccountList.Disable != 1 or AccountList.Disable is null)";

            Dictionary<string, string> sortableFieldsDefault = new Dictionary<string, string>
                        {
                            { "chucvuid", "JobtitleList.RowID"},
                            { "chucvu", "JobtitleList.JobtitleName"},
                            { "tinhtrang", "JobtitleList.IsActive"},
                        };

            Dictionary<string, string> sortableFieldsJeeHR = new Dictionary<string, string>
                        {
                            { "chucvuid", "AccountList.JobtitleID"},
                            { "chucvu", "AccountList.Jobtitle"},
                        };

            var checkusedjeehr = GeneralReponsitory.IsUsedJeeHRCustomerid(_connectionString, customerid);
            if (!checkusedjeehr)
            {
                if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsDefault.ContainsKey(query.sortField))
                {
                    orderByStrDefault = sortableFieldsDefault[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
                }

                whereStrDefault = CreateWhereStrDefault(query, whereStrDefault);
            }
            else
            {
                if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsJeeHR.ContainsKey(query.sortField))
                {
                    orderByStrJeeHR = sortableFieldsJeeHR[query.sortField] + ("desc".Equals(query.sortOrder) ? " desc" : " asc");
                }

                whereStrJeeHR = CreateWhereStrJeeHR(query, whereStrJeeHR);
            }
            if (checkusedjeehr)
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
                    var ds_jeehr = FilterLstJeeHRChucVu(list.data, query, sortableFieldsJeeHR);
                    pageModel.TotalCount = ds_jeehr.Count;
                    if (ds_jeehr.Count() == 0)
                    {
                        return await ReturnObjectGetListJobtitleIsJeeHRAsync(query, pageModel, customerid, whereStrJeeHR, orderByStrJeeHR, checkusedjeehr);
                    }
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

                    return new { data = ds_jeehr, panigator = pageModel, isJeeHR = checkusedjeehr };
                }
                else
                {
                    return await ReturnObjectGetListJobtitleIsJeeHRAsync(query, pageModel, customerid, whereStrJeeHR, orderByStrJeeHR, checkusedjeehr);
                }
            }
            else
            {
                return await ReturnObjectGetListJobtitleDefaultAsync(query, pageModel, customerid, whereStrDefault, orderByStrDefault, checkusedjeehr);
            }
        }

        private async Task<object> ReturnObjectGetListJobtitleDefaultAsync(QueryParams query, PageModel pageModel, long customerid, string whereStrDefault, string orderByStrDefault, bool checkIsJeeHR)
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

            return new { data = lst_default, panigator = pageModel, isJeeHR = checkIsJeeHR };
        }

        private async Task<object> ReturnObjectGetListJobtitleIsJeeHRAsync(QueryParams query, PageModel pageModel, long customerid, string whereStrJeeHR, string orderByStrJeeHR, bool checkIsJeeHR)
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

            return new { data = lst_jeehr, panigator = pageModel, isJeeHR = checkIsJeeHR };
        }

        private string CreateWhereStrDefault(QueryParams query, string whereStrDefault)
        {
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

        private string CreateWhereStrJeeHR(QueryParams query, string whereStrJeeHR)
        {
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

        private List<JeeHRChucVuToJeeHRFromDB> FilterLstJeeHRChucVu(List<JeeHRChucVu> lst, QueryParams query, Dictionary<string, string> sortableFieldsJeeHR)
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
            if (!string.IsNullOrEmpty(query.sortField) && sortableFieldsJeeHR.ContainsKey(query.sortField))
            {
                if (query.sortField.Equals("chucvuid"))
                {
                    if ("desc".Equals(query.sortOrder))
                    {
                        lst = lst.AsEnumerable().OrderByDescending(item => item.ID).ToList();
                    }
                }
                if (query.sortField.Equals("chucvu"))
                {
                    if ("desc".Equals(query.sortOrder))
                    {
                        lst = lst.AsEnumerable().OrderByDescending(item => item.Title).ToList();
                    }
                    else
                    {
                        lst = lst.AsEnumerable().OrderBy(item => item.Title).ToList();
                    }
                }
            }

            var list = TranferDataHelper.LstJeeHRChucVuToJeeHRFromDBFromLstJeeHRChucvu(lst);
            return list;
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

        public JobtitleModel GetJobtitle(int rowid, long CustomerID)
        {
            return _reponsitory.GetJobtitle(rowid, CustomerID);
        }

        public void UpdateJobtitle(JobtitleModel job, long CustomerID, string Username, bool isJeeHR)
        {
            _reponsitory.UpdateJobtitle(job, CustomerID, Username, isJeeHR);
        }

        public void DeleteJobtile(string DeletedBy, long customerID, int departmemntID)
        {
            _reponsitory.DeleteJobtile(DeletedBy, customerID, departmemntID);
        }
    }
}