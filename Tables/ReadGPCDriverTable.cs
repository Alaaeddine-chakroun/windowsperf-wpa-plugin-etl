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

using Microsoft.Performance.SDK.Extensibility;
using Microsoft.Performance.SDK.Processing;
using System;
using System.Collections.Generic;

namespace wpa_plugin_etl.Tables
{
    [Table]
    public static class ReadGPCDriverTable
    {
        public static readonly DataCookerPath dataCookerPath = DataCookerPath.ForSource(nameof(WpaPluginEtlSourceParser), nameof(ReadGPCDriverDataCooker));

        public static TableDescriptor TableDescriptor => new TableDescriptor(
                    Guid.Parse("{E122471E-25A6-4F7F-BE6C-E62774FD0410}"),
                    "WindowsPerf GPC Data from Driver",
                    "GPC data gathered with WindowsPerf Driver",
                    "PMU from WindowsPerf",
                    requiredDataCookers: new List<DataCookerPath> { ReadGPCDriverTable.dataCookerPath });

        private static readonly ColumnConfiguration EventColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{E7907471-C6E2-41D2-AAA2-E4D790EB8676}"), "Event Index", "The raw index of the event"),
           new UIHints { Width = 150 });

        private static readonly ColumnConfiguration TimeColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{E3056A08-D44D-4CD6-8158-503BDAEF899C}"), "Time", "The time of the event"),
           new UIHints { 
               IsVisible = true,
           });

        private static readonly ColumnConfiguration CoreColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{229769EA-126A-4F29-8AB9-DACE3331FCD3}"), "Core", "The CPU core that generated this event"),
             new UIHints
             {
                 Width = 150
             });
        
        private static readonly ColumnConfiguration GPCIdxColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{2E057186-CE48-45DB-85C2-4948D332FCB7}"), "GPC Index", "The index of the GPC that the value was read from"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration ValueColumn = new ColumnConfiguration(
         new ColumnMetadata(new Guid("{54B4A016-9F78-4BAE-A0AB-AFDFBF33C3F1}"), "Value", "The value read"),
         new UIHints { 
             AggregationMode = AggregationMode.Sum,
             Width = 150 });

        public static void Build(ITableBuilder tableBuilder, IReadOnlyList<ReadGPCEvent> data)
        {
            var baseProjection = Projection.Index(data);

            var eventProjection = baseProjection.Compose(x => x.Event);
            var timeProjection = baseProjection.Compose(x => x.Time);

            var coreProjection = baseProjection.Compose(x => x.Core);
            var GPCIdxProjection = baseProjection.Compose(x => x.GPCIdx);
            var valueProjection = baseProjection.Compose(x => x.Value);
            
            var config = new TableConfiguration("Driver PMU Data")
            {
                Columns = new[]
                {
                    EventColumn,
                    CoreColumn,
                    TableConfiguration.PivotColumn,
                    TableConfiguration.LeftFreezeColumn,
                    GPCIdxColumn,
                    TimeColumn,
                    TableConfiguration.GraphColumn,
                    TableConfiguration.RightFreezeColumn,
                    ValueColumn,
                },
            };

            config.AddColumnRole(ColumnRole.StartTime, TimeColumn);

            _ = tableBuilder.AddTableConfiguration(config)
               .SetDefaultTableConfiguration(config)
               .SetRowCount(data.Count)
               .AddColumn(EventColumn, eventProjection)
               .AddColumn(CoreColumn, coreProjection)
               .AddColumn(GPCIdxColumn, GPCIdxProjection)
               .AddColumn(TimeColumn, timeProjection)
               .AddColumn(ValueColumn, valueProjection);            
            
        }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataExtensionRetrieval)
        {
            IReadOnlyList<ReadGPCEvent> lineItems = dataExtensionRetrieval.QueryOutput<IReadOnlyList<ReadGPCEvent>>(
                    new DataOutputPath(ReadGPCDriverDataCooker.DataCookerPath, nameof(ReadGPCDriverDataCooker.Events))
                );
            if (lineItems.Count == 0)
            {
                return;
            }

            Build(tableBuilder, lineItems);
        }
    }
}
