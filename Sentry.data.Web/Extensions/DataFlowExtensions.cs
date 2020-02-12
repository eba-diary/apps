using Newtonsoft.Json;
using Sentry.data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Sentry.data.Web
{
    public static class DataFlowExtensions
    {
        public static List<DFModel> ToModelList(this List<Core.DataFlowDto> dtoList)
        {
            List<DFModel> modelList = new List<DFModel>();
            foreach (Core.DataFlowDto dto in dtoList)
            {
                modelList.Add(dto.ToModel());
            }
            return modelList;
        }
        public static DFModel ToModel(this Core.DataFlowDto dto)
        {
            return new DFModel(dto) { };
        }

        public static Core.DataFlowDto ToDto(this DataFlowModel model)
        {
            Core.DataFlowDto dto = new Core.DataFlowDto
            {
                Id = model.DataFlowId,
                Name = "Blah",
                CreatedBy = model.CreatedBy,
                CreateDTM = model.CreatedDTM,
                IngestionType = model.IngestionType,
                IsCompressed = model.IsCompressed
            };

            if (model.SchemaMaps != null)
            {
                dto.SchemaMap = model.SchemaMaps.ToDto();
            }

            if (model.RetrieverJob != null)
            {
                dto.RetrieverJob = model.RetrieverJob.First().ToDto();
            }

            if (model.CompressionJob != null)
            {
                dto.CompressionJob = model.CompressionJob.First().ToDto();
            }

            dto.DFQuestionnaire = JsonConvert.SerializeObject(dto);

            return dto;
        }

        private static Core.CompressionJobDto ToDto(this CompressionModel model)
        {
            return new Core.CompressionJobDto()
            {
                CompressionType = (CompressionTypes)Enum.Parse(typeof(CompressionTypes), model.CompressionType),
                FileNameExclusionList = model.FileNameExclusionList
            };
        }

        public static List<Core.SchemaMapDto> ToDto(this List<SchemaMapModel> modelList)
        {
            List<Core.SchemaMapDto> dtoList = new List<Core.SchemaMapDto>();

            foreach(SchemaMapModel model in modelList)
            {
                dtoList.Add(model.ToDto());
            }

            return dtoList;
        }

        public static Core.SchemaMapDto ToDto(this SchemaMapModel model)
        {
            Core.SchemaMapDto dto = new Core.SchemaMapDto
            {
                Id = model.Id,
                SchemaId = model.SelectedSchema,
                DatasetId = model.SelectedDataset,
                SearchCriteria = model.SearchCriteria
            };

            return dto;
        }

        public static Core.RetrieverJobDto ToDto(this JobModel model)
        {
            Core.RetrieverJobDto dto = new Core.RetrieverJobDto()
            {
                DataSourceId = model.SelectedDataSource,
                DataSourceType = model.SelectedSourceType,
                IsCompressed = false, //for the data flow compression is handled outside of retriever job logic
                CreateCurrentFile = model.CreateCurrentFile,
                DatasetFileConfig = 0, //jobs for the data flow are linked via data flow id not datasetfileconfig
                FileNameExclusionList = null,
                FileSchema = 0,
                FtpPatrn = model.FtpPattern,
                HttpRequestBody = model.HttpRequestBody,
                JobId = 0,
                RelativeUri = model.RelativeUri,
                RequestDataFormat = model.SelectedRequestDataFormat,
                RequestMethod = model.SelectedRequestMethod,
                Schedule = model.Schedule,
                SearchCriteria = model.SearchCriteria,
                TargetFileName = model.TargetFileName
        };

            return dto;
        }
    }
}