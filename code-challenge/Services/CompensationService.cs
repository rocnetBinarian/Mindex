using challenge.Models;
using challenge.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.Services
{
    public class CompensationService : ICompensationService
    {
        private readonly ILogger<CompensationService> _logger;
        private readonly ICompensationRepository _compensationRepository;
        private readonly IEmployeeService _employeeService;

        public CompensationService(ILogger<CompensationService> logger,
            ICompensationRepository compensationRepository,
            IEmployeeService employeeService)
        {
            _logger = logger;
            _compensationRepository = compensationRepository;
            _employeeService = employeeService;
        }
        public void TryCreate(Compensation comp)
        {
            if (comp?.employee?.EmployeeId != null &&
                _employeeService.GetById(comp.employee.EmployeeId) != null)
            {
                var existingComp = _compensationRepository.GetByEmployeeId(comp.employee.EmployeeId);
                if (existingComp != default(Compensation))
                {
                    throw new ApplicationException($"Compensation for employee id {comp.employee.EmployeeId} already exists.");
                }
                _compensationRepository.Add(comp);
                _compensationRepository.SaveAsync().Wait();
            } else
            {
                throw new ArgumentException("Values must exist and be valid.");
            }
        }

        public Compensation TryGetByEmployeeId(string id)
        {
            if (id == null)
                throw new ArgumentException("No id provided");

            var existingComp = _compensationRepository.GetByEmployeeId(id);
            return existingComp;
        }
    }
}
