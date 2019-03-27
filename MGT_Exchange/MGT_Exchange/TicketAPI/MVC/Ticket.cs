using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.TicketAPI.MVC
{    
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        
        // 1 to Many - Steven Sandersons
        public virtual List<CommentTicket> Comments { get; set; }

    }

    // Class start with Uppercase, fields with lowercase
    public class CommentTicket
    {
        [Key]
        public int commentTicketId { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }

        // 1 to Many - Steven Sandersons
        public int TicketId { get; set; }
        [ForeignKey("TicketId")]
        [JsonIgnore] // To avoid circular calls. Customer -> Order -> Customer -> Order
        public virtual Ticket Ticket { get; set; }
    }

}
