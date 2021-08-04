using JeeAccount.Classes;
using JeeAccount.Controllers;
using JeeAccount.Models.Common;
using JeeAccount.Models.DepartmentManagement;
using JeeAccount.Models.JeeHR;
using JeeAccount.Reponsitories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Services.DepartmentManagement
{
    public class DepartmentManagementService : IDepartmentManagementService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;
        private readonly IDepartmentManagementReponsitory _reponsitory;
        private readonly string HOST_JEEHR_API;

        public DepartmentManagementService(IConfiguration configuration, IDepartmentManagementReponsitory reponsitory)
        {
            _config = configuration;
            _connectionString = configuration.GetValue<string>("AppConfig:Connection");
            _reponsitory = reponsitory;
            HOST_JEEHR_API = configuration.GetValue<string>("Host:JeeHR_API");
        }

        public ReturnSqlModel ChangeTinhTrang(long customerID, long RowID, string Note, long UserIdLogin)
        {
            return _reponsitory.ChangeTinhTrang(customerID, RowID, Note, UserIdLogin);
        }

        public bool CheckDepartmentExist(long CustomerID, string connectionString)
        {
            return _reponsitory.CheckDepartmentExist(CustomerID, connectionString);
        }

        public void CreateDepartment(DepartmentModel departmentModel, long CustomerID, string Username)
        {
            _reponsitory.CreateDepartment(departmentModel, CustomerID, Username);
        }

        public async Task<IEnumerable<DepartmentDTO>> GetListDepartmentDefaultAsync(long custormerID)
        {
            return await _reponsitory.GetListDepartmentDefaultAsync(custormerID).ConfigureAwait(false);
        }

        public async Task<IEnumerable<JeeHRCoCauToChucModelFromDB>> GetListDepartmentIsJeeHRtAsync(long custormerID)
        {
            return await _reponsitory.GetListDepartmentIsJeeHRtAsync(custormerID).ConfigureAwait(false);
        }

        public async Task<object> GetDSPhongBan(QueryParams query, long customerid, string token, bool isShowPage = false)
        {

            query = query == null ? new QueryParams() : query;
            PageModel pageModel = new PageModel();

            string orderByStrDefault = "DepartmentList.RowID asc";
            string orderByStrJeeHR = "AccountList.DepartmentID asc";
            string whereStrDefault = " (DepartmentList.Disable != 1 or DepartmentList.Disable is null)";
            string whereStrJeeHR = " (AccountList.Disable != 1 or AccountList.Disable is null)";

            Dictionary<string, string> sortableFieldsDefault = new Dictionary<string, string>
                        {
                            { "departmentid", "DepartmentList.RowID"},
                            { "departmentname", "DepartmentList.DepartmentName"},
                            { "tinhtrang", "DepartmentList.IsActive"},
                            { "DepartmentManager", "DepartmentList.DepartmentManager"},
                        };

            Dictionary<string, string> sortableFieldsJeeHR = new Dictionary<string, string>
                        {
                            { "departmentid", "AccountList.DepartmentID"},
                            { "departmentname", "AccountList.DepartmentName"},
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
                    var jeehrController = new JeeHRController(HOST_JEEHR_API);
                    var list = await jeehrController.GetDSCoCauToChuc(token);
                    if (list.status == 1)
                    {
                        var flat = TranferDataHelper.FlatListJeeHRCoCauToChuc(list.data);
                        flat = FilterLstFlatJeeHRCoCauToChuc(flat, query);
                        if (isShowPage)
                        {
                            pageModel.TotalCount = flat.Count;
                            if (flat.Count() == 0) throw new KhongCoDuLieuException();
                            pageModel.AllPage = (int)Math.Ceiling(flat.Count / (decimal)query.record);
                            pageModel.Size = query.record;
                            pageModel.Page = query.page;
                            if (query.more)
                            {
                                query.page = 1;
                                pageModel.AllPage = 1;
                                pageModel.Size = 1;
                                query.record = pageModel.TotalCount;
                            }
                            flat = flat.Skip((query.page - 1) * query.record).Take(query.record).ToList();
                        }

                        var obj = new
                        {
                            tree = list.data,
                            flat = flat,
                            isTree = true,
                            isJeeHR = checkusedjeehr,
                        };
                        if (!isShowPage)
                        {
                            return obj;
                        }
                        else
                        {
                            return new { data = obj, panigator = pageModel };
                        }
                    }
                    else
                    {
                        var error = JsonConvert.SerializeObject(list.error);
                        throw new JeeHRException(list.error);
                    }
                }
                else
                {
                    var depart = await _reponsitory.GetListDepartmentIsJeeHRtAsync(customerid, whereStrJeeHR, orderByStrJeeHR);
                    if (isShowPage)
                    {
                        pageModel.TotalCount = depart.Count();
                        if (depart.Count() == 0) throw new KhongCoDuLieuException();
                        pageModel.AllPage = (int)Math.Ceiling(depart.Count() / (decimal)query.record);
                        pageModel.Size = query.record;
                        pageModel.Page = query.page;
                        if (query.more)
                        {
                            query.page = 1;
                            pageModel.AllPage = 1;
                            pageModel.Size = 1;
                            query.record = pageModel.TotalCount;
                        }
                        depart = depart.Skip((query.page - 1) * query.record).Take(query.record).ToList();
                    }
                    var obj = new
                    {
                        tree = DBNull.Value,
                        flat = depart,
                        isTree = false,
                        isJeeHR = checkusedjeehr,
                    };
                    if (!isShowPage)
                    {
                        return obj;
                    }
                    else
                    {
                        return new { data = obj, panigator = pageModel };
                    }
                }
            }
            else
            {
                var depart = await _reponsitory.GetListDepartmentDefaultAsync(customerid, whereStrDefault, orderByStrDefault);
                if (isShowPage)
                {
                    pageModel.TotalCount = depart.Count();
                    if (depart.Count() == 0) throw new KhongCoDuLieuException();
                    pageModel.AllPage = (int)Math.Ceiling(depart.Count() / (decimal)query.record);
                    pageModel.Size = query.record;
                    pageModel.Page = query.page;
                    if (query.more)
                    {
                        query.page = 1;
                        pageModel.AllPage = 1;
                        pageModel.Size = 1;
                        query.record = pageModel.TotalCount;
                    }
                    depart = depart.Skip((query.page - 1) * query.record).Take(query.record).ToList();
                }
                var obj = new
                {
                    tree = DBNull.Value,
                    flat = depart,
                    isTree = false,
                    isJeeHR = checkusedjeehr,
                };
                if (!isShowPage)
                {
                    return obj;
                }
                else
                {
                    return new { data = obj, panigator = pageModel };
                }
            }
        }

        private string CreateWhereStrDefault(QueryParams query)
        {
            string whereStrDefault = "";
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                whereStrDefault += $" and (AccountList.LastName + ' ' + AccountList.FirstName like N'%{query.filter["keyword"]}%' " +
 $"or DepartmentList.DepartmentName like N'%{query.filter["keyword"]}%')";
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                whereStrDefault += $" and (DepartmentList.DepartmentName like N'%{query.filter["phongban"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                whereStrDefault += $" and (DepartmentList.DepartmentID  = {query.filter["phongbanid"]}) ";
            }

            if (!string.IsNullOrEmpty(query.filter["dakhoa"]))
            {
                if (Convert.ToBoolean(query.filter["dakhoa"]))
                {
                    whereStrDefault += $" and (DepartmentList.IsActive = 0) ";
                }
            }
            return whereStrDefault;
        }

        private string CreateWhereStrJeeHR(QueryParams query)
        {
            string whereStrJeeHR = "";
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                whereStrJeeHR += $" and AccountList.DepartmentName like N'%{query.filter["keyword"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                whereStrJeeHR += $" and (AccountList.DepartmentName like N'%{query.filter["phongban"]}%') ";
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                whereStrJeeHR += $" and (AccountList.DepartmentID  = {query.filter["phongbanid"]}) ";
            }
            return whereStrJeeHR;
        }

        private List<FlatJeeHRCoCauToChucModel> FilterLstFlatJeeHRCoCauToChuc(List<FlatJeeHRCoCauToChucModel> lst, QueryParams query)
        {
            if (!string.IsNullOrEmpty(query.filter["keyword"]))
            {
                lst = lst.AsEnumerable().Where(item => item.Title.Contains(query.filter["keyword"])).ToList();
            }

            if (!string.IsNullOrEmpty(query.filter["phongban"]))
            {
                lst = lst.AsEnumerable().Where(item => item.Title.Contains(query.filter["phongban"])).ToList();
            }

            if (!string.IsNullOrEmpty(query.filter["phongbanid"]))
            {
                lst = lst.AsEnumerable().Where(item => item.RowID.ToString() == query.filter["phongbanid"]).ToList();
            }
            return lst;
        }
    }
}