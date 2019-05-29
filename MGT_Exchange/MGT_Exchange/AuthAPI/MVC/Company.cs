using MGT_Exchange.AuthAPI.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.MVC
{

    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string CompanyId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string LoginTokenId { get; set; } // Use it to Identify 

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string TokenAuth { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<UserApp> Users { get; set; }
    }
}
