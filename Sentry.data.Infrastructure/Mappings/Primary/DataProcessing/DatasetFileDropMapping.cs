﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Sentry.data.Core;

namespace Sentry.data.Infrastructure.Mappings.Primary
{
    public class DatasetFileDropMapping : ClassMapping<DatasetFileDrop>
    {
        public DatasetFileDropMapping()
        {
            this.Table("DatasetFileDrop");

            this.Id(x => x.DatasetFileDropID);

            ///Properties from abstract class <see cref="Sentry.data.Core.Entities.DataProcessing.BaseDatasetFileV2"/>
            this.Property(x => x.FileNME);
            this.Property(x => x.FlowExecutionGUID);
            this.Property(x => x.ObjectBucket);
            this.Property(x => x.ObjectKey);
            this.Property(x => x.ObjectVersionID);
            this.Property(x => x.ObjectETag);
            this.Property(x => x.DatasetID);
            this.Property(x => x.SchemaId);
            this.Property(x => x.CreateDTM);

            ///Add properties specific to this class
        }
    }
}