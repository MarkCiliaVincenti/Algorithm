using System;
using System.Collections.Generic;
using Eocron.Serialization.Tests.Helpers;
using Eocron.Serialization.Tests.Models.Json;
using NUnit.Framework;

namespace Eocron.Serialization.Tests
{
    [TestFixture]
    public class JsonSerializationTests : SerializationTestSuit<JsonTestModel>
    {
        public override ISerializationConverter GetConverter()
        {
            return SerializationConverter.Json;
        }

        public override JsonTestModel CreateTestModel(string path)
        {
            return new JsonTestModel
            {
                Dictionary = new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                },
                TimeSpan = TimeSpan.FromSeconds(3),
                Nullable = null,
                Double = 1.4d,
                Integer = 123,
                List = new List<int> { 1, 2, 3 },
                Array = new long[] { 2, 3, 4 },
                EmptyArray = new int[0],
                EmptyList = new List<int>(),
                FooBarString = "foobar",
                Struct = new JsonTestStruct()
                {
                    Value = 234
                },
                DateTime = new DateTime(2022, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                NullReference = null,
                Boolean = true,
                Long = 456,
                Guid = Guid.Parse("1a4c5b27-3881-4330-a13b-f709c004bbc4"),
                Enum = JsonTestEnum.Three
            };
        }

        [Test]
        public void CheckSerializeAndDeserializeByText()
        {
            AssertSerializeAndDeserializeByText(null);
        }

        [Test]
        [TestCase("TestData/Json/TestModelWithoutBOM.json")]
        public void CheckDeserializedModelEqualTo(string path)
        {
            AssertDeserializedFromTextModelEqualTo(path);
        }

        [Test]
        [TestCase("TestData/Json/TestModelWithoutBOM.json")]
        public void CheckSerializedTextEqualTo(string path)
        {
            AssertSerializedTextEqualTo(path);
        }

        [Test]
        [TestCase("TestData/Json/TestModelWithoutBOM.json")]
        public void CheckSerializedBytesEqualTo(string path)
        {
            AssertSerializedBytesEqualTo(path, false);
        }
    }
}