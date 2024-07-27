using Microsoft.AspNetCore.Mvc;
using Serilog;
using Newtonsoft.Json;
using PlantAPI.Models;
using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace PlantAPI.Controllers
{
    public class PlantController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string? openAiApiKey;
        private readonly string? connectionString;
        private readonly string? containerName;
        private readonly string? endpointAzureStorage;
        private readonly string? baseUrlOpenAI;
        public PlantController(IConfiguration configuration)
        {
            _configuration = configuration;
            openAiApiKey = _configuration["openAiApiKey"];
            connectionString = _configuration["blobService_conectionString"];
            containerName = _configuration["blobService_container"];
            endpointAzureStorage = _configuration["azureStorage_endpoint"];
            baseUrlOpenAI = _configuration["baseUrlOpenAI"];
        }

        [HttpPost, Route("uploadFromCamera")]
        public async Task<IActionResult> Post(IFormFile image, bool dev = false)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var path = Path.Combine("C://Temp//", string.Concat(Guid.NewGuid() + "_", image.FileName)); //guardo la imagen

            using (var stream = new FileStream(path, FileMode.Create))
            {
               await image.CopyToAsync(stream);
            }


            if (dev)
            {
                return Ok(await GetDiseasePredictionTest(path));
            }
            else
            {
                var url = await UploadImageToBlobStorage(path);
                return Ok(await GetDiseasePrediction(path, url));
            }

        }

        public async Task<OpenAIResponse> GetDiseasePrediction(string pathImage, string url)
        {
            var client = new HttpClient();
            var messages = new List<object>();

            var contentList = new List<object>();
            var content1 = new { type = "text", text = "Dada la siguiente planta, identificar enfermedades potenciales" };
            var content2 = new { type = "image_url", image_url = new OpenAIRequest.ImageUrl() { url = url } };
            contentList.Add(content1);
            contentList.Add(content2);
            messages.Add(new { role = "user", content = contentList });

            var model = new OpenAIRequest()
            {
                model = "gpt-4o-mini",
                messages = messages,
                max_tokens = 200
            };

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrlOpenAI);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",  openAiApiKey);
            client.DefaultRequestHeaders.Add("User-Agent", "C# App");
            var content = new StringContent(JsonConvert.SerializeObject(model), null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            var modelresult = JsonConvert.DeserializeObject<OpenAIResponse>(response.Content.ReadAsStringAsync().Result);
            return modelresult;
        }

        public async Task<OpenAIResponse> GetDiseasePredictionTest(string pathImage)
        {
            var choice = new Choice()
            {
                Index = 0,
                Message = new PlantAPI.Models.Message()
                {
                    Role = "assistant",
                    Content = "En la planta de uva que muestras, las características visibles sugieren varias enfermedades potenciales:\n\n1. **Mildew polvoriento (Oidium)**: La presencia de un manto blanco en las hojas puede indicar esta enfermedad fúngica. Se manifiesta como un polvo blanquecino que afecta el crecimiento de la planta.\n\n2. **Punto de hoja (Cercospora)**: Las manchas marrones o rojizas en las hojas pueden ser un signo de esta enfermedad. Puede llevar a la caída prematura de las hojas.\n\n"
                },
                LogProbs = null,
                FinishReason = "length"
            };

            List<Choice> listaChoices = [choice];

            return new OpenAIResponse()
            {
                id = "chatcmpl-9nH9can1G1aFpldPnWfqRocbYbc1B",
                created = 1721531040,
                model = "gpt-4o-mini-2024-07-18",
                choices = listaChoices,
                usage = new Usage()
                {
                    PromptTokens = 25516,
                    CompletionTokens = 200,
                    TotalTokens = 2571
                }
            };
        }

        public async Task<string> UploadImageToBlobStorage(string imagePath)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            //// Create the container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to a blob
            string blobName = Path.GetFileName(imagePath);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            Console.WriteLine($"Uploading to Blob storage as blob:\n\t {blobClient.Uri}");

            // Open the file and upload its data
            using FileStream uploadFileStream = System.IO.File.OpenRead(imagePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
            return blobClient.Uri.AbsoluteUri;
          
        }
    }
}
