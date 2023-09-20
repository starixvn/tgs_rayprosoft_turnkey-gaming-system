using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Tgs.Promotion.RebateService.Client;
using TGS.Workers.Schedulers.Jobs.Promotion;

namespace TGS.Tests.Schedulers.UnitTests;

[TestFixture]
public class PromotionSendRebateDailyWeeklyWorkerServiceTests
{
    [Test]
    public async Task Send_Rebate()
    {
        var clientMock = new Mock<IPromotionRebateClient>();

        var worker = new PromotionSendRebateDailyWeeklyWorkerService(clientMock.Object);

        await ((IPromotionSendRebateDailyWeeklyWorkerService)worker).ManualExecutionAsync(DateOnly.Parse("2023-01-01"), 16);

        clientMock.Verify(x
            => x.SendBonusAsync(It.Is<DateOnly>(d => d == DateOnly.Parse("2023-01-01")), It.Is<int>(h => h == 16)));
    }
}
