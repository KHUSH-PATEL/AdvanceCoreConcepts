namespace CoreAdvanceConcepts.Models
{
    public class ResponceMessage<T>
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int? DataCount { get; set; }
        public List<string>? ErrorMessage { get; set; }
        public T? Data { get; set; }        
    }
}
