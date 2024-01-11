using KonsiApi.Models;

namespace KonsiApi.Repositories
{
    public interface ICpfRepository
    {
        Task<IEnumerable<Cpf>> GetAllCpfsAsync();
        Task<Cpf> GetCpfByIdAsync(int id);
        Task<Cpf> GetCpfByNumberAsync(string cpfNumber);
        Task AddCpfAsync(Cpf cpf);
        Task UpdateCpfAsync(Cpf cpf);
        Task DeleteCpfAsync(int id);
    }
}
