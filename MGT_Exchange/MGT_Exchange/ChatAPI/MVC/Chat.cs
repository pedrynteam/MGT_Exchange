using MGT_Exchange.AuthAPI.MVC;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

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

        //* All the following fields to be used as references, i.e. How many comments are unseen, how many likes, etc

        [NotMapped]
        public int CommentsInChat { get; set; }

        // To use for User References, not used as Total Chat but individual by User
        [NotMapped]
        public int UnseenForUser { get; set; }
        

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


        /*/ 1 to 1 - Steven Sandersons
        public String UserId { get; set; }
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual IdentityUser CreatedByUser { get; set; } /*/
        /*
        // 1 to 1 - Steven Sandersons
        public int ChatStatusId { get; set; }
        [ForeignKey("ChatStatusId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual ChatStatus ChatStatus { get; set; }

        // 1 to 1 - Steven Sandersons
        public int ChatKindId { get; set; }
        [ForeignKey("ChatKindId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual ChatKind ChatKind { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<Participant> Participants { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<Comment> Comments { get; set; }
        */
    }

}
