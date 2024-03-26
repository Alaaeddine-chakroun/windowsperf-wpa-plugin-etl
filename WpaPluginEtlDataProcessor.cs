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
        private IReadOnlyList<Tuple<string, DateTime, Timestamp, ReadGPCEventDriver>> driver_fileContent;
        private IReadOnlyList<Tuple<string, DateTime, Timestamp, ReadGPCEventApp>> app_fileContent;
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

            var driverList = new List<Tuple<string, DateTime, Timestamp, ReadGPCEventDriver>>();
            var appList = new List<Tuple<string, DateTime, Timestamp, ReadGPCEventApp>>();

            foreach (var path in this.filePaths)
            {
                using (var source = new ETWTraceEventSource(path))
                {
                    var parser = new DynamicTraceEventParser(source);
                    parser.All += delegate (TraceEvent data)
                    {
                        if (data.ProviderName.IndexOf("WindowsPerf Driver") != -1)
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

                            ReadGPCEventDriver @event = new ReadGPCEventDriver();
                            @event.Core = (ulong)(long)data.PayloadValue(0);
                            @event.Event = (uint)(int)data.PayloadValue(1);
                            @event.GPCIdx = (uint)(int)data.PayloadValue(2);
                            @event.Value = (ulong)(long)data.PayloadValue(3);

                            driverList.Add(new Tuple<String, DateTime, Timestamp, ReadGPCEventDriver>(
                                String.Format("{0:X8}",@event.Event),
                                data.TimeStamp,
                                Timestamp.FromNanoseconds(stamp.ToNanoseconds - startTime.ToNanoseconds),
                                @event));
                        } else if(data.ProviderName.IndexOf("WindowsPerf App") != -1)
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

                            ReadGPCEventApp @event = new ReadGPCEventApp();
                            @event.Core = (ulong)(long)data.PayloadValue(0);
                            @event.Event = (String)data.PayloadValue(1);
                            @event.EventIdx = (uint)(int)data.PayloadValue(2);
                            @event.EventNote = (String)data.PayloadValue(3);
                            @event.Value = (ulong)(long)data.PayloadValue(4);

                            appList.Add(new Tuple<String, DateTime, Timestamp, ReadGPCEventApp>(
                                String.Format("{0:X8}", @event.Event),
                                data.TimeStamp,
                                Timestamp.FromNanoseconds(stamp.ToNanoseconds - startTime.ToNanoseconds),
                                @event));
                        }
                    };
                    source.Process();
                }

                this.dataSourceInfo = new DataSourceInfo(0, (endTime - startTime).ToNanoseconds, firstEvent.ToUniversalTime());
            }

            this.driver_fileContent = new List<Tuple<string, DateTime, Timestamp, ReadGPCEventDriver>>(driverList);
            this.app_fileContent = new List<Tuple<string, DateTime, Timestamp, ReadGPCEventApp>>(appList);

            this.dataSourceInfo = new DataSourceInfo(0, (endTime - startTime).ToNanoseconds, firstEvent.ToUniversalTime());

            progress.Report(100);
            return Task.CompletedTask;
        }

        protected override void BuildTableCore(
            TableDescriptor tableDescriptor,
            ITableBuilder tableBuilder)
        {
            var type = tableDescriptor.ExtendedData["Type"] as Type;
            
            if (type != null && tableDescriptor.Guid == ReadGPCDriverTable.TableDescriptor.Guid)
            {
                var table = InstantiateDriverTable(type);
                table.Build(tableBuilder);
            } else if(type != null && tableDescriptor.Guid == ReadGPCAppTable.TableDescriptor.Guid)
            {
                var table = InstantiateAppTable(type);
                table.Build(tableBuilder);
            }
        }

        private ReadGPCDriverTable InstantiateDriverTable(Type tableType)
        {
            var instance = Activator.CreateInstance(tableType, new[] { this.driver_fileContent, });
            return (ReadGPCDriverTable)instance;
        }

        private ReadGPCAppTable InstantiateAppTable(Type tableType)
        {
            var instance = Activator.CreateInstance(tableType, new[] { this.app_fileContent, });
            return (ReadGPCAppTable)instance;
        }
    }
}
