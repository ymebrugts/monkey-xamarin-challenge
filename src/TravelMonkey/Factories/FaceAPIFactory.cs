using Microsoft.Azure.CognitiveServices.Vision.Face;
using System;
using System.Collections.Generic;
using System.Text;

namespace TravelMonkey.Factories
{
    public class FaceAPIFactory
    {
        private string _faceApiKey = ApiKeys.FaceAPIKey;
        private string faceApiEndpoint = ApiKeys.FaceAPIEndpoint;

        public FaceAPIFactory()
        {           
        }

        public FaceClient CreateFaceClient()
        {
            return new FaceClient(new ApiKeyServiceClientCredentials(_faceApiKey), new System.Net.Http.DelegatingHandler[] { }) { Endpoint = faceApiEndpoint };
        }

    }
}
