using PhoneAgendaData.Models;

namespace PhoneAgendaData.Repositories;
public interface IContactRepository
{
    Task<Contact> CreateAsync(Contact contact);
    Task<IEnumerable<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(string id);
    Task<Contact> UpdateAsync(Contact contact);
    Task DeleteAsync(string id);
}

