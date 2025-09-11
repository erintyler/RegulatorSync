namespace Regulator.Data.Redis.Models;

public interface ICacheModel<T> where T : notnull
{
    T Id { get; }
    string Key { get; }
}