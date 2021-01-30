namespace Cracker.Base.HttpClient.Data
{
    public class MaskTemplate
    {
        public string Charset1 { get; set; }
        public string Charset2 { get; set; }
        public string Charset3 { get; set; }
        public string Charset4 { get; set; }
        public string Mask { get; set; }

        public override string ToString()
        {
            return "-a 3 " + (string.IsNullOrEmpty(Charset1) ? string.Empty : $"-1 {Charset1} ")
                           + (string.IsNullOrEmpty(Charset2) ? string.Empty : $"-2 {Charset2} ")
                           + (string.IsNullOrEmpty(Charset3) ? string.Empty : $"-3 {Charset3} ")
                           + (string.IsNullOrEmpty(Charset4) ? string.Empty : $"-4 {Charset4} ")
                           + $" {Mask}";
        }
    }
}