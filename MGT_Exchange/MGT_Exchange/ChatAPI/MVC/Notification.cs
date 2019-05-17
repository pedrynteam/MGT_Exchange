using HotChocolate.Types;
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
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string Type { get; set; } // NewMessage, NewChat * Enum
        public string Title { get; set; } // Game Request * Translate
        public string Subtitle { get; set; } // Five Cards Draw * Translate
        public string Body { get; set; } // Bob wants to play poker
        public string Message { get; set; } // Hello!, New Photo
        
        /*
        // 1 to 1 - Steven Sandersons
        public string FromUserAppId { get; set; }
        [ForeignKey("FromUserAppId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual UserApp FromUserApp { get; set; } // I think this should be nullable
        */
        
        // 1 to 1 - Steven Sandersons
        public string ToUserAppId { get; set; }
        //[ForeignKey("UserAppId")]
        [ForeignKey("ToUserAppId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual UserApp ToUserApp { get; set; }

        // To go to the correct screen or detailed page
        public string Route { get; set; }
        public string RouteAction { get; set; }
        public string RouteId { get; set; }

        // To continue showing or not the notification. Change Seen when the user sees the event. i.e. Go into Chat
        public DateTime CreatedAt { get; set; }
        public bool Seen { get; set; }
        public DateTime? SeenAt { get; set; }

    }







}
