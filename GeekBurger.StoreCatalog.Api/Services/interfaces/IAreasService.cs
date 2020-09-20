using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeekBurger.StoreCatalog.Api.Services.interfaces
{
    public interface IAreasService
    {
        Task<List<Production.Contract.Production>> GetProductionAreas();

        void SaveAreas(List<Production.Contract.Production> areas);

        void SaveArea(Production.Contract.Production area);

        List<Production.Contract.Production> GetAreasFromMemory();
    }
}
