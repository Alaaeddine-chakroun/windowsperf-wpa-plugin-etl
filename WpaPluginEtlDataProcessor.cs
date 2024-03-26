// BSD 3-Clause License
//
// Copyright (c) 2024, Arm Limited
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.Performance.SDK.Processing;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using wpa_plugin_etl.Tables;
using Microsoft.Performance.SDK;

namespace wpa_plugin_etl
{
    internal class WpaPluginEtlDataProcessor : CustomDataProcessor
    {
        private readonly string[] filePaths;
        private IReadOnlyList<Tuple<string, DateTime, Timestamp, Timestamp, ReadGPCEvent>> fileContent;
        private DataSourceInfo dataSourceInfo;

        public WpaPluginEtlDataProcessor(
           string[] filePaths,
           ProcessorOptions options,
           IApplicationEnvironment applicationEnvironment,
           IProcessorEnvironment processorEnvironment)
            : base(options, applicationEnvironment, processorEnvironment)
        {
            this.filePaths = filePaths;
        }

        public override DataSourceInfo GetDataSourceInfo()
        {
            return this.dataSourceInfo;
        }

        protected override Task ProcessAsyncCore(
           IProgress<int> progress,
           CancellationToken cancellationToken)
        {
            Timestamp startTime = Timestamp.MaxValue;
            Timestamp endTime = Timestamp.MinValue;
            DateTime firstEvent = DateTime.MinValue;

            var list = new List<Tuple<string, DateTime, Timestamp, Timestamp, ReadGPCEvent>>();
            foreach (var path in this.filePaths)
            {
                long lastEndTime = 0;
                using (var source = new ETWTraceEventSource(path))
                {
                    var parser = new DynamicTraceEventParser(source);
                    parser.All += delegate (TraceEvent data)
                    {
                        if (data.ProviderName.IndexOf("WindowsPerf") != -1)
                        {
                            //Debug.WriteLine("GOT EVENT: " + data.ToString());

                            DateTime time = data.TimeStamp;
                            Timestamp stamp = Timestamp.FromNanoseconds(time.Ticks * 100);

                            if (stamp < startTime)
                            {
                                startTime = stamp;
                                lastEndTime = 0;
                                firstEvent = time;
                            }
                            if (stamp > endTime)
                            {
                                endTime = stamp;
                            }

                            ReadGPCEvent @event = new ReadGPCEvent();
                            @event.Core = (ulong)(long)data.PayloadValue(0);
                            @event.Event = (uint)(int)data.PayloadValue(1);
                            @event.GPCIdx = (uint)(int)data.PayloadValue(2);
                            @event.Value = (ulong)(long)data.PayloadValue(3);

                            list.Add(new Tuple<String, DateTime, Timestamp, Timestamp, ReadGPCEvent>(
                                String.Format("{0:X8}",@event.Event),
                                data.TimeStamp,
                                Timestamp.FromNanoseconds(lastEndTime),
                                Timestamp.FromNanoseconds(stamp.ToNanoseconds - startTime.ToNanoseconds),
                                @event));
                            lastEndTime = stamp.ToNanoseconds - startTime.ToNanoseconds;
                        }
                    };
                    source.Process();
                }

                this.dataSourceInfo = new DataSourceInfo(0, (endTime - startTime).ToNanoseconds, firstEvent.ToUniversalTime());
            }

            this.fileContent = new List<Tuple<string, DateTime, Timestamp, Timestamp, ReadGPCEvent>>(list);
            
            this.dataSourceInfo = new DataSourceInfo(0, (endTime - startTime).ToNanoseconds, firstEvent.ToUniversalTime());

            progress.Report(100);
            return Task.CompletedTask;
        }

        protected override void BuildTableCore(
            TableDescriptor tableDescriptor,
            ITableBuilder tableBuilder)
        {
            var type = tableDescriptor.ExtendedData["Type"] as Type;

            if (type != null)
            {
                var table = InstantiateTable(type);
                table.Build(tableBuilder);
            }
        }

        private ReadGPCTable InstantiateTable(Type tableType)
        {
            var instance = Activator.CreateInstance(tableType, new[] { this.fileContent, });
            return (ReadGPCTable)instance;
        }
    }
}
