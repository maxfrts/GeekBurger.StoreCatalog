using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using GeekBurger.StoreCatalog.Api.Services.interfaces;
using GeekBurger.StoreCatalog.Api.Services.ServiceBus.Topic;
using GeekBurger.StoreCatalog.Contract.Requests;
using GeekBurger.StoreCatalog.Contract.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;

namespace GeekBurger.StoreCatalog.Api.Controllers
{
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _products;
        private readonly IMapper _mapper;
        private readonly ITopicBus _topicBus;

        public ProductsController(IProductsService productService, IMapper mapper, ITopicBus topicBus)
        {
            _products = productService;
            _mapper = mapper;
            _topicBus = topicBus;
        }

        [HttpGet]
        [Route("api/products")]
        [EnableCors("AllowOrigin")]
        public async Task<ActionResult> GetProductsAsync([FromQuery] ProductsRequest productRequest)
        {
            try
            {
                var products = await _products.GetProductsAsync(productRequest);

                if (products != null)
                    return Ok(products);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {

                return BadRequest(ex);
            }
        }
    }
}
