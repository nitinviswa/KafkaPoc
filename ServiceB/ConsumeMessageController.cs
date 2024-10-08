using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class ConsumeMessageController : ControllerBase
{
    private readonly KafkaProducerService _kafkaProducerService;

    public ConsumeMessageController(KafkaProducerService kafkaProducerService)
    {
        _kafkaProducerService = kafkaProducerService;
    }

    [HttpPost("sendToB")]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
    {
        if (messageDto == null || string.IsNullOrEmpty(messageDto.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        messageDto.Dtime = DateTime.Now;
        messageDto.Origin = "ServiceB";

        const int maxRetries = 3; 

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                bool isProduced = await _kafkaProducerService.PublishMessageAsync("service-b-topic", messageDto, maxRetries);

                if (isProduced)
                {
                    return Ok("Message sent to service-b-topic with origin");
                }
                else
                {
                    Console.WriteLine($"Attempt {attempt + 1} failed to send message. Retrying...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on attempt {attempt + 1}: {ex.Message}");
            }
        }

        return StatusCode(500, "Error sending message after multiple attempts");
    }
}


