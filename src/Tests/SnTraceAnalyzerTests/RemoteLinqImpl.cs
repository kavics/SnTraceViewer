using Aqua.Dynamic;
using Remote.Linq;
using Remote.Linq.Expressions;
using SenseNet.Diagnostics.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SnTraceAnalyzerTests
{
    /* ======================================================================== Common */

    public interface IQueryService
    {
        IEnumerable<DynamicObject> ExecuteQuery(Expression queryExpression, Reader testReader);
    }

    /* ======================================================================== Client */

    /// <summary>
    /// Client frontend object.
    /// </summary>
    public class AnalyzatorServer : IDisposable
    {
        private readonly Func<Expression, IEnumerable<DynamicObject>> _dataProvider;

        public AnalyzatorServer(string uri, Reader testReader)
        {
            _dataProvider = expression =>
            {
                // setup service connectivity
                IQueryService service = CreateServerConection(uri);
                // send expression to service and get back results
                IEnumerable<DynamicObject> result = service.ExecuteQuery(expression, testReader);
                return result;
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Entry> Entries => RemoteQueryable.Factory.CreateQueryable<Entry>(_dataProvider);

        private IQueryService CreateServerConection(string uri)
        {
            return new ServerProxy();
        }

    }
    internal class ServerProxy : IQueryService
    {
        public IEnumerable<DynamicObject> ExecuteQuery(Expression queryExpression, Reader testReader)
        {
            var channel = new CommunicationChannel(testReader);

            // simulate request/response
            var response = channel.Communicate(queryExpression);

            return response;
        }
    }

    /* ======================================================================== Server */

    internal class CommunicationChannel
    {
        string _fileName = @"D:\tempfile.xml";
        Reader _testReader;

        public CommunicationChannel(Reader testReader)
        {
            _testReader = testReader;
        }

        public IEnumerable<DynamicObject> Communicate(Expression queryExpression)
        {
            SerializeExpression(queryExpression, _fileName);

            // send ---> receive

            var deserializedExpression = DeserializeExpression(_fileName);

            // simulate server
            var server = new QueryService(new SnTraceDataContext(_testReader));
            var result = server.ExecuteQuery(deserializedExpression, _testReader);

            return result;
        }

        private void SerializeExpression(Expression expression, string fileName)
        {
            var dcs = new DataContractSerializer(typeof(Expression));
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8))
                {
                    writer.WriteStartDocument();
                    dcs.WriteObject(writer, expression);
                }
            }
        }
        private Expression DeserializeExpression(string fileName)
        {
            Expression result;

            DataContractSerializer dcs = new DataContractSerializer(typeof(Expression));
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max))
                result = (Expression)dcs.ReadObject(reader);

            return result;
        }
    }

    /* ======================================================================== Server */

    public class QueryService : IQueryService
    {
        // any linq provider e.g. entity framework, nhibernate, ...
        private IDataProvider _dataContext;

        public QueryService(IDataProvider dataContext)
        {
            _dataContext = dataContext;
        }

        public IEnumerable<DynamicObject> ExecuteQuery(Expression queryExpression, Reader testReader)
        {
            // Execute is an extension method provided by Remote.Linq
            return queryExpression.Execute(queryableProvider: (type) => { return _dataContext.GetQueryableByType(type); });
        }

        public void Dispose()
        {
            _dataContext.Dispose();
        }
    }
    public interface IDataProvider : IDisposable
    {
        IQueryable GetQueryableByType(Type type);
    }
    public class SnTraceDataContext : IDataProvider
    {
        private Reader _reader;

        public SnTraceDataContext(Reader reader)
        {
            _reader = reader;
        }

        public IQueryable GetQueryableByType(Type type)
        {
            if (type == typeof(Entry))
                return _reader.AsQueryable();
            throw new NotSupportedException($"SnTraceDataContext does not support this type: {type.FullName}.");
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }

}
