using challenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace challenge.Services
{
    public interface IEmployeeService
    {
        Employee GetById(String id);
        Employee Create(Employee employee);
        Employee Replace(Employee originalEmployee, Employee newEmployee);

        /// <summary>
        /// This doesn't strictly need to be here, and an argument could be made
        /// that it shouldn't be, since it doesn't have to interact with the db at all.
        /// But, this is also still an employee action, and realistically, a different
        /// implementation could require a db, so we're putting it here.
        /// </summary>
        /// <param name="startNode"></param>
        /// <returns></returns>
        ReportingStructure BuildReportingStructure(Employee startNode);
    }
}
