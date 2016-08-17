﻿using System;
using FakeItEasy;
using Magic.Net;
using NUnit.Framework;
using NUnit.Magic.Net.Test.Helper;

namespace NUnit.Magic.Net.Test
{
    [TestFixture]
    public class NetConnectionTest
    {
        [Test, Category("receive data package")]
        public void NetConnection_ReceivedQueue_OnReceivedDataPackage_Ok()
        {
            IDataPackageHandler dataPackageHandler = A.Fake<IDataPackageHandler>();
            INetConnectionAdapter adapter = A.Fake<INetConnectionAdapter>();
            TestNetConnection connection = new TestNetConnection(adapter, dataPackageHandler);
            A.CallTo(() => adapter.IsConnected).Returns(true);

            var package = new NetDataPackage(new byte[] { 1, 1, 0, 0, 0 });
            connection.AddAddToReceivedDataQueue(package);

            connection.CallDequeueReceivedData();

            A.CallTo(() => dataPackageHandler.ReceiveCommand(package)).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Test, Category("data package version")]
        public void When_Version_Is_not_1()
        {
            var connection = A.Fake<TestNetConnection>(o => o.CallsBaseMethods());
            var buffer = new NetDataPackage(new byte[] { 20, 1, 0, 0, 0 });
            connection.AddAddToReceivedDataQueue(buffer);

            Assert.Throws<NotSupportedException>(connection.CallDequeueReceivedData);
        }
    }
    
    public class NetConnectionDataPackageTypeTests
    {
        private TestNetConnection _connection;
        private IDataPackageHandler _dataPackageHandler;

        [SetUp]
        public void Init()
        {
            _dataPackageHandler = A.Fake<IDataPackageHandler>();
            INetConnectionAdapter adapter = A.Fake<INetConnectionAdapter>();
            _connection = new TestNetConnection(adapter, _dataPackageHandler);
            A.CallTo(() => adapter.IsConnected).Returns(true);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_unknown()
        {
            var buffer = new NetDataPackage(new byte[] { 1, 255, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(buffer);
            var exception = Assert.Throws<NetCommandException>(_connection.CallDequeueReceivedData);

            Assert.NotNull(exception);
            Assert.AreEqual("package.PackageContentType 255 unknown.", exception.Message);
            Assert.AreEqual(NetCommandExceptionReasonses.UnknownPackageContentType, exception.Reasonses);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_1()
        {
            var package = new NetDataPackage(new byte[] { 1, 1, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(package);
            _connection.CallDequeueReceivedData();
            A.CallTo(() => _dataPackageHandler.ReceiveCommand(package)).MustHaveHappened(Repeated.AtLeast.Once);
        }

        [Test, Category("package type")]
        public void When_DataPackage_Type_Is_10()
        {
            var package = new NetDataPackage(new byte[] { 1, 10, 0, 0, 0 });
            _connection.AddAddToReceivedDataQueue(package);
            _connection.CallDequeueReceivedData();

            A.CallTo(() => _dataPackageHandler.ReceiveCommandStream(package)).MustHaveHappened(Repeated.AtLeast.Once);
        }
    }
}