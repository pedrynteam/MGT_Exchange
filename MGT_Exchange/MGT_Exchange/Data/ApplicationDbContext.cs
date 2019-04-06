using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MGT_Exchange.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private static bool _created = false;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            /*Database.EnsureDeleted();
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
        public ApplicationDbContext()
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

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
