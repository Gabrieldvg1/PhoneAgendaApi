using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PhoneAgendaData.Models;
using PhoneAgendaData.Repositories;

namespace PhoneAgendaApi
{
    public class ContactFunctions
    {
        private readonly ILogger<ContactFunctions> _logger;
        private readonly IContactRepository _contactRepository;

        public ContactFunctions(ILogger<ContactFunctions> logger, IContactRepository contactRepository)
        {
            _logger = logger;
            _contactRepository = contactRepository;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function("GetContacts")]
        public async Task<HttpResponseData> GetContacts([HttpTrigger(AuthorizationLevel.Function, "get", Route = "contacts")] HttpRequestData req)
        {
            var contacts = await _contactRepository.GetAllAsync();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(contacts);
            return response;
        }

        [Function("GetContactById")]
        public async Task<HttpResponseData> GetContactById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "contacts/{id}")] HttpRequestData req, string id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            var response = req.CreateResponse(contact is null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
            if (contact != null)
                await response.WriteAsJsonAsync(contact);
            return response;
        }

        [Function("CreateContact")]
        public async Task<HttpResponseData> CreateContact([HttpTrigger(AuthorizationLevel.Function, "post", Route = "contacts")] HttpRequestData req)
        {
            var contact = await req.ReadFromJsonAsync<Contact>();
            if (contact is null)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var created = await _contactRepository.CreateAsync(contact);
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(created);
            return response;
        }

        [Function("UpdateContact")]
        public async Task<HttpResponseData> UpdateContact([HttpTrigger(AuthorizationLevel.Function, "put", Route = "contacts/{id}")] HttpRequestData req, string id)
        {
            var contact = await req.ReadFromJsonAsync<Contact>();
            if (contact is null || contact.Id != id)
                return req.CreateResponse(HttpStatusCode.BadRequest);

            var updated = await _contactRepository.UpdateAsync(contact);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updated);
            return response;
        }

        [Function("DeleteContact")]
        public async Task<HttpResponseData> DeleteContact([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "contacts/{id}")] HttpRequestData req, string id)
        {
            await _contactRepository.DeleteAsync(id);
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}

