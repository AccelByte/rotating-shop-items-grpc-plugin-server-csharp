// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using AccelByte.Platform.Catalog.Section.V1;
using AccelByte.PluginArch.ItemRotation.Demo.Server.Services;

namespace AccelByte.PluginArch.ItemRotation.Demo.Tests
{
    [TestFixture]
    public class LootboxFunctionServiceTests
    {
        private ILogger<SectionFunctionService> _ServiceLogger;

        public LootboxFunctionServiceTests()
        {
            ILoggerFactory loggerFactory = new NullLoggerFactory();
            _ServiceLogger = loggerFactory.CreateLogger<SectionFunctionService>();
        }

        [Test]
        public async Task RotationItemTest()
        {
            int maxItemCount = 8;
            int upperLimit = 24;

            var service = new SectionFunctionService(_ServiceLogger);

            GetRotationItemsRequest request = new GetRotationItemsRequest();
            request.Namespace = "accelbyte";
            request.UserId = "b52a2364226d436285c1b8786bc9cbd1";
            request.SectionObject = new SectionObject()
            {
                SectionId = "c4d737f6f42c423e8690ff705ab75d9f",
                SectionName = "example",
                StartDate = 1672519500,
                EndDate = 1675197900
            };

            for (int i = 1; i <= maxItemCount; i++)
            {
                request.SectionObject.Items.Add(new SectionItemObject()
                {
                    ItemId = Guid.NewGuid().ToString().Replace("-", ""),
                    ItemSku = $"SKU_{i}"
                });
            }

            var response = await service.GetRotationItems(request, new UnitTestCallContext());
            Assert.IsNotNull(response);

            float inputCount = maxItemCount;
            float currentPoint = DateTime.Now.Hour;
            int selectedIndex = (int)Math.Floor((inputCount / upperLimit) * currentPoint);
            SectionItemObject expectedItem = request.SectionObject.Items[selectedIndex];

            Assert.Greater(response.Items.Count, 0);
            Assert.AreEqual(expectedItem.ItemId, response.Items[0].ItemId);
        }

        [Test]
        public async Task BackfillTest()
        {
            var service = new SectionFunctionService(_ServiceLogger);

            BackfillRequest request = new BackfillRequest();
            request.Namespace = "accelbyte";
            request.UserId = "b52a2364226d436285c1b8786bc9cbd1";
            request.SectionId = "c4d737f6f42c423e8690ff705ab75d9f";
            request.SectionName = "example";

            int maxItemCount = 8;
            int randomOwnedIndex = (new Random()).Next(1, maxItemCount + 1);

            for (int i = 1; i <= maxItemCount; i++)
            {
                request.Items.Add(new RotationItemObject()
                {
                    Index = i,
                    ItemId = Guid.NewGuid().ToString().Replace("-", ""),
                    ItemSku = $"SKU_{i}",
                    Owned = (i == randomOwnedIndex)
                });
            }

            var response = await service.Backfill(request, new UnitTestCallContext());
            Assert.IsNotNull(response);

            bool isFound = false;
            foreach (var item in response.BackfilledItems)
            {
                if (item.Index == randomOwnedIndex)
                {
                    Assert.AreNotEqual(request.Items[randomOwnedIndex].ItemId, item.ItemId);
                    isFound = true;
                    break;
                }   
            }

            Assert.IsTrue(isFound);
        }
    }
}