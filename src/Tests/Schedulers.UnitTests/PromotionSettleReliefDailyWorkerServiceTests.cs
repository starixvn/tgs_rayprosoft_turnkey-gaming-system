using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Tgs.Promotion.RebateService.Client;
using Tgs.Promotion.ReliefMoney.Client;
using TGS.Services.Operations.Queries;
using TGS.Workers.Schedulers.Jobs.Promotion;

namespace TGS.Tests.Schedulers.UnitTests;

[TestFixture]
public class PromotionSettleReliefDailyWorkerServiceTests
{
    [Test]
    public async Task Can_Settle_Relief()
    {
        var clientMock = new Mock<IPromotionReliefClient>();

        var worker = new PromotionSettleReliefDailyWorkerService(clientMock.Object);

        await ((IPromotionSettleReliefDailyWorkerService)worker).ManualExecutionAsync(DateOnly.Parse("2023-01-01"), 16);

        clientMock.Verify(x
            => x.SettleAsync(It.Is<DateOnly>(d => d == DateOnly.Parse("2023-01-01")), It.Is<int>(h => h == 16)));
    }
}
