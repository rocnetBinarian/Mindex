using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using challenge.Models;
using Microsoft.Extensions.Logging;
using challenge.Repositories;

namespace challenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        /*
         * This reeks of having a bug.  Between the entity tracking and the removal of the originalEmployee (regardless
         *   of whether or not the new employee is null), I don't like this.
         * */
        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        /// <summary>
        /// See <see cref="challenge.Services"/> for notes on this method.
        /// 
        /// See README.txt for assumptions that determined implementation design.
        /// Said assumptions may not necessarily exist in the real world.
        /// </summary>
        /// <param name="startNode">Starting node for recursion</param>
        /// <returns>ReportingStructure denoting the number of reports under them (recursive)</returns>
        public ReportingStructure BuildReportingStructure(Employee startNode)
        {
            var subordinates = startNode.DirectReports;
            ReportingStructure rtn = new ReportingStructure()
            {
                employee = startNode
            };
            if (subordinates == null)
                return rtn;
            foreach(var s in subordinates)
            {
                var recRtn = BuildReportingStructure(s);

                rtn.numberOfReports += recRtn.numberOfReports;
            }
            rtn.numberOfReports += subordinates.Count;
            return rtn;
        }
    }
}
