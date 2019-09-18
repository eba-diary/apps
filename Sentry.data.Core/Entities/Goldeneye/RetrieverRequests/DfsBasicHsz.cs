﻿using System;
using System.Linq;
using System.IO;

namespace Sentry.data.Core
{
    public class DfsBasicHsz : DfsSource
    {
        public DfsBasicHsz()
        {
            IsUriEditable = false;
            BaseUri = new Uri($"{BaseUri.ToString()}DatasetLoader/");
        }

        //Setting Discriminator Value for NHibernate
        public override string SourceType
        {
            get
            {
                return "DFSBasicHsz";
            }
        }
        public override Uri CalcRelativeUri(RetrieverJob Job)
        {
            string storageCode = Job.DatasetConfig.GetStorageCode();

            Uri u = new Uri(Path.Combine(new string[] { BaseUri.ToString(), storageCode}).ToString());

            return u;
        }
    }
}