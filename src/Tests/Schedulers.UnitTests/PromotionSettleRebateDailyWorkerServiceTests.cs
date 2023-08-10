using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Tgs.Promotion.RebateService.Client;
using TGS.Workers.Schedulers.Jobs.Promotion;

namespace TGS.Tests.Schedulers.UnitTests;

[TestFixture]
public class PromotionSettleRebateDailyWorkerServiceTests
{
    [Test]
    public async Task Can_Settle_Rebate()
    {
        var clientMock = new Mock<IPromotionRebateClient>();

        var worker = new PromotionSettleRebateDailyWorkerService(clientMock.Object);

        await ((IPromotionSettleRebateDailyWorkerService)worker).ManualExecutionAsync(DateOnly.Parse("2023-01-01"), 16);

        clientMock.Verify(x
            => x.SettleAsync(It.Is<DateOnly>(d => d == DateOnly.Parse("2023-01-01")), It.Is<int>(h => h == 16)));
    }
}
