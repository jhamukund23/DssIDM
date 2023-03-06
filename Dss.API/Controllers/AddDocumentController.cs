using Confluent.Kafka;
using Dss.API.AzureBlobStorage.Controllers;
using Dss.application.Interfaces;
using Dss.Application.Constants;
using Dss.Application.Interfaces;
using Dss.Application.Services;
using Dss.Domain.Models;
using Dss.Domain.Models.Azure;
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
        private readonly IKafkaProducer<string, AddDocumentErrorResponse> _kafkaErrorProducer;
        private readonly IAddDocumentService _addDocumentService;
        private readonly IAzureStorage _storage;
        private readonly ILogger<StorageController> _logger;
        public AddDocumentController(
            IKafkaProducer<string, AddDocumentResponse> kafkaProducer,
            IKafkaProducer<string, AddDocumentErrorResponse> kafkaErrorProducer,
            IAddDocumentService addDocumentService,
            IAzureStorage storage,
            ILogger<StorageController> logger
            )
        {
            _kafkaProducer = kafkaProducer;
            _kafkaErrorProducer = kafkaErrorProducer;
            _addDocumentService = addDocumentService;
            _storage = storage;
            _logger = logger;
        }

        #region Public Action Method
        // Generate and return the SAS URI.
        [HttpPost]
        [Route("GetAddDocumentBlobUrl")]
        public async Task<IActionResult> GetAddDocumentBlobUrl(AddDocumentRequest addDocumentRequest)
        {
            try
            {
                // Get the SAS URI.
                Uri sasUrl = await _storage.GetServiceSasUriForContainer();

                // Send correlation id and sasUrl to Kafka topic.
                ProduceAddDocumentResponse(addDocumentRequest.CorrelationId, sasUrl);
                
                // Insert add document details into database.
                InsertAddDocumentRecordIntoDB(addDocumentRequest, sasUrl);

                //Return status code
                return Ok(new { StatusCode = StatusCodes.Status201Created});
            }
            catch (Exception ex)
            {
                // Send correlation id and error message to Kafka topic.
                ProduceAddDocumentErrorResponse(addDocumentRequest.CorrelationId, ex.Message);
                _logger.LogError($"Exception occurred with a message: {ex.Message}");
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }


        [HttpPost]
        [Route("BlobCompletedEvent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> BlobCompletedEvent(BlobStorageEventHubData blobStorageEventHubData)
        {
            try
            {
                if (blobStorageEventHubData.eventType == "Microsoft.Storage.BlobCreated")
                {


                }

                _logger.LogInformation("sas url details " + "");
                return Ok(new { StatusCode = StatusCodes.Status201Created });
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        #endregion

        #region Private Method
        private void InsertAddDocumentRecordIntoDB(AddDocumentRequest addDocumentRequest, Uri sasUrl)
        {
            AddDocument addDocument = new()
            {
                correlationid = addDocumentRequest.CorrelationId,
                filename = addDocumentRequest.FileName,
                tempbloburl = sasUrl,
            };
            _addDocumentService.AddDocumentAsync(addDocument);
        }
        private void ProduceAddDocumentResponse(Guid correlationId, Uri sasUrl)
        {
            AddDocumentResponse addDocumentResponse = new()
            {
                SasUrl = sasUrl,
                CorrelationId = correlationId
            };
            var topicPart = new TopicPartition(KafkaTopics.AddDocumentResponse, new Partition(1));
            _kafkaProducer.ProduceAsync(topicPart, null, addDocumentResponse);
        }
        private void ProduceAddDocumentErrorResponse(Guid correlationId, string ex)
        {
            AddDocumentErrorResponse addDocumentErrorResponse = new()
            {
                CorrelationId = correlationId,
                Error = ex
            };
            var topicPart = new TopicPartition(KafkaTopics.AddDocumentErrorResponse, new Partition(1));
            _kafkaErrorProducer.ProduceAsync(topicPart, null, addDocumentErrorResponse);
        }
        #endregion
    }
}
