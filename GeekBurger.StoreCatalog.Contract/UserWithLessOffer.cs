using System;
using System.Collections.Generic;
using System.Text;

namespace GeekBurger.StoreCatalog.Contract
{
    public class UserWithLessOffer
    {
        public int UserId { get; set; }

        public string[] Restrictions { get; set; }
    }
}
