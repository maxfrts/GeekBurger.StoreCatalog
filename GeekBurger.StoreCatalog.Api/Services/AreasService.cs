using GeekBurger.StoreCatalog.Api.Services.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeekBurger.Production.Contract;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace GeekBurger.StoreCatalog.Api.Services
{
    public class AreasService : IAreasService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly string _cacheName = "productionAreas";
        private readonly string _baseUrlProductionAreas;

        public AreasService(IMemoryCache memoryCache, IConfiguration configuration)
        {
            _memoryCache = memoryCache;
            _baseUrlProductionAreas = configuration.GetValue<string>("ProductionAreaApiBaseUrl");
        }


        public async Task<List<Production.Contract.Production>> GetProductionAreas()
        {
            List< Production.Contract.Production> areas = new List<Production.Contract.Production>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(_baseUrlProductionAreas + "/api/Production/areas"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        areas = JsonConvert.DeserializeObject<List<Production.Contract.Production>>(apiResponse);
                    }
                }
            }

            return areas;
        }

        public void SaveAreas(List<Production.Contract.Production> areas)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddDays(1)
            };
            
            _memoryCache.Set(_cacheName, areas, cacheOptions);
        }

        public void SaveArea(Production.Contract.Production area)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddDays(1)
            };

            var areas = GetAreasFromMemory();

            //Identify if area is already registered
            var areaToRemove = areas.Where(a => a.ProductionId == area.ProductionId).FirstOrDefault();
            if (areaToRemove != null)
            {
                //if area found remove it from the list to insert the updated registry
                areas.Remove(areaToRemove);
            }

            //Insert area into the list and save
            areas.Add(area);
            _memoryCache.Set(_cacheName, areas, cacheOptions);
        }

        public List<Production.Contract.Production> GetAreasFromMemory() 
        {
            _memoryCache.TryGetValue(_cacheName, out List<Production.Contract.Production> areas);

            return areas;
        }

    }
}
