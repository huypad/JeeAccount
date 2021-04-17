using DpsLibs.Data;
using JeeAccount.Models.Common;
using JeeAccount.Reponsitories;
using JeeAccount.Classes;
using JeeAccount.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JeeAccount.Controllers
{
    [EnableCors("AllowOrigin")]
    [Route("api/widgetdashboard")]
    [ApiController]
    public class WidgetDashBoardController: ControllerBase
    {
        private readonly IConfiguration _config;

        private readonly IWidgetDashBoardRepository repository;

        public WidgetDashBoardController(IConfiguration configuration)
        {
            _config = configuration;
            this.repository = new WidgetDashBoardRepository(_config.GetConnectionString("DefaultConnection"));
        }

        [HttpGet("Get_DSWidget")]
        public async Task<BaseModel<object>> GetAll()
        {
            try {
                var widgets = await repository.GetAll();
                return JsonResultCommon.ThanhCong(widgets);
            }
            catch (Exception ex) {
                return JsonResultCommon.Exception(ex);
            }
            
        }

        [HttpGet("Get/WidgetId={WidgetId}")]
        public async Task<BaseModel<object>> GetById(long WidgetId)
        { 
            try {
                var widget = await repository.GetByID(0, WidgetId);
                if (widget is null) {
                    return JsonResultCommon.KhongTonTai("widget");
                }
                return JsonResultCommon.ThanhCong(widget);
            }
            catch (Exception ex) {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("Post_UpdateWidget")]
        public async Task<BaseModel<object>> Update([FromBody] WidgetDashBoardModel widget)
        {
            try {
                if (widget is null) {
                    return JsonResultCommon.BatBuoc("widget");
                }
                ReturnSqlModel update = await repository.Update(widget);
                if (!update.Susscess) {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST)) {
                        return JsonResultCommon.KhongTonTai("widget");
                    }
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION)) {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return JsonResultCommon.ThatBai(update.ErrorMessgage);
                    }
                }
                return JsonResultCommon.ThanhCong(widget);
            } catch (Exception ex) {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpPost("Create_Widget")]
        public async Task<BaseModel<object>> Create([FromBody] WidgetDashBoardModel widget)
        {
            try {
                if (widget is null) {
                    return JsonResultCommon.BatBuoc("widget");
                }
                ReturnSqlModel update = await repository.Create(widget);
                if (!update.Susscess) {
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST)) {
                        return JsonResultCommon.Trung("widget");
                    }
                    if (update.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION)) {
                        // TODO: bổ sung ghi log sau
                        string logMessage = update.ErrorMessgage;

                        return JsonResultCommon.ThatBai(update.ErrorMessgage);
                    }
                }
                return JsonResultCommon.ThanhCong(widget);
            }
            catch (Exception ex) {
                return JsonResultCommon.Exception(ex);
            }
        }

        [HttpGet("Delete/WidgetId={WidgetId}")]
        public async Task<BaseModel<object>> Delete(long WidgetId)
        {
            try {
                ReturnSqlModel delete = await repository.Delete(0, WidgetId);
                if (!delete.Susscess) {
                    if (delete.ErrorCode.Equals(Constant.ERRORCODE_NOTEXIST)) {
                        return JsonResultCommon.KhongTonTai("widget");
                    }
                    if (delete.ErrorCode.Equals(Constant.ERRORCODE_EXCEPTION)) {
                        // TODO: bổ sung ghi log sau
                        string logMessage = delete.ErrorMessgage;
                        return JsonResultCommon.ThatBai(delete.ErrorMessgage);
                    }
                }
                return JsonResultCommon.ThanhCong();
            }
            catch (Exception ex) {
                return JsonResultCommon.Exception(ex);
            }
        }
    }
}
