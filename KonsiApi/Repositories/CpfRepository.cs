using KonsiApi.Data;
using KonsiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KonsiApi.Repositories
{
    public class CpfRepository : ICpfRepository
    {
        private readonly DesafioKonsiContext _context;

        public CpfRepository(DesafioKonsiContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cpf>> GetAllCpfsAsync()
        {
            return await _context.Cpfs.ToListAsync();
        }

        public async Task<Cpf> GetCpfByIdAsync(int id)
        {
            return await _context.Cpfs.FindAsync(id);
        }

        public async Task<Cpf> GetCpfByNumberAsync(string cpfNumber)
        {
            return await _context.Cpfs.FirstOrDefaultAsync(c => c.Numero == cpfNumber);
        }

        public async Task AddCpfAsync(Cpf cpf)
        {
            _context.Cpfs.Add(cpf);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCpfAsync(Cpf cpf)
        {
            _context.Entry(cpf).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCpfAsync(int id)
        {
            var cpf = await _context.Cpfs.FindAsync(id);
            if (cpf != null)
            {
                _context.Cpfs.Remove(cpf);
                await _context.SaveChangesAsync();
            }
        }
    }
}
