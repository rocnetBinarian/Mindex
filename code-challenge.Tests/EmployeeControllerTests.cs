using challenge.Controllers;
using challenge.Data;
using challenge.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using code_challenge.Tests.Integration.Extensions;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using code_challenge.Tests.Integration.Helpers;
using System.Text;
using System.Threading.Tasks;

namespace code_challenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [TestInitialize]
        public void InitializeTest()
        {
            _testServer = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<TestServerStartup>()
                .UseEnvironment("Development"));

            _httpClient = _testServer.CreateClient();
        }

        [TestCleanup]
        public void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetReportingStructure_Returns_NotFound()
        {
            string id = "BADID";
            var tResponse = _httpClient.GetAsync($"api/employee/getReportingStructure/{id}");
            var response = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        // test top of the hierarchy
        [TestMethod]
        public async Task GetReportingStructure1_Returns_OK()
        {
            string id = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var tResponse = _httpClient.GetAsync($"api/employee/getReportingStructure/{id}");
            var response = await tResponse.ConfigureAwait(false);
            var rs = response.DeserializeContent<ReportingStructure>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(4, rs.numberOfReports);
        }

        // Test middle of hierarchy with underlings
        [TestMethod]
        public async Task GetReportingStructure2_Returns_OK()
        {
            string id = "03aa1462-ffa9-4978-901b-7c001562cf6f";
            var tResponse = _httpClient.GetAsync($"api/employee/getReportingStructure/{id}");
            var response = await tResponse.ConfigureAwait(false);
            var rs = response.DeserializeContent<ReportingStructure>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, rs.numberOfReports);
        }

        // Test middle of hierarchy without underlings
        [TestMethod]
        public async Task GetReportingStructure3_Returns_OK()
        {
            string id = "b7839309-3348-463b-a7e3-5de1c168beb3";
            var tResponse = _httpClient.GetAsync($"api/employee/getReportingStructure/{id}");
            var response = await tResponse.ConfigureAwait(false);
            var rs = response.DeserializeContent<ReportingStructure>();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, rs.numberOfReports);
        }


        // There is no employee service endpoint to make someone an underling of someone else, so we can't
        //  test adding a new employee and then running this reportingstructure.  Moving on.





        // Test an empty Compensation
        [TestMethod]
        public async Task CreateCompensation1_Returns_BadRequest()
        {
            string id = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            // empty compensation values
            Compensation comp = new Compensation();

            var postContent = new JsonSerialization().ToJson(comp);
            var tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            var resp = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        // Test a compensation with a duplicate employee Id
        [TestMethod]
        public async Task CreateCompensation2_Returns_BadRequest()
        {
            string id = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            string errMsg = $"Compensation for employee id {id} already exists.";

            // First, create something
            await CreateCompensation1_Returns_Ok().ConfigureAwait(false);
            


            // Now, submit a new one using the same values (that we as a developer know)
            Compensation comp = new Compensation()
            {
                effectiveDate = DateTime.Now,
                salary = 123
            };

            var postContent = new JsonSerialization().ToJson(comp);
            var tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            var resp = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.AreEqual(errMsg, await resp.Content.ReadAsStringAsync().ConfigureAwait(false));

            // Now submit a different compensation under the same employee
            comp = new Compensation()
            {
                effectiveDate = DateTime.Today.AddDays(1),
                salary = 456.78
            };

            postContent = new JsonSerialization().ToJson(comp);
            tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            resp = await tResponse.ConfigureAwait(false);
            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.AreEqual(errMsg, await resp.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        [TestMethod]
        public async Task CreateCompensation_Returns_NotFound()
        {
            string id = "BADID";

            Compensation comp = new Compensation()
            {
                effectiveDate = DateTime.Now,
                salary = 123
            };

            var postContent = new JsonSerialization().ToJson(comp);
            var tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            var resp = await tResponse.ConfigureAwait(false);
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);

            // Now try with a prepopulated employee
            var getRequestTask = _httpClient.GetAsync($"api/employee/16a596ae-edd3-4847-99fe-c4518e82c86f");
            var response = await getRequestTask.ConfigureAwait(false);
            var employee = response.DeserializeContent<Employee>();


            comp = new Compensation()
            {
                effectiveDate = DateTime.Now,
                employee = employee,
                salary = 123.45
            };

            postContent = new JsonSerialization().ToJson(comp);
            tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            resp = await tResponse.ConfigureAwait(false);
            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [TestMethod]
        public async Task CreateCompensation1_Returns_Ok()
        {
            string id = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            Compensation comp = new Compensation()
            {
                effectiveDate = DateTime.Today,
                salary = 123
            };

            var postContent = new JsonSerialization().ToJson(comp);
            var tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            var resp = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
        }


        // Test adding a second compensation
        [TestMethod]
        public async Task CreateCompensation2_Returns_Ok()
        {
            string id = "b7839309-3348-463b-a7e3-5de1c168beb3";

            await CreateCompensation1_Returns_Ok().ConfigureAwait(false);


            Compensation comp = new Compensation()
            {
                effectiveDate = DateTime.Today.AddDays(2),
                salary = 567.92
            };

            var postContent = new JsonSerialization().ToJson(comp);
            var tResponse = _httpClient.PostAsync($"api/employee/createCompensation/{id}",
                new StringContent(postContent, Encoding.UTF8, "application/json"));
            var resp = await tResponse.ConfigureAwait(false);

            var rtnComp = resp.DeserializeContent<Compensation>();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
            Assert.AreEqual(567.92, rtnComp.salary);
            Assert.AreEqual(DateTime.Today.AddDays(2), rtnComp.effectiveDate);
            Assert.AreEqual(id, rtnComp.employee.EmployeeId);
            Assert.AreEqual("Paul", rtnComp.employee.FirstName);
            Assert.AreEqual("McCartney", rtnComp.employee.LastName);
        }

        // Test with a bad eid
        [TestMethod]
        public async Task GetCompensation1_Returns_NotFound()
        {
            string id = "BADID";

            var tResponse = _httpClient.GetAsync($"api/employee/getCompensation/{id}");
            var resp = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }


        // Test with a good eid, but a bad cid
        [TestMethod]
        public async Task GetCompensation2_Returns_NotFound()
        {
            string id = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            var tResponse = _httpClient.GetAsync($"api/employee/getCompensation/{id}");
            var resp = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
        }

        // Will not test for BadRequest, as the current API endpoint would not allow program flow to
        //  reach the point where id would be null and throw that exception.  It exists as a sanity check.


        [TestMethod]
        public async Task GetCompensation1_Returns_Ok()
        {
            string id = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            // First, create compensation
            await CreateCompensation1_Returns_Ok().ConfigureAwait(false);

            var tResponse = _httpClient.GetAsync($"api/employee/getCompensation/{id}");
            var resp = await tResponse.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var comp = resp.DeserializeContent<Compensation>();

            Assert.AreEqual(DateTime.Today, comp.effectiveDate);
            Assert.AreEqual(123, comp.salary);
            Assert.AreEqual(id, comp.employee.EmployeeId);
            Assert.AreEqual("John", comp.employee.FirstName);
            Assert.AreEqual("Lennon", comp.employee.LastName);
        }

    }
}
