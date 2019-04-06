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
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required]
        [Display(Name = "IsAdmin")]
        public bool IsAdmin { get; set; }

        // 1 to Many - Steven Sandersons
        public int ChatId { get; set; }
        [ForeignKey("ChatId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual Chat Chat { get; set; }

        // 1 to 1 - Steven Sandersons
        public string UserAppId { get; set; }
        [ForeignKey("UserAppId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual UserApp User { get; set; }
    }
}
