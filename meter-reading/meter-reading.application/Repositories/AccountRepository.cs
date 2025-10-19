using meter_reading.Domain.Entities;
using meter_reading.Domain.Interfaces;
using System.Linq.Expressions;
using web_api_db.Infrastructure.Database;

namespace web_api_db.Core.Repository
{
    public class AccountRepository : IRepository<Account>
    {
        private readonly DatabaseContext Context;

        public AccountRepository(DatabaseContext context)
        {
            Context = context;
        }

        public async Task AddAsync(Account entity)
        {
            throw new NotImplementedException();
        }

        public async Task AddRangeAsync(ICollection<Account> entities)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Account> Get(Expression<Func<Account, bool>> predicate)
        {
            return Context.Accounts.Where(predicate);
        }

        public Task<Account?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }
    }
}
