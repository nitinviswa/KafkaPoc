using MicroservicesSolution.ServiceA.Services;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddSingleton<ProducerService>(sp => new ProducerService("localhost:9092"));
    services.AddHostedService<KafkaConsumerService>();
}



    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        
        Console.WriteLine("Application is starting...");

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
