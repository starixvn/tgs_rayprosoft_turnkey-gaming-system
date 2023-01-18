using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using TGS.Services.Operations.Queries;
using TGS.Workers.Schedulers.Jobs.Promotion;

namespace TGS.Tests.Schedulers.UnitTests;

[TestFixture]
[Parallelizable]
public class PromotionWeeklyScheduleWorkerTests
{
    [Test]
    public async Task Get_420_Time_Span_Minutes()
    {
        var promotionSettlementWeeklyWorkerServiceMock = new Mock<IPromotionSettlementWeeklyWorkerService>();
        var settingQueriesMock = new Mock<ISettingQueries>();

        var utcNowMock = new Mock<IDateTimeOffsetExtension>();

        var worker = new PromotionSettlementWeeklyWorker(promotionSettlementWeeklyWorkerServiceMock.Object, settingQueriesMock.Object, utcNowMock.Object);

        var json = "{\r\n    \"16\":480,\r\n    \"17\":420\r\n}";

        settingQueriesMock.Setup(s => s.GetSettingValueByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(json);

        var date = new DateTimeOffset(2023, 1, 17, 17, 33, 0, TimeSpan.Zero);

        utcNowMock.Setup(s => s.UtcNow).Returns(date.UtcDateTime);

        _ = await worker.ExecuteAsync(default);

        promotionSettlementWeeklyWorkerServiceMock.Verify(x => x.ExecutionAsync(It.Is<int>(t => t == 420)));
    }

    [Test]
    public async Task Get_480_Time_Span_Minutes()
    {
        var promotionSettlementWeeklyWorkerServiceMock = new Mock<IPromotionSettlementWeeklyWorkerService>();
        var settingQueriesMock = new Mock<ISettingQueries>();

        var utcNowMock = new Mock<IDateTimeOffsetExtension>();

        var worker = new PromotionSettlementWeeklyWorker(promotionSettlementWeeklyWorkerServiceMock.Object, settingQueriesMock.Object, utcNowMock.Object);

        var json = "{\r\n    \"16\":480,\r\n    \"17\":420\r\n}";

        settingQueriesMock.Setup(s => s.GetSettingValueByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(json);

        var date = new DateTimeOffset(2023, 1, 17, 16, 0, 0, TimeSpan.Zero);

        utcNowMock.Setup(s => s.UtcNow).Returns(date.UtcDateTime);

        _ = await worker.ExecuteAsync(default);

        promotionSettlementWeeklyWorkerServiceMock.Verify(x => x.ExecutionAsync(It.Is<int>(t => t == 480)));
    }

    [Test]
    public async Task Get_null_Time_Span_Minutes()
    {
        var promotionSettlementWeeklyWorkerServiceMock = new Mock<IPromotionSettlementWeeklyWorkerService>();
        var settingQueriesMock = new Mock<ISettingQueries>();

        var utcNowMock = new Mock<IDateTimeOffsetExtension>();

        var worker = new PromotionSettlementWeeklyWorker(promotionSettlementWeeklyWorkerServiceMock.Object, settingQueriesMock.Object, utcNowMock.Object);

        var json = "{\r\n    \"16\":480,\r\n    \"17\":420\r\n}";

        settingQueriesMock.Setup(s => s.GetSettingValueByKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(json);

        var date = new DateTimeOffset(2023, 1, 17, 18, 33, 0, TimeSpan.Zero);

        utcNowMock.Setup(s => s.UtcNow).Returns(date.UtcDateTime);

        _ = await worker.ExecuteAsync(default);

        promotionSettlementWeeklyWorkerServiceMock.Verify(x => x.ExecutionAsync(It.IsAny<int>()), Times.Never());
    }
}
