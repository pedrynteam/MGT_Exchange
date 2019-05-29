using MGT_Exchange.AuthAPI.MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MGT_Exchange.ChatAPI.MVC
{
    // One to One or One to Many Chat. Just chat.
    public class Chat
    {
        [Key]
        public int ChatId { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool Closed { get; set; }
        public DateTime? ClosedAt { get; set; }

        // 1 to 1 - Steven Sandersons
        public string CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual Company Company { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<Participant> Participants { get; set; }
        // public virtual List<UserApp> Users { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<Comment> Comments { get; set; }

        /*
        public string Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime Closed { get; set; }

        public string Picture { get; set; }

        /* Fix foreign keys on delete
        // 1 to 1 - Steven Sandersons * UserApp in MVCDbContext, User Account Context in ApplicationDB this for query simplicity and Database security
        public int UserAppId { get; set; }
        [ForeignKey("UserAppId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual UserApp CreatedBy { get; set; }
        */

    }

}
