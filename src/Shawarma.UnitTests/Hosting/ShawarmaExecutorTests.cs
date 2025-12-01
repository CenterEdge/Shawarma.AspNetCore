using Microsoft.Extensions.Logging;
using Shawarma.AspNetCore.Hosting;
using Shawarma.AspNetCore.Internal;

namespace Shawarma.UnitTests.Hosting;

public class ShawarmaExecutorTests
{
    #region StartAsync

    [Fact]
    public async Task StartAsync_StartsServices()
    {
        // Arrange

        var logger = new Mock<ILogger<ShawarmaExecutor>>().Object;

        var stateProvider = new ApplicationStateProvider(new Mock<ILogger<ApplicationStateProvider>>().Object);

        var service = new Mock<IShawarmaService>();
        service
            .Setup(m => m.UpdateStateAsync(It.IsAny<ApplicationState>(), CancellationToken.None))
            .Returns(Task.CompletedTask);

        var services = new[] { service.Object };

        var executor = new ShawarmaExecutor(stateProvider, services, logger);

        // Act

        await executor.StartAsync(CancellationToken.None);

        // Assert

        service
            .Verify(m => m.UpdateStateAsync(It.IsAny<ApplicationState>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task StartAsync_MonitorsForChanges()
    {
        // Arrange

        var logger = new Mock<ILogger<ShawarmaExecutor>>().Object;

        var stateProvider = new ApplicationStateProvider(new Mock<ILogger<ApplicationStateProvider>>().Object);

        var service = new Mock<IShawarmaService>();
        service
            .Setup(m => m.UpdateStateAsync(It.IsAny<ApplicationState>(), CancellationToken.None))
            .Returns(Task.CompletedTask);
        service
            .Setup(m => m.StopAsync(CancellationToken.None))
            .Returns(Task.CompletedTask);

        var services = new[] { service.Object };

        var executor = new ShawarmaExecutor(stateProvider, services, logger);

        // Act

        await executor.StartAsync(CancellationToken.None);
        service.Invocations.Clear();

        await stateProvider.SetApplicationStateAsync(new ApplicationState {Status = ApplicationStatus.Active});
        await stateProvider.SetApplicationStateAsync(new ApplicationState {Status = ApplicationStatus.Inactive});
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert

        service
            .Verify(m => m.UpdateStateAsync(It.IsAny<ApplicationState>(), CancellationToken.None), Times.Exactly(2));
        service
            .Verify(m => m.StopAsync(CancellationToken.None), Times.Never);
    }

    #endregion

    #region StopAsync

    [Fact]
    public async Task StopAsync_StopsServices()
    {
        // Arrange

        var logger = new Mock<ILogger<ShawarmaExecutor>>().Object;

        var stateProvider = new ApplicationStateProvider(new Mock<ILogger<ApplicationStateProvider>>().Object);

        var service = new Mock<IShawarmaService>();
        service
            .Setup(m => m.StopAsync(CancellationToken.None))
            .Returns(Task.CompletedTask);

        var services = new[] { service.Object };

        var executor = new ShawarmaExecutor(stateProvider, services, logger);

        // Act

        await executor.StopAsync(CancellationToken.None);

        // Assert

        service
            .Verify(m => m.StopAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task StopAsync_PreventsChangeUpdates()
    {
        // Arrange

        var logger = new Mock<ILogger<ShawarmaExecutor>>().Object;

        var stateProvider = new ApplicationStateProvider(new Mock<ILogger<ApplicationStateProvider>>().Object);

        var service = new Mock<IShawarmaService>();
        service
            .Setup(m => m.UpdateStateAsync(It.IsAny<ApplicationState>(), CancellationToken.None))
            .Returns(Task.CompletedTask);
        service
            .Setup(m => m.StopAsync(CancellationToken.None))
            .Returns(Task.CompletedTask);

        var services = new[] { service.Object };

        var executor = new ShawarmaExecutor(stateProvider, services, logger);

        await executor.StartAsync(CancellationToken.None);

        // Act

        await executor.StopAsync(CancellationToken.None);
        service.Invocations.Clear();

        await stateProvider.SetApplicationStateAsync(new ApplicationState {Status = ApplicationStatus.Active});
        await Task.Delay(100, TestContext.Current.CancellationToken);

        // Assert

        service
            .Verify(m => m.UpdateStateAsync(It.IsAny<ApplicationState>(), CancellationToken.None), Times.Never);
        service
            .Verify(m => m.StopAsync(CancellationToken.None), Times.Never);
    }

    #endregion
}
