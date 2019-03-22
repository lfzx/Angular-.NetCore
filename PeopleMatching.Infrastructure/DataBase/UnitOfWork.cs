using PeopleMatching.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PeopleMatching.Infrastructure.DataBase
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyContext _mycontext;

        public UnitOfWork(MyContext mycontext)
        {
            _mycontext = mycontext;
        }

        public async Task<bool> SaveAsync()
        {
            return await _mycontext.SaveChangesAsync() > 0;
        }
    }
}
