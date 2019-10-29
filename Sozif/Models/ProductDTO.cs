using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Models
{
    public class ProductDTO
    {

        public ProductDTO()
        {
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal BaseNetPrice { get; set; }
        public int TaxRateId { get; set; }

    }
}
