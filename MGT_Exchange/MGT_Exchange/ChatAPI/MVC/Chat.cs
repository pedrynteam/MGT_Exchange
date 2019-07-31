using MGT_Exchange.AuthAPI.MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MGT_Exchange.ChatAPI.MVC
{
    // One to One or One to Many Chat. Just chat.
    public class chat
    {
        [Key]
        public int chatId { get; set; }

        public string type { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public DateTime createdAt { get; set; }

        public DateTime? updatedAt { get; set; }

        public bool closed { get; set; }
        public DateTime? closedAt { get; set; }

        // 1 to 1 - Steven Sandersons
        public string companyId { get; set; }
        [ForeignKey("companyId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual company company { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<participant> participants { get; set; }
        // public virtual List<UserApp> Users { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<comment> comments { get; set; }

    }

}
