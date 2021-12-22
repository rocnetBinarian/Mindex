using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using challenge.Data;
using challenge.Models;

namespace challenge.Repositories
{
    public class CompensationRepository : ICompensationRepository
    {
        private readonly EmployeeContext _employeeContext;
        private readonly ILogger<ICompensationRepository> _logger;

        public CompensationRepository(EmployeeContext employeeContext, ILogger<ICompensationRepository> logger)
        {
            _employeeContext = employeeContext;
            _logger = logger;
        }

        public void Add(Compensation comp)
        {
            _employeeContext.Compensations.Add(comp);
        }

        public Compensation GetByEmployeeId(string id)
        {
            return _employeeContext.Compensations.SingleOrDefault(x => x.employee.EmployeeId == id);
        }

        public Task SaveAsync()
        {
            return _employeeContext.SaveChangesAsync();
        }
    }
}
