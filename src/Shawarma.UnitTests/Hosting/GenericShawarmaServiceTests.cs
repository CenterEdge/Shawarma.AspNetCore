using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Shawarma.AspNetCore;
using Shawarma.AspNetCore.Hosting;
using Xunit;

namespace Shawarma.UnitTests.Hosting
{
    public class GenericShawarmaServiceTests
    {
        #region IsRunning

        [Fact]
        public void IsRunning_OnInit_False()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };

            // Act

            var result = mock.Object.IsRunning;

            // Assert

            Assert.False(result);
        }

        #endregion

        #region UpdateStateAsync

        [Fact]
        public async Task UpdateStateAsync_NullState_ArgumentNullException()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };

            // Act/Assert

            await Assert.ThrowsAsync<ArgumentNullException>(() => mock.Object.UpdateStateAsync(null, CancellationToken.None));
        }

        [Fact]
        public async Task UpdateStateAsync_ActiveStateNotRunning_CallsStartInternal()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };
            mock
                .Protected()
                .Setup<Task>("StartInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);
            mock
                .Protected()
                .Setup<Task>("StopInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            // Act

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);

            // Assert

            mock
                .Protected()
                .Verify<Task>("StartInternalAsync", Times.Once(), It.IsAny<CancellationToken>());
            mock
                .Protected()
                .Verify<Task>("StopInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task UpdateStateAsync_ActiveStateNotRunning_SetsIsRunning()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            // Act

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);

            // Assert

            Assert.True(mock.Object.IsRunning);
        }

        [Fact]
        public async Task UpdateStateAsync_ActiveStateRunning_DoesNothing()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };
            mock
                .Protected()
                .Setup<Task>("StartInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);
            mock
                .Protected()
                .Setup<Task>("StopInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);
            Assert.True(mock.Object.IsRunning);
            mock.Invocations.Clear();

            // Act

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);

            // Assert

            mock
                .Protected()
                .Verify<Task>("StartInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            mock
                .Protected()
                .Verify<Task>("StopInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            Assert.True(mock.Object.IsRunning);
        }

        [Fact]
        public async Task UpdateStateAsync_InactiveStateRunning_CallsStopInternal()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };
            mock
                .Protected()
                .Setup<Task>("StartInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);
            mock
                .Protected()
                .Setup<Task>("StopInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);
            Assert.True(mock.Object.IsRunning);
            mock.Invocations.Clear();

            state.Status = ApplicationStatus.Inactive;

            // Act

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);

            // Assert

            mock
                .Protected()
                .Verify<Task>("StartInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            mock
                .Protected()
                .Verify<Task>("StopInternalAsync", Times.Once(), It.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task UpdateStateAsync_ActiveStateRunning_UnSetsIsRunning()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);
            Assert.True(mock.Object.IsRunning);
            mock.Invocations.Clear();

            state.Status = ApplicationStatus.Inactive;

            // Act

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);

            // Assert

            Assert.False(mock.Object.IsRunning);
        }

        [Fact]
        public async Task UpdateStateAsync_InactiveStateNotRunning_DoesNothing()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };
            mock
                .Protected()
                .Setup<Task>("StartInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);
            mock
                .Protected()
                .Setup<Task>("StopInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Inactive
            };

            // Act

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);

            // Assert

            mock
                .Protected()
                .Verify<Task>("StartInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            mock
                .Protected()
                .Verify<Task>("StopInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            Assert.False(mock.Object.IsRunning);
        }

        #endregion

        #region StopAsync

        [Fact]
        public async Task StopAsync_InactiveStateRunning_CallsStopInternal()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };
            mock
                .Protected()
                .Setup<Task>("StartInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);
            mock
                .Protected()
                .Setup<Task>("StopInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);
            Assert.True(mock.Object.IsRunning);
            mock.Invocations.Clear();

            state.Status = ApplicationStatus.Inactive;

            // Act

            await mock.Object.StopAsync(CancellationToken.None);

            // Assert

            mock
                .Protected()
                .Verify<Task>("StartInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            mock
                .Protected()
                .Verify<Task>("StopInternalAsync", Times.Once(), It.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task StopAsync_ActiveStateRunning_UnSetsIsRunning()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Active
            };

            await mock.Object.UpdateStateAsync(state, CancellationToken.None);
            Assert.True(mock.Object.IsRunning);
            mock.Invocations.Clear();

            state.Status = ApplicationStatus.Inactive;

            // Act

            await mock.Object.StopAsync(CancellationToken.None);

            // Assert

            Assert.False(mock.Object.IsRunning);
        }

        [Fact]
        public async Task StopAsync_InactiveStateNotRunning_DoesNothing()
        {
            // Arrange

            var logger = new Mock<ILogger>();

            var mock = new Mock<GenericShawarmaService>(logger.Object)
            {
                CallBase = true
            };
            mock
                .Protected()
                .Setup<Task>("StartInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);
            mock
                .Protected()
                .Setup<Task>("StopInternalAsync", It.IsAny<CancellationToken>())
                .Returns(Task.CompletedTask);

            var state = new ApplicationState
            {
                Status = ApplicationStatus.Inactive
            };

            // Act

            await mock.Object.StopAsync(CancellationToken.None);

            // Assert

            mock
                .Protected()
                .Verify<Task>("StartInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            mock
                .Protected()
                .Verify<Task>("StopInternalAsync", Times.Never(), It.IsAny<CancellationToken>());
            Assert.False(mock.Object.IsRunning);
        }

        #endregion
    }
}
