using System.Threading.Tasks;

namespace Aluno.Core.Domain.Interfaces.Repositories;

public interface IRedisRepository
{
    Task SetAsync<T>(string key, T value, int? expirationInMinutes = null);
    Task<T> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}