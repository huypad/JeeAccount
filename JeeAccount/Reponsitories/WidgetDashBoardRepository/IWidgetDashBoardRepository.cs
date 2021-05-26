using JeeAccount.Models;
using JeeAccount.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JeeAccount.Reponsitories
{
    public interface IWidgetDashBoardRepository
    {
        Task<IEnumerable<Widget>> GetAll();

        Task<WidgetDashBoardModel> GetByID(long ID_NV, long WidgetId);

        Task<ReturnSqlModel> Update(WidgetDashBoardModel widget);

        Task<ReturnSqlModel> Create(WidgetDashBoardModel widget);

        Task<ReturnSqlModel> Delete(long ID_NV, long WidgetId);
    }
}