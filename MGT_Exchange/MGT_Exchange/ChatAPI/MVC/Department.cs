using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.MVC
{
    public class department
    {
        [Key]
        public int departmentId { get; set; }

        [Required]
        public string name { get; set; }

        public string description { get; set; }
    }
}
