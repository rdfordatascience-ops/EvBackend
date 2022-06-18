namespace MyEvBackend;

public interface ISmsApi
{
    Task<string> SendApi(string phoneNumber);
}