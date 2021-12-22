using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using challenge.Models;

namespace challenge.Repositories
{
    public interface ICompensationRepository
    {
        Compensation GetByEmployeeId(string id);
        void Add(Compensation comp);
        Task SaveAsync();
    }
}
