public class MessageDto
{
    public string Message { get; set; } 
    public string Origin { get; set; } = "ServiceB";

     public DateTime Dtime {get; set;}= DateTime.Now;
}
