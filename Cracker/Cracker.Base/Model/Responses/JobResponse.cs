namespace Cracker.Base.Model.Responses
{
    public record JobResponse(
        string outfile, //base64
        string potfile, //base64
        double speed,
        string error);// null если все хорошо 
}