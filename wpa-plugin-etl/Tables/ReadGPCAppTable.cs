﻿// BSD 3-Clause License
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
using System;
using System.Collections.Generic;
using Microsoft.Performance.SDK.Extensibility;

namespace wpa_plugin_etl.Tables
{
    [Table]
    public static class ReadGPCAppTable
    {
        public static readonly DataCookerPath dataCookerPath = DataCookerPath.ForSource(nameof(WpaPluginEtlSourceParser), nameof(ReadGPCAppDataCooker));

        public static TableDescriptor TableDescriptor => new TableDescriptor(
                    Guid.Parse("{CC0CDEC2-710D-4955-8C3C-CEBF606960D2}"),
                    "WindowsPerf GPC Data from App",
                    "GPC data gathered with WindowsPerf App",
                    "PMU from WindowsPerf",
                    requiredDataCookers: new List<DataCookerPath> { ReadGPCAppTable.dataCookerPath });

        private static readonly ColumnConfiguration EventIdxColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{58A54596-0A1A-4F59-91E5-4F231F80F06E}"), "Event Index", "The raw index of the event"),
           new UIHints { Width = 150 });

        private static readonly ColumnConfiguration TimeColumn = new ColumnConfiguration(
           new ColumnMetadata(new Guid("{61979965-B0B8-4529-AE52-3C6E2A58BA8A}"), "Time", "The time of the event"),
           new UIHints
           {
               IsVisible = false,
           });

        private static readonly ColumnConfiguration CoreColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{B8F574DD-1DAC-44AD-A623-4B39D39A09F6}"), "Core", "The CPU core that generated this event"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration EventColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{15C1CB95-FA17-4C10-8EE9-1851C8992D8E}"), "Event Name", "The human readable name of the event"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration EventNoteColumn = new ColumnConfiguration(
             new ColumnMetadata(new Guid("{C646628B-DD05-489A-9DF1-A1BB2E62561D}"), "Event Note", "The event note"),
             new UIHints
             {
                 Width = 150
             });

        private static readonly ColumnConfiguration ValueColumn = new ColumnConfiguration(
         new ColumnMetadata(new Guid("{30971239-377C-443C-B00D-EE8047E59B28}"), "Value", "The value read"),
         new UIHints
         {
             AggregationMode = AggregationMode.Sum,
             Width = 150
         });

        public static void Build(ITableBuilder tableBuilder, IReadOnlyList<ReadGPCEvent> data)
        {
            var baseProjection = Projection.Index(data);

            var eventProjection = baseProjection.Compose(x => x.Event);
            var timeProjection = baseProjection.Compose(x => x.Time);

            var coreProjection = baseProjection.Compose(x => x.Core);
            var eventIdxProjection = baseProjection.Compose(x => x.EventIdx);
            var eventNoteProjection = baseProjection.Compose(x => x.EventNote);
            var valueProjection = baseProjection.Compose(x => x.Value);

            var config = new TableConfiguration("App PMU Data")
            {
                Columns = new[]
                {
                    EventColumn,
                    CoreColumn,
                    TableConfiguration.PivotColumn,
                    TableConfiguration.LeftFreezeColumn,
                    EventIdxColumn,
                    EventNoteColumn,
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
               .AddColumn(TimeColumn, timeProjection)
               .AddColumn(CoreColumn, coreProjection)
               .AddColumn(EventIdxColumn, eventIdxProjection)
               .AddColumn(EventNoteColumn, eventNoteProjection)
               .AddColumn(ValueColumn, valueProjection);

        }

        public static void BuildTable(ITableBuilder tableBuilder, IDataExtensionRetrieval dataExtensionRetrieval)
        {
            IReadOnlyList<ReadGPCEvent> lineItems = dataExtensionRetrieval.QueryOutput<IReadOnlyList<ReadGPCEvent>>(
                    new DataOutputPath(ReadGPCAppDataCooker.DataCookerPath, nameof(ReadGPCAppDataCooker.Events))
                );
            if (lineItems.Count == 0)
            {
                return;
            }

            Build(tableBuilder, lineItems);
        }
    }
}
