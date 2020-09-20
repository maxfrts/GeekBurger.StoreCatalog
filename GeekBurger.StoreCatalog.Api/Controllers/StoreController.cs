using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GeekBurger.StoreCatalog.Api.Controllers
{
    [Route("api/store")]
    [Produces("application/json")]
    public class StoreController : ControllerBase
    {
        public StoreController() { }

        [HttpGet]
        public async Task<ActionResult> GetStoreAsync()
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
