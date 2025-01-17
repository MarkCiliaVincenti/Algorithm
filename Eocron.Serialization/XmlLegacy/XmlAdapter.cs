﻿using System;
using System.IO;
using System.Xml;
using Eocron.Serialization.XmlLegacy.Document;
using Eocron.Serialization.XmlLegacy.Serializer;

namespace Eocron.Serialization.XmlLegacy
{
    /// <summary>
    /// Adapter for different type of legacy xml document formats (XmlDocument, XDocument)
    /// and different legacy xml serializers (XmlSerializer, DataContractSerializer, XmlObjectSerializer)
    /// </summary>
    /// <typeparam name="TDocument"></typeparam>
    public sealed class XmlAdapter<TDocument> : IXmlAdapter<TDocument>
    {
        private readonly IXmlSerializerAdapter _serializer;
        private readonly IXmlDocumentAdapter<TDocument> _documentAdapter;

        public XmlReaderSettings ReaderSettings { get; set; } = new XmlReaderSettings(){ IgnoreComments = true, IgnoreWhitespace = true };
        public XmlWriterSettings WriterSettings { get; set; } = new XmlWriterSettings() { Encoding = SerializationConverter.DefaultEncoding, Indent = SerializationConverter.DefaultIndent };

        public XmlAdapter(IXmlSerializerAdapter serializer, IXmlDocumentAdapter<TDocument> documentAdapter)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _documentAdapter = documentAdapter ?? throw new ArgumentNullException(nameof(documentAdapter));
        }

        public TDocument SerializeToDocument(Type type, object content)
        {
            var writer = _documentAdapter.CreateNewDocumentAndWriter(out var document);
            using (var w = writer)
            {
                _serializer.WriteObject(w, type, content);
            }
            _documentAdapter.AfterCreation(document);
            return document;
        }

        public object DeserializeFromDocument(Type type, TDocument document)
        {
            using var reader = _documentAdapter.CreateReader(document);
            return _serializer.ReadObject(reader, type);
        }

        public TDocument ReadDocumentFrom(StreamReader sourceStream)
        {
            using var reader = XmlReader.Create(sourceStream, ReaderSettings);
            var document = _documentAdapter.ReadFrom(reader);
            _documentAdapter.AfterCreation(document);
            return document;
        }

        public void WriteDocumentTo(StreamWriter targetStream, TDocument document)
        {
            var xmlTextWriter = XmlWriter.Create(targetStream, WriterSettings);
            _documentAdapter.WriteTo(document, xmlTextWriter);
            xmlTextWriter.Flush();
        }
    }
}
