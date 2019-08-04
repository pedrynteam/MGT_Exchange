using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MGT_Exchange.TicketAPI.MVC;
using Microsoft.Extensions.Configuration;
using System.IO;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.AuthAPI.MVC;

namespace MGT_Exchange.Models
{
    public class MVCDbContext : DbContext
    {
        private static bool _created = false;
        public MVCDbContext (DbContextOptions<MVCDbContext> options)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            // To ignore the id to be ignored since that columns is not a Key
            modelBuilder.Entity<company>().Property(x => x.id).Metadata.AfterSaveBehavior = Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore;

            modelBuilder.Entity<userApp>().Property(x => x.id).Metadata.AfterSaveBehavior = Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore;

            //model
            /*
            modelBuilder.Entity<Company>().HasKey(u => new
            {
                u.CompanyId,
                u.Id
            });

            /*

            modelBuilder.Entity<Posts>()
          .HasOne(p => p.User)
          .WithMany(c => c.Posts)
          .HasForeignKey(p => p.UserId);

      modelBuilder.Entity<Attachment>()
        .HasOne(p => p.Posts)
        .WithMany(c => c.Attachment);
            */

            /* fix foreign keys
            modelBuilder.Entity<Comment>()
                        .HasOne(c => c.User)
                        .WithOne()
                        .HasForeignKey<UserApp>(q => q.UserAppId).OnDelete(DeleteBehavior.Restrict)
                        ;

            modelBuilder.Entity<Participant>()
                        .HasOne(c => c.User)
                        .WithOne()
                        .HasForeignKey<UserApp>(q => q.UserAppId).OnDelete(DeleteBehavior.Restrict)
                        ;
            
            modelBuilder.Entity<Chat>()
                        .HasOne(c => c.CreatedBy)
                        .WithOne()
                        .HasForeignKey<UserApp>(q => q.UserAppId).OnDelete(DeleteBehavior.Restrict)
                        ;


            /*
            modelBuilder.Entity<Comment>()                
                .HasOne(a => a.UserApp)
                .WithOne(b => b.Author)
                .HasForeignKey<user>(b => b.AuthorRef).OnDelete(do;
                */
        }

        /*
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.OneToManyCascadeDeleteConvention>();
        }
        */

        public DbSet<MGT_Exchange.TicketAPI.MVC.Ticket> Ticket { get; set; }

        public DbSet<MGT_Exchange.TicketAPI.MVC.CommentTicket> CommentTicket { get; set; }

        public DbSet<MGT_Exchange.ChatAPI.MVC.chat> Chat { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.ChatKind> ChatKind { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.ChatStatus> ChatStatus { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.comment> Comment { get; set; }        
        public DbSet<MGT_Exchange.ChatAPI.MVC.participant> Participant { get; set; }
        public DbSet<MGT_Exchange.AuthAPI.MVC.userApp> UserApp { get; set; }
        public DbSet<MGT_Exchange.AuthAPI.MVC.company> Company { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.commentInfo> CommentInfo { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.notification> Notification { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.department> Department { get; set; }
        public DbSet<MGT_Exchange.ChatAPI.MVC.groupof> Group { get; set; }

    }
}
