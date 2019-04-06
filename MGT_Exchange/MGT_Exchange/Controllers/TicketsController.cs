using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MGT_Exchange.Models;
using MGT_Exchange.TicketAPI.MVC;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.AuthAPI.MVC;

namespace MGT_Exchange.Controllers
{
    public class TicketsController : Controller
    {
        private readonly MVCDbContext _context;

        public TicketsController(MVCDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {


            // Create Chat conversation
            /*
            
            Chat chat = new Chat { ChatId = 0, Name = "Chat 2" };

            UserApp user = new UserApp { UserAppId = 0, Id = "third", Email = "yothird@here.com", PasswordHash = "Entrar82#" };

            Participant participant = new Participant { ParticipantId = 0, ChatId = 2, UserAppId = 3, IsAdmin = true };

            Comment comment = new Comment { CommentId = 0, ChatId = 2, UserAppId = 3, Message = "Comment 3 Chat 2"};

            //_context.Chat.Add(chat);
            //_context.UserApp.Add(user);
            //_context.Participant.Add(participant);
            _context.Comment.Add(comment);
            */


            

            _context.ChatKind.Add(new ChatKind { ChatKindId = 0, Name = "Chat Room", Description="Chat Room" });
            _context.ChatKind.Add(new ChatKind { ChatKindId = 0, Name = "Chat Notification", Description = "Chat Notification" });

            _context.ChatStatus.Add(new ChatStatus { ChatStatusId = 0, Name = "OPEN", Description = "OPEN" });
            _context.ChatStatus.Add(new ChatStatus { ChatStatusId = 0, Name = "CANCEL", Description = "CANCEL" });
            _context.ChatStatus.Add(new ChatStatus { ChatStatusId = 0, Name = "RESOLVED", Description = "RESOLVED" });
    

            //_context.UserApp.Add(new UserApp { UserAppId = 0, UserName = "username" });

            await _context.SaveChangesAsync();


            

            return View(await _context.Ticket.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,Name")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,Name")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.TicketId == id);
        }
    }
}
