using System.Collections.Generic;
using Lupusec2Mqtt.Lupusec.Dtos;
using Lupusec2Mqtt.Mqtt.Homeassistant;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Logging;


namespace Lupusec2MqttTests
{
    public class ConversionServiceTests
    {
        private ConversionService _conversionService;

        [SetUp]
        public void Setup()
        {
            var mockConfiguration = Mock.Of<IConfiguration>();
            var mockLogger = Mock.Of<ILogger>();
            _conversionService = new ConversionService(mockConfiguration, mockLogger);

        }

        public class GetDeviceClassDefaultValueTests
        {
            private ConversionService _conversionService;
            [SetUp]
            public void Setup()
            {
                var mockConfiguration = Mock.Of<IConfiguration>();
                var mockLogger = Mock.Of<ILogger>();
                _conversionService = new ConversionService(mockConfiguration, mockLogger);

            }
            [Test]
            public void WhenTypeIdIsFourThenDeviceClassIsWindow()
            {
                var actor = new LupusActorMock() { TypeId = 4 };
                var deviceClass = _conversionService.GetDeviceClassDefaultValue(actor);
                Assert.AreEqual(deviceClass, "window");
            }
            [Test]
            public void WhenTypeIdIs33ThenDeviceClassIsWindow()
            {
                var actor = new LupusActorMock() { TypeId = 33 };
                var deviceClass = _conversionService.GetDeviceClassDefaultValue(actor);
                Assert.AreEqual(deviceClass, "window");
            }
            [Test]
            public void WhenTypeIdIsNineThenDeviceClassIsMotion()
            {
                var actor = new LupusActorMock() { TypeId = 9 };
                var deviceClass = _conversionService.GetDeviceClassDefaultValue(actor);
                Assert.AreEqual(deviceClass, "motion");
            }
            [Test]
            public void WhenTypeIdIsElevenThenDeviceClassIsSmoke()
            {
                var actor = new LupusActorMock() { TypeId = 11 };
                var deviceClass = _conversionService.GetDeviceClassDefaultValue(actor);
                Assert.AreEqual(deviceClass, "smoke");
            }
            [Test]
            public void WhenTypeIdIsFiveThenDeviceClassIsMoisture()
            {
                var actor = new LupusActorMock() { TypeId = 5 };
                var deviceClass = _conversionService.GetDeviceClassDefaultValue(actor);
                Assert.AreEqual(deviceClass, "moisture");
            }
            [Test]
            public void WhenTypeIdIsNotHandledThenDeviceClassIsNull()
            {
                var actor = new LupusActorMock() { TypeId = 9999 };
                var deviceClass = _conversionService.GetDeviceClassDefaultValue(actor);
                Assert.AreEqual(deviceClass, null);
            }
        }

        public class GetStateByStatusTests
        {
            private ConversionService _conversionService;
            [SetUp]
            public void Setup()
            {
                var mockConfiguration = Mock.Of<IConfiguration>();
                var mockLogger = Mock.Of<ILogger>();
                _conversionService = new ConversionService(mockConfiguration, mockLogger);

            }
            [Test]
            public void WhenTypeIdIsFourAndStatusIsWEB_MSG_DC_OPENThenStateIsOn()
            {
                var actor = new LupusActorMock() { TypeId = 4 ,Status = "{WEB_MSG_DC_OPEN}" };
                var state = _conversionService.GetStateByStatus(actor,null);
                Assert.AreEqual(state, "ON");
            }
            [Test]
            public void WhenTypeIdIsFourAndStatusIsNotWEB_MSG_DC_OPENThenStateIsOn()
            {
                var actor = new LupusActorMock() { TypeId = 4, Status = "Test" };
                var state = _conversionService.GetStateByStatus(actor, null);
                Assert.AreEqual(state, "OFF");
            }
            [Test]
            public void WhenTypeIdI33AndStatusIsWEB_MSG_DC_OPENThenStateIsOn()
            {
                var actor = new LupusActorMock() { TypeId = 33, Status = "{WEB_MSG_DC_OPEN}" };
                var state = _conversionService.GetStateByStatus(actor, null);
                Assert.AreEqual(state, "ON");
            }
            [Test]
            public void WhenTypeIdIs33AndStatusIsNotWEB_MSG_DC_OPENThenStateIsOn()
            {
                var actor = new LupusActorMock() { TypeId = 33, Status = "Test" };
                var state = _conversionService.GetStateByStatus(actor, null);
                Assert.AreEqual(state, "OFF");
            }
            [Test]
            public void WhenTypeIdIsElevenAndStatusIsRPT_CID_111ThenStateIsOn()
            {
                var actor = new LupusActorMock() { TypeId = 11, Status = "{RPT_CID_111}" };
                var state = _conversionService.GetStateByStatus(actor, null);
                Assert.AreEqual(state, "ON");
            }
            [Test]
            public void WhenTypeIdIsElevenAndStatusIsNotRPT_CID_111ThenStateIsOn()
            {
                var actor = new LupusActorMock() { TypeId = 11, Status = "Test" };
                var state = _conversionService.GetStateByStatus(actor, null);
                Assert.AreEqual(state, "OFF");
            }
        }
        private class LupusActorMock:ILupusActor
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public int TypeId { get; set; }
            public string Status { get; set; }
        }
    }
}