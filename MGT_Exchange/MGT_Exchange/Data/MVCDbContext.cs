using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MGT_Exchange.TicketAPI.MVC;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MGT_Exchange.Models
{
    public class MVCDbContext : DbContext
    {
        private static bool _created = false;
        public MVCDbContext (DbContextOptions<MVCDbContext> options)
            : base(options)
        {
            /*
            Database.EnsureDeleted();
            _created = false; // */

            if (!_created)
            {
                _created = true;
                Database.EnsureCreated();
            }
        }

        // Error: No database provider has been configured for this DbContext. 
        // A provider can be configured by overriding the DbContext.OnConfiguring method 
        // or by using AddDbContext on the application service provider. If AddDbContext is used, 
        // then also ensure that your DbContext type accepts a DbContextOptions<TContext> object in 
        // its constructor and passes it to the base constructor for DbContext.
        public MVCDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                /*
                optionsBuilder.UseSqlite("Data Source = MVCDb.db");
                */

                IConfigurationRoot configuration = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();

                var connectionString = configuration.GetConnectionString("MVCDbContext");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public DbSet<MGT_Exchange.TicketAPI.MVC.Ticket> Ticket { get; set; }

        public DbSet<MGT_Exchange.TicketAPI.MVC.CommentTicket> CommentTicket { get; set; }
    }
}
