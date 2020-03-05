﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace AmazonProvider
{
    public class AmazonIdentification
    {
        private readonly IAmazonRekognition _amazonRekognition;
        private readonly List<string> detectionAttributes = new List<string> { "ALL" };
        private readonly QualityFilter qualityFilter = QualityFilter.HIGH;
        private readonly int maxFaces = 1;
        public AmazonIdentification(IAmazonRekognition amazonRekognition)
        {
            _amazonRekognition = amazonRekognition;
        }

        public async Task GenerateNewCollectionAsync(string collectionId)
        {
            CreateCollectionResponse createCollectionResponse = await this._amazonRekognition.CreateCollectionAsync(
                new CreateCollectionRequest
                {
                    CollectionId = collectionId
                }
            );
            if (createCollectionResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(createCollectionResponse.HttpStatusCode.ToString());
        }

        public async Task<string> faceAnalyzerAsync(IFormFile photoFile, string collectionID)
        {
            MemoryStream memoryStream = new MemoryStream();
            photoFile.CopyTo(memoryStream);

            IndexFacesRequest indexFacesRequest = new IndexFacesRequest
            {
                CollectionId = collectionID,
                DetectionAttributes = detectionAttributes,
                MaxFaces = maxFaces,
                QualityFilter = qualityFilter,
                Image = new Image
                {
                    Bytes = memoryStream,
                }
            };

            IndexFacesResponse indexFacesResponse = await this._amazonRekognition.IndexFacesAsync(indexFacesRequest);

            if (indexFacesResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception(indexFacesResponse.HttpStatusCode.ToString());

            if (indexFacesResponse.UnindexedFaces.Count > 0 || indexFacesResponse.FaceRecords.Count != 1)
                throw new Exception("Unindexed Faces: " + indexFacesResponse.UnindexedFaces.Count + "\nIndexed Faces: " + indexFacesResponse.FaceRecords.Count);

            return JsonSerializer.Serialize(indexFacesResponse.FaceRecords[0]);
        }
    }
}