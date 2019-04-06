using MGT_Exchange.AuthAPI.MVC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.MVC
{

    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string CompanyId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        //public DateTime Created { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string TokenAuth { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<UserApp> Users { get; set; }
    }
}
