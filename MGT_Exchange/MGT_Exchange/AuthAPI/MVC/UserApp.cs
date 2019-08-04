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

    public class userApp
    {        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string userAppId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string loginTokenId { get; set; } // Use it to Identify 

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userName { get; set; }
        public string nickname { get; set; }
        public string alias { get; set; } // htr003 - For name duplications
        
        public string email { get; set; }
        public string password { get; set; }
        public string tokenAuth { get; set; }

        public DateTime lastSeen { get; set; }

        public bool active { get; set; } // To know that the user is Active or not. So dont send him notifications, messages, etc, etc.

        // 1 to 1 - Steven Sandersons
        public string companyId { get; set; }
        [ForeignKey("companyId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual company company { get; set; }

        // 1 to 1 - Steven Sandersons
        public int? departmentId { get; set; }
        [ForeignKey("departmentId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual department department { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<groupof> groups { get; set; }

    }
        
}
