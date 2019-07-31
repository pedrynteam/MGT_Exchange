using GreenDonut;
using HotChocolate.Resolvers;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions.Resources
{
    public class UserAppDataLoader : DataLoaderBase<string, userApp>
    {
        private readonly MVCDbContext _context;

        public UserAppDataLoader(MVCDbContext context)
          : base(new DataLoaderOptions<string>())
        {
            _context = context;
        }

        public virtual IReadOnlyList<userApp> Lines
        {
            get { return _context.UserApp.ToList(); }
        }

        /*
        public Task<Person> GetPerson(string id, IResolverContext context, [Service]IPersonRepository repository)
        {
            return context.BatchDataLoader<string, Person>("personByIdBatch", keys => repository.GetPersonBatchAsync(keys)).LoadAsync(id);
        }
        */

        public Task<userApp> GetPerson(string id, IResolverContext context)
        {
            
            return context.BatchDataLoader<string, userApp>("personByIdBatch", async keys => await _context.UserApp.ToDictionaryAsync(mc => mc.userAppId) ).LoadAsync(id);
        }

        protected override async Task<IReadOnlyList<Result<userApp>>> FetchAsync(
                    IReadOnlyList<string> keys,
                    CancellationToken cancellationToken)
        {
            List<GreenDonut.Result<userApp>> te = new List<Result<userApp>>();
            Result<userApp> uno = await _context.UserApp.FirstOrDefaultAsync();
            te.Add(uno);

            //List<Result<UserApp>> dos = await _context.UserApp.ToListAsync();

            return te;

            // return await _context.UserApp.FindAsync(keys);

        }

        /*
        protected override async Task<IReadOnlyList<Result<UserApp>>> FetchAsync(
            IReadOnlyList<string> keys,
            CancellationToken cancellationToken)
        {
            
            IReadOnlyList<Result<UserApp>> test;            
            return await test;

        }
        
        /*
        protected override async Task<UserApp> FetchAsync(
            IReadOnlyList<string> keys,
            CancellationToken cancellationToken)
        {
            var info = await _context.UserApp.FindAsync(keys);

            return await _context.UserApp.FindAsync(keys);

            //return await _context.UserApp.FindAsync("hola")
        }
        */
    }
}
