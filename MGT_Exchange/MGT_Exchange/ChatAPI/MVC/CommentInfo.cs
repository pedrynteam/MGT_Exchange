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
    public class commentInfo
    {
        [Key]
        public int commentInfoId { get; set; }

        public DateTime createdAt { get; set; }

        public bool delivered { get; set; }

        public DateTime? deliveredAt { get; set; }

        public bool seen { get; set; }

        public DateTime? seenAt { get; set; }

        // 1 to 1 - Steven Sandersons
        public string userAppId { get; set; }
        [ForeignKey("userAppId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual userApp user { get; set; }

        // 1 to Many - Steven Sandersons
        public int commentId { get; set; }
        [ForeignKey("commentId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual comment comment { get; set; }

    }
}
