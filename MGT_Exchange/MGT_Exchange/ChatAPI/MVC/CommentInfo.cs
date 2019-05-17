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
    public class CommentInfo
    {
        [Key]
        public int CommentInfoId { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Delivered { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public bool Seen { get; set; }

        public DateTime? SeenAt { get; set; }

        // 1 to 1 - Steven Sandersons
        public string UserAppId { get; set; }
        [ForeignKey("UserAppId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual UserApp User { get; set; }

        // 1 to Many - Steven Sandersons
        public int CommentId { get; set; }
        [ForeignKey("CommentId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual Comment Comment { get; set; }

    }
}
