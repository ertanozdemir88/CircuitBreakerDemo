using Polly;
using Polly.CircuitBreaker;

class Program
{
    static async Task Main(string[] args)
    {
        HttpClient client = new HttpClient();

        //circuit breaker politikası tanımları
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>() //hangi hatalarda devre açılacağını belirliyoruz.
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3, //hata sayısı 3 olduğunda devre açılır
                durationOfBreak: TimeSpan.FromSeconds(10) //devre 10 saniye açık kalır
                );

        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine($"indis: {i}");
            try
            {
                //circuit breaker politikası ile API çağrısı yapıyoruz.
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    Console.WriteLine("API'ye istek gönderiliyoru");
                    //hatalı url: "https://yanlisurl.com/posts/1"
                    //geçerli url: "https://jsonplaceholder.typicode.com/posts/1"
                    HttpResponseMessage response = await client.GetAsync("https://jsonplaceholder.typicode.com/posts/1");

                    //eğer istek başırısız olursa hata fırlat
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Yanıt: {responseBody}");
                });
            }
            catch (BrokenCircuitException ex)
            {
                Console.WriteLine("Circuit açık, istek yapılmadı.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"İstek sırasında hata oluştu: {ex.Message}");
            }

            //her bir deneme arasında bekleme süresi
            await Task.Delay(2000);
        }

        // Program bitmeden önce ekrana basılan sonuçları görmek için bekleme ekleyelim
        Console.WriteLine("İşlem tamamlandı. Çıkmak için bir tuşa basın...");
        Console.ReadKey();
    }
}
