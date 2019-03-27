using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MGT_Exchange.Models;
using MGT_Exchange.TicketAPI.MVC;

namespace MGT_Exchange.Controllers
{
    public class CommentTicketsController : Controller
    {
        private readonly MVCDbContext _context;

        public CommentTicketsController(MVCDbContext context)
        {
            _context = context;
        }

        // GET: CommentTickets
        public async Task<IActionResult> Index()
        {
            var mVCDbContext = _context.CommentTicket.Include(c => c.Ticket);
            return View(await mVCDbContext.ToListAsync());
        }

        // GET: CommentTickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentTicket = await _context.CommentTicket
                .Include(c => c.Ticket)
                .FirstOrDefaultAsync(m => m.commentTicketId == id);
            if (commentTicket == null)
            {
                return NotFound();
            }

            return View(commentTicket);
        }

        // GET: CommentTickets/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Ticket, "TicketId", "Name");
            return View();
        }

        // POST: CommentTickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("commentTicketId,Message,TicketId")] CommentTicket commentTicket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(commentTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "TicketId", "Name", commentTicket.TicketId);
            return View(commentTicket);
        }

        // GET: CommentTickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentTicket = await _context.CommentTicket.FindAsync(id);
            if (commentTicket == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Ticket, "TicketId", "Name", commentTicket.TicketId);
            return View(commentTicket);
        }

        // POST: CommentTickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("commentTicketId,Message,TicketId")] CommentTicket commentTicket)
        {
            if (id != commentTicket.commentTicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(commentTicket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentTicketExists(commentTicket.commentTicketId))
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
            ViewData["TicketId"] = new SelectList(_context.Ticket, "TicketId", "Name", commentTicket.TicketId);
            return View(commentTicket);
        }

        // GET: CommentTickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentTicket = await _context.CommentTicket
                .Include(c => c.Ticket)
                .FirstOrDefaultAsync(m => m.commentTicketId == id);
            if (commentTicket == null)
            {
                return NotFound();
            }

            return View(commentTicket);
        }

        // POST: CommentTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commentTicket = await _context.CommentTicket.FindAsync(id);
            _context.CommentTicket.Remove(commentTicket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentTicketExists(int id)
        {
            return _context.CommentTicket.Any(e => e.commentTicketId == id);
        }
    }
}
