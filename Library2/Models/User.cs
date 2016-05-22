using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Library2.Models
{
    public class User
    {        
        public int ID { get; set; }
        [Required(ErrorMessage = "Input the E-mail!")]
        [EmailAddress]
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
    }
}