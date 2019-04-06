using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.MVC
{
    public class ChatKind
    {
        [Key]
        public int ChatKindId { get; set; }

        public string Name { get; set; } // NoticeOnly, ChatOnly, Project In english should be used for translation

        public string Description { get; set; }

    }
}
