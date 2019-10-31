using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Sozif.Models
{
    public class InvoiceDTO
    {

        public InvoiceDTO()
        {
            Orders = new List<KeyValuePair<int, bool>>();
        }

        public string CustomerName { get; set; }
        public string CustomerNip { get; set; }
        public string CustomerAddress { get; set; }
        public int DaysToPay { get; set; }

        [BindProperty]
        public List<KeyValuePair<int, bool>> Orders { get; set; }

    }
}
