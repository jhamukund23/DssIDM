using Confluent.Kafka;
using Dss.API.AzureBlobStorage.Controllers;
using Dss.application.Interfaces;
using Dss.Application.Constants;
using Dss.Domain.Models;
using Kafka.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dss.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddDocumentController : ControllerBase
    {
        private readonly IKafkaProducer<string, AddDocumentResponse> _kafkaProducer;
        private readonly IAzureStorage _storage;
        private readonly ILogger<StorageController> _logger;
        public AddDocumentController(
            IKafkaProducer<string, AddDocumentResponse> kafkaProducer,
            IAzureStorage storage,
            ILogger<StorageController> logger
            )
        {
            _kafkaProducer = kafkaProducer;
            _storage = storage;
            _logger = logger;
        }

        [HttpPost]
        [Route("GetAddDocumentBlobUrl")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddDocumentBlobUrl(AddDocument addDocument)
        {
            Uri sasUrl = await _storage.GetServiceSasUriForContainer();
            AddDocumentResponse addDocumentResponse = new()
            {
                SasUrl = sasUrl,
                CorrelationId = addDocument.CorrelationId
            };
            var topicPart = new TopicPartition(KafkaTopics.AddDocumentResponse, new Partition(1));
            await _kafkaProducer.ProduceAsync(topicPart, null, addDocumentResponse);
            _logger.LogInformation("sas url details " + addDocumentResponse);
            return Ok(addDocumentResponse);
        }

    }
}
