namespace Cracker.Base.Model.Responses
{
    public record JobResponse(
        long JobId,
        string Outfile, //base64
        string Potfile, //base64
        double Speed,
        string Error)
    {
        public static JobResponse FromError(long jobId, string error)
            => new(jobId, null, null, 0, error);
    }
}