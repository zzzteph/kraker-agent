namespace Cracker.Base.Model.Responses
{
    public record HashListResponse(long count, // -1 если ошибка
        string error); // null если все хорошо 
}