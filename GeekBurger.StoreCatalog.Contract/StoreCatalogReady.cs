using System;

namespace GeekBurger.StoreCatalog.Contract
{
    /// <summary>
    /// Entity expected as response of api/store request
    /// </summary>
    public class StoreCatalogReady
    {
        /// <summary>
        /// Store Unique Identifier
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Indicates if store is ready or not
        /// </summary>
        public bool Ready { get; set; }
    }
}
