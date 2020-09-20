using System;
using System.Collections.Generic;

namespace GeekBurger.StoreCatalog.Contract.Requests
{

    public class ProductsRequest
    {
        public string StoreName { get; set; }

        public int UserId { get; set; }

        public string[] Restrictions { get; set; }
    }
}
