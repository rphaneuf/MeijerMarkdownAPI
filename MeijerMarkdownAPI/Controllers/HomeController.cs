using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Data.Models;
using Core;

namespace MeijerMarkdownAPI.Controllers
{
    public class HomeController : Controller
    {
        public string GetItems(string UPC)
        {
            MeijerMarkdownCode myClass = new MeijerMarkdownCode();
            myClass.ExtractData(UPC);

            IList<Data.Models.Data> products = new List<Data.Models.Data>
            {
                new Data.Models.Data
                {
                    Description = myClass.Description,
                    Price = myClass.Price,
                    UnitOM = myClass.UnitOM.ToString().Trim()
                }
            };

            return Newtonsoft.Json.JsonConvert.SerializeObject(products);
        }       
    }
}
