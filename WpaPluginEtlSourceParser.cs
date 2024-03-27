using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Performance.SDK;
using Microsoft.Performance.SDK.Extensibility.SourceParsing;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wpa_plugin_etl.Tables;

namespace wpa_plugin_etl
{
    public class WpaPluginEtlSourceParser : ISourceParser<ReadGPCEvent, WpaPluginEtlSourceParser, string>
    {
        private readonly string[] filePaths;

        public DataSourceInfo DataSourceInfo { get; private set; }

        public string Id = nameof(WpaPluginEtlSourceParser);

        public Type DataElementType => typeof(ReadGPCEvent);

        public Type DataContextType => typeof(WpaPluginEtlSourceParser);

        public Type DataKeyType => typeof(string);

        public int MaxSourceParseCount => 1;

        string ISourceParserDescriptor.Id => nameof(WpaPluginEtlSourceParser);

        public void PrepareForProcessing(bool allEventsConsumed, IReadOnlyCollection<string> requestedDataKeys) {}

        public WpaPluginEtlSourceParser(string[] filePaths)
        {
            this.filePaths = filePaths;
        }

        public void ProcessSource(ISourceDataProcessor<ReadGPCEvent, WpaPluginEtlSourceParser, string> dataProcessor, ILogger logger, IProgress<int> progress, CancellationToken cancellationToken)
        {
            if(filePaths.Length == 0) { return; }

            Timestamp startTime = Timestamp.MaxValue;
            Timestamp endTime = Timestamp.MinValue;
            DateTime firstEvent = DateTime.MinValue;

            foreach (var path in this.filePaths)
            {
                using (var source = new ETWTraceEventSource(path))
                {
                    var parser = new DynamicTraceEventParser(source);
                    parser.All += delegate (TraceEvent data)
                    {
                        DateTime time = data.TimeStamp;
                        Timestamp stamp = Timestamp.FromNanoseconds(time.Ticks * 100);

                        if (stamp < startTime)
                        {
                            startTime = stamp;
                            firstEvent = time;
                        }
                        if (stamp > endTime)
                        {
                            endTime = stamp;
                        }

                        if (data.ProviderName.IndexOf("WindowsPerf Driver") != -1)
                        {
                            ReadGPCEvent ev = new ReadGPCEvent
                            {
                                Key = data.ProviderName,
                                Core = (ulong)(long)data.PayloadValue(0),
                                Event = String.Format("{0:X8}", (uint)(int)data.PayloadValue(1)),
                                EventIdx = (uint)(int)data.PayloadValue(1),
                                EventNote = "",
                                GPCIdx = (uint)(int)data.PayloadValue(2),
                                Time = Timestamp.FromNanoseconds(stamp.ToNanoseconds - startTime.ToNanoseconds),
                                Value = (ulong)(long)data.PayloadValue(3),
                            };

                            _ = dataProcessor.ProcessDataElement(ev, this, cancellationToken);
                        }
                        else if (data.ProviderName.IndexOf("WindowsPerf App") != -1)
                        {
                            ReadGPCEvent ev = new ReadGPCEvent
                            {
                                Key = data.ProviderName,
                                Core = (ulong)(long)data.PayloadValue(0),
                                Event = (String)data.PayloadValue(1),
                                EventIdx = (uint)(int)data.PayloadValue(2),
                                EventNote = (String)data.PayloadValue(3),
                                GPCIdx = 0,
                                Time = Timestamp.FromNanoseconds(stamp.ToNanoseconds - startTime.ToNanoseconds),
                                Value = (ulong)(long)data.PayloadValue(4),
                            };

                            _ = dataProcessor.ProcessDataElement(ev, this, cancellationToken);
                        }
                    };
                    source.Process();
                }
            }

            DataSourceInfo = new DataSourceInfo(0, (endTime - startTime).ToNanoseconds, firstEvent.ToUniversalTime());

            progress.Report(100);
        }
    }
}
