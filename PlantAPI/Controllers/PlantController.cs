using Microsoft.AspNetCore.Mvc;
using Serilog;
using Newtonsoft.Json;
using PlantAPI.Models;
using System.Net.Http.Headers;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Text;
using OpenAI;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using System;
using PlantAPI.Data;

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
        private IDbService _dbService;
        public PlantController(IConfiguration configuration, IDbService dbService)
        {
            _configuration = configuration;
            openAiApiKey = _configuration["openAiApiKey"];
            connectionString = _configuration["blobService_conectionString"];
            containerName = _configuration["blobService_container"];
            endpointAzureStorage = _configuration["azureStorage_endpoint"];
            baseUrlOpenAI = _configuration["baseUrlOpenAI"];
            _dbService = dbService;
        }

        [HttpPost, Route("uploadFromCamera")]
        public async Task<IActionResult> Post(IFormFile image, string deviceId, bool dev = false)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileName = $"{Guid.NewGuid()}_{image.FileName}";
            var path = Path.Combine("C://Temp//", fileName);

            try
            {
                await using (var stream = new FileStream(path, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var url = await UploadImageToBlobStorage(path);

                if (dev)
                {
                    return Ok(await GetDiseasePredictionTest(path, url, deviceId));
                }
                else
                {

                    return Ok(await GetDiseasePrediction(path, url, deviceId));
                }
            }
            catch
            {
                return StatusCode(500, "An error occurred while processing the image.");
            }
        }

        public async Task<Desease> GetDiseasePrediction(string pathImage, string url, string deviceId)
        {
            using var client = new HttpClient();

            var messages = new List<object>
            {
                new { role = "user", content = new List<object>
                    {
                        //new { type = "text", text = "Base on this image, detect desease and give the information in a valid json with this format in spanish:\r\n\r\n{\r\n   \"isDesease\": true,\r\n   \"scientific_name\":\"plant cientific denomination\"\r\n   \"description\": \"small description of the plant\",\r\n   \"deseases\": [ up to two deseases ordered by highest probability\r\n      \"deseaseName\": \"desease plant cientific denomination\",\r\n      \"deseaseDescription\":\"small description of desease\",\r\n      \"solution\": \"fertilizer or another solution description\"\r\n     ]\r\n}\r\n" },
                    new { type = "text", text = "Base on this image, detect desease and give the information in a valid json that must strictly adhere to this format. Give the information in spanish:\r\n\r\n{\r\n   \"isDesease\": \"type boolean, true if ill or false if not\",\r\n   \"scientific_name\":\"type string, cientific name denomination of plant\"\r\n   \"description\": \"small description of the plant, information about his leaves,trunk, perennial or not, description technical\",\r\n   \"deseases\": [ the most probability desease (one) if ordered by highest probability, if not ill send all element below in null\r\n      \"deseaseName\": \"type string, desease plant cientific name denomination\",\r\n      \"deseaseDescription\":\"type string, small description of desease, like what produce this desease\",\r\n      \"solution\": \"type string, small description of solution to desease\",\r\n      \"fertilizer\":\"type string, fertilizer brands witch can be buyed in Argentina, up to 3, indicate the name of the product and company that manufactures the product, separated by comma. For example, for oidio desease give me some responses like Cantus from BASF\"\r\n     ]\r\n}"},    
                    new { type = "image_url", image_url = new OpenAIRequest.ImageUrl() { url = url } }
                    }
                }
            };

            var model = new OpenAIRequest()
            {
                model = "gpt-4o-mini",
                messages = messages,
                response_format = new { type = "json_object" },
                max_tokens = 500
            };

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrlOpenAI)
            {
                Content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")
            };

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var modelResult = JsonConvert.DeserializeObject<OpenAIResponse>(responseContent);
            var cont = JsonConvert.DeserializeObject<Desease>(modelResult.choices[0].Message.Content);

            await SaveResultCapture(url, deviceId, cont);

            return cont;
        }

        public async Task<bool> SaveResultCapture(string pathImage, string deviceId, Desease model)
        {
            Capture capture = new Capture()
            {
                deviceId = deviceId,
                urlImage = pathImage,
                deseasesResult = JsonConvert.SerializeObject(model.deseases),
                scientific_name = model.scientific_name,
                description = model.description,
                Id = Guid.NewGuid().ToString(),
                isDesease = model.isDesease
            };

            var result = await _dbService.AddCapture(capture);

            return true;
        }

        public async Task<Desease> GetDiseasePredictionTest(string pathImage, string url, string deviceId)
        {
            //return new OpenAIResponse()
            //{
            //    id = "chatcmpl-9nH9can1G1aFpldPnWfqRocbYbc1B",
            //    created = 1721531040,
            //    model = "gpt-4o-mini-2024-07-18",
            //    choices = listaChoices,
            //    usage = new PlantAPI.Models.Usage()
            //    {
            //        PromptTokens = 25516,
            //        CompletionTokens = 200,
            //        TotalTokens = 2571
            //    }
            //};

            var enfermedad1 = new DeseaseDetails()
            {
                deseaseName = "Oidio",
                deseaseScientificName = "Erysiphe necator",
                deseaseDescription = "El oidio es un hongo que afecta las hojas y los frutos, cubriéndolos con un polvo blanco o gris.",
                solution = "Aplicar fungicidas específicos como azufre en polvo o productos a base de cobre.",
                fertilizer = "Azufre en polvo o fungicidas a base de cobre"
            };
            var enfermedad2 = new DeseaseDetails()
            {
                deseaseName = "Mildiu",
                deseaseScientificName = "Plasmopara viticola",
                deseaseDescription = "El mildiu es un hongo que provoca manchas aceitosas en las hojas y moho en los frutos.",
                solution = "Usar fungicidas a base de cobre y asegurar una buena ventilación del viñedo.",
                fertilizer = "Fungicidas cúpricos."
            };
            var enfer = new List<DeseaseDetails>
            {
                enfermedad1,
                enfermedad2
            };

            var model = new Desease()
            {
                isDesease = true,
                scientific_name = "Vitis vinifera",
                description = "La vid es una planta leñosa trepadora, cultivada principalmente por sus frutos, las uvas, que se utilizan para producir vino, jugo, y otras bebidas.",
                deseases = enfer
            };

            await SaveResultCapture(url, deviceId, model);

            return model;
        }

        public async Task<string> UploadImageToBlobStorage(string imagePath)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            string blobName = Path.GetFileName(imagePath);

            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            using FileStream uploadFileStream = System.IO.File.OpenRead(imagePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
            return blobClient.Uri.AbsoluteUri;
        }

        [HttpGet, Route("captures")]
        public async Task<IActionResult> GetCaptures(string deviceId)
        {
            return Ok(new { capture = await _dbService.GetCapturesByDeviceId(deviceId) });
        }
    }
}
