using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using NUnit.Framework;

namespace Eocron.Serialization.Tests.Performance
{
    public abstract class SerializationPerformanceTestsBase<TTests, TModel>
    {
        protected readonly string _serializedText;
        protected readonly byte[] _serializedBytes;

        protected readonly ISerializationConverter _converter;
        protected readonly TModel _model;

        protected SerializationPerformanceTestsBase()
        {
            _converter = GetConverter();
            _model = GetTestModel();
            _serializedText = _converter.SerializeToString(_model);
            _serializedBytes = _converter.SerializeToBytes(_model);
        }

        public abstract ISerializationConverter GetConverter();

        public abstract TModel GetTestModel();

        public void SerializeBinary()
        {
            _converter.SerializeToBytes(_model);
        }

        public void DeserializeBinary()
        {
            _converter.Deserialize<TModel>(_serializedBytes);
        }

        public void DeserializeText()
        {
            _converter.Deserialize<TModel>(_serializedText);
        }

        public void SerializeText()
        {
            _converter.SerializeToString(_model);
        }

        [Test]
        [Explicit]
        public void Run()
        {
            BenchmarkRunner.Run<TTests>(new DebugInProcessConfig());
        }
    }
}