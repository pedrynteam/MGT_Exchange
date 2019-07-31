using MGT_Exchange.AuthAPI.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.MVC
{

    public class company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string companyId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        
        public string loginTokenId { get; set; } // Use it to Identify 

        public string name { get; set; }

        public string description { get; set; }

        public DateTime createdAt { get; set; }

        public string email { get; set; }
        public string password { get; set; }

        public string tokenAuth { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<userApp> users { get; set; }
    }
}
