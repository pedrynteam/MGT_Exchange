using MGT_Exchange.AuthAPI.MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.MVC
{
    public class comment
    {
        [Key]
        public int commentId { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string message { get; set; }

        public DateTime? createdAt { get; set; }

        public bool seenByAll { get; set; } // Save it on Database, just to know if comment was seen by all participants

        // 1 to Many - Steven Sandersons
        public int chatId { get; set; }
        [ForeignKey("chatId")]        
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual chat chat { get; set; }

        // 1 to 1 - Steven Sandersons
        public string userAppId { get; set; }
        [ForeignKey("userAppId")]        
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual userApp user { get; set; }

        // 1 to Many - Steven Sandersons
        public virtual List<commentInfo> commentsInfo { get; set; }

        

    }
}
