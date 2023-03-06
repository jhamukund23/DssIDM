﻿using Confluent.Kafka;
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

        [HttpPost]
        [Route("GetAddDocumentBlobUrl")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddDocumentBlobUrl(AddDocumentRequest addDocumentRequest)
        {
            try
            {
                Uri sasUrl = await _storage.GetServiceSasUriForContainer();
                SendResponseToTopic(addDocumentRequest.CorrelationId, sasUrl);
                InsertAddDocumentIntoDB(addDocumentRequest, sasUrl);

                //_logger.LogInformation("sas url details " + addDocumentResponse);
                return Ok(new { StatusCode = StatusCodes.Status201Created });
            }
            catch (Exception ex)
            {
                SendErrorResponseToTopic(addDocumentRequest.CorrelationId, ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest);
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

        #region
        private void InsertAddDocumentIntoDB(AddDocumentRequest addDocumentRequest, Uri sasUrl)
        {
            AddDocument addDocument = new()
            {
                correlationid = addDocumentRequest.CorrelationId,
                filename = addDocumentRequest.FileName,
                tempbloburl = sasUrl,
            };
            _addDocumentService.AddDocumentAsync(addDocument);
        }
        private void SendResponseToTopic(Guid correlationId, Uri sasUrl)
        {
            AddDocumentResponse addDocumentResponse = new()
            {
                SasUrl = sasUrl,
                CorrelationId = correlationId
            };
            var topicPart = new TopicPartition(KafkaTopics.AddDocumentResponse, new Partition(1));
            _kafkaProducer.ProduceAsync(topicPart, null, addDocumentResponse);
        }
        private void SendErrorResponseToTopic(Guid correlationId, string ex)
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
