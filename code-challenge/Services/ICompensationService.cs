using challenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.Services
{
    /// <summary>
    /// This didn't strictly need to be its own service.  But, since the point
    /// of this exercise is to look at dependency injection, why not.  Additionally,
    /// it helps keep things separated and organized.
    /// </summary>
    public interface ICompensationService
    {
        Compensation TryGetByEmployeeId(string id);
        void TryCreate(Compensation comp);
    }
}
