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
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool SeenByAll { get; set; } // Save it on Database, just to know if comment was seen by all participants

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

        // 1 to Many - Steven Sandersons
        public virtual List<CommentInfo> CommentsInfo { get; set; }

        
        /*
        // Instead of all this combinations, just add one column for Status: Sent, Delivered to All, Seen by All. (update the comment record using the users connections)
        [NotMapped]
        public int UsersTotal
        {
            get
            {
                if (this.CommentsInfo == null)
                    return -1;
                else
                    return this.CommentsInfo.Count();
            }
        }

        [NotMapped]
        public int UsersDeliveredTo
        {
            get
            {
                if (this.CommentsInfo == null)
                    return -1;
                else
                    return this.CommentsInfo.Where(x => x.Delivered == true).Count();
            }
        }

        [NotMapped]
        public int UsersSeenBy
        {
            get {
                if (this.CommentsInfo == null)
                    return -1;
                else
                    return this.CommentsInfo.Where(x => x.Seen == true).Count();
            }
        }

    */



    }
}
