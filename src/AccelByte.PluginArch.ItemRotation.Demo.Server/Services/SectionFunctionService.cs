// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Grpc.Core;
using AccelByte.Platform.Catalog.Section.V1;

namespace AccelByte.PluginArch.ItemRotation.Demo.Server.Services
{
    public class SectionFunctionService : AccelByte.Platform.Catalog.Section.V1.Section.SectionBase
    {
        private readonly ILogger<SectionFunctionService> _Logger;

        private float _UpperLimit = 24;

        public SectionFunctionService(ILogger<SectionFunctionService> logger)
        {
            _Logger = logger;
        }

        public override Task<GetRotationItemsResponse> GetRotationItems(GetRotationItemsRequest request, ServerCallContext context)
        {
            _Logger.LogInformation("Received GetRotationItems request.");

            List<SectionItemObject> items = new List<SectionItemObject>(request.SectionObject.Items);
            float inputCount = items.Count;

            
            float currentPoint = DateTime.Now.Hour;
            int selectedIndex = (int)Math.Floor((inputCount / _UpperLimit) * currentPoint);

            SectionItemObject selectedItem = items[selectedIndex];

            GetRotationItemsResponse response = new GetRotationItemsResponse();
            response.ExpiredAt = 0;
            response.Items.Add(selectedItem);

            return Task.FromResult(response);
        }

        public override Task<BackfillResponse> Backfill(BackfillRequest request, ServerCallContext context)
        {
            _Logger.LogInformation("Received Backfill request.");

            BackfillResponse response = new BackfillResponse();

            foreach (var item in request.Items)
            {
                if (item.Owned)
                {
                    BackfilledItemObject newItem = new BackfilledItemObject()
                    {
                        ItemId = Guid.NewGuid().ToString().Replace("-", ""),
                        Index = item.Index
                    };
                    response.BackfilledItems.Add(newItem);
                }
            }

            return Task.FromResult(response);
        }
    }
}
