using Microsoft.AspNetCore.Mvc;
using MicroservicesSolution.ServiceA.Services;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api")]
public class MessageController : ControllerBase
{
    private readonly ProducerService _producerService;

    public MessageController(ProducerService producerService)
    {
        _producerService = producerService;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok("Service is running");
    }

    [HttpPost("sendToA")]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto messageDto)
    {
        if (messageDto == null || string.IsNullOrEmpty(messageDto.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        messageDto.Dtime = DateTime.Now;
        messageDto.Origin = "ServiceA";

        const int maxRetries = 3; 

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                bool isProduced = await _producerService.ProduceAsync("service-a-topic", messageDto);

                if (isProduced)
                {
                    return Ok("Message sent to service-a-topic with origin");
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



