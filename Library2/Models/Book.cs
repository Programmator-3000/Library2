using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace Library2.Models
{
    public class Book
    {
        [Required(ErrorMessage = "Inpuе the name!")]
        public string Name { get; set; }

        public int ID { get; set; }

        [Required(ErrorMessage = "Inpuе the quantity!")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Inpuе the author`s name!")]
        public string Author { get; set; }

    }
}