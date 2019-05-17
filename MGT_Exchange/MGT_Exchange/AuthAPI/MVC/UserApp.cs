using MGT_Exchange.ChatAPI.MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.MVC
{

    public class UserApp
    {        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserAppId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TokenAuth { get; set; }

        public DateTime LastSeen { get; set; }

        // 1 to 1 - Steven Sandersons
        public string CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual Company Company { get; set; }
    }
    
    /*
    public class UserApp
    {
        [Key]
        public int UserAppId { get; set; }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string TokenAuth { get; set; }

        // 1 to 1 - Steven Sandersons
        public string CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual Company Company { get; set; }
    }
    */
}
