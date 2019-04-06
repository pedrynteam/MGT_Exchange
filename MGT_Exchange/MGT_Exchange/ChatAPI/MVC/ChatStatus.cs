using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.MVC
{
    public class ChatStatus
    {
        [Key]
        public int ChatStatusId { get; set; }

        public string Name { get; set; } // OPEN, CLOSED, Rejected In english should be used for translation

        public string Description { get; set; }

    }
}
