using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Schema.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Infrastructure.Tests
{
    [TestClass]
    public class GoogleBigQueryServiceTests : BaseInfrastructureUnitTest
    {
        [TestMethod]
        public void UpdateSchemaFields_CreateNewRevision()
        {
            JObject rawFields = GetData("GoogleBigQuery_BasicSchema.json");

            Mock<ISchemaService> schemaService = new Mock<ISchemaService>(MockBehavior.Strict);
            schemaService.Setup(x => x.GetLatestSchemaRevisionDtoBySchema(1)).Returns<SchemaRevisionDto>(null);
            schemaService.Setup(x => x.CreateAndSaveSchemaRevision(1, It.IsAny<List<BaseFieldDto>>(), $"GoogleBigQuery_{DateTime.Today:yyyyMMdd}", null))
                .Callback<int, List<BaseFieldDto>, string, string>((id, fields, name, json) =>
                {
                    Assert.AreEqual(7, fields.Count);

                    BaseFieldDto field = fields.First();
                    Assert.AreEqual("string_field", field.Name);
                    Assert.AreEqual(1, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, field.FieldType);
                    Assert.IsFalse(field.IsArray);

                    field = fields[1];
                    Assert.AreEqual("integer_field", field.Name);
                    Assert.AreEqual(2, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.BIGINT, field.FieldType);
                    Assert.IsFalse(field.IsArray);

                    field = fields[2];
                    Assert.AreEqual("date_field", field.Name);
                    Assert.AreEqual(3, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.DATE, field.FieldType);
                    Assert.IsFalse(field.IsArray);

                    field = fields[3];
                    Assert.AreEqual("decimal_field", field.Name);
                    Assert.AreEqual(4, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.DECIMAL, field.FieldType);
                    Assert.IsFalse(field.IsArray);
                    Assert.AreEqual(5, field.Precision);
                    Assert.AreEqual(2, field.Scale);

                    field = fields[4];
                    Assert.AreEqual("struct_field", field.Name);
                    Assert.AreEqual(5, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.STRUCT, field.FieldType);
                    Assert.IsTrue(field.IsArray);
                    Assert.IsTrue(field.HasChildren);
                    Assert.AreEqual(2, field.ChildFields.Count);

                    BaseFieldDto childField = field.ChildFields.First();
                    Assert.AreEqual("child_string_field", childField.Name);
                    Assert.AreEqual(6, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);

                    childField = field.ChildFields.Last();
                    Assert.AreEqual("child_struct_field", childField.Name);
                    Assert.AreEqual(7, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.STRUCT, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);
                    Assert.IsTrue(childField.HasChildren);
                    Assert.AreEqual(2, childField.ChildFields.Count);

                    BaseFieldDto grandChildField = childField.ChildFields.First();
                    Assert.AreEqual("grandchild_timestamp_field", grandChildField.Name);
                    Assert.AreEqual(8, grandChildField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.TIMESTAMP, grandChildField.FieldType);
                    Assert.IsFalse(grandChildField.IsArray);

                    grandChildField = childField.ChildFields.Last();
                    Assert.AreEqual("grandchild_float_field", grandChildField.Name);
                    Assert.AreEqual(9, grandChildField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, grandChildField.FieldType);
                    Assert.IsFalse(grandChildField.IsArray);

                    field = fields[5];
                    Assert.AreEqual("key_value_struct", field.Name);
                    Assert.AreEqual(10, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.STRUCT, field.FieldType);
                    Assert.IsTrue(field.IsArray);
                    Assert.IsTrue(field.HasChildren);
                    Assert.AreEqual(2, field.ChildFields.Count);

                    childField = field.ChildFields.First();
                    Assert.AreEqual("key", childField.Name);
                    Assert.AreEqual(11, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);

                    childField = field.ChildFields.Last();
                    Assert.AreEqual("value", childField.Name);
                    Assert.AreEqual(12, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);

                    field = fields[6];
                    Assert.AreEqual("string_field_2", field.Name);
                    Assert.AreEqual(13, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, field.FieldType);
                    Assert.IsFalse(field.IsArray);
                }).Returns(1);

            GoogleBigQueryService service = new GoogleBigQueryService(schemaService.Object);

            service.UpdateSchemaFields(1, (JArray)rawFields.SelectToken("fields"));

            schemaService.Verify();
        }

        [TestMethod]
        public void UpdateSchemaFields_UpdateRevision()
        {
            JObject rawFields = GetData("GoogleBigQuery_BasicSchema.json");

            Mock<ISchemaService> schemaService = new Mock<ISchemaService>(MockBehavior.Strict);
            schemaService.Setup(x => x.GetLatestSchemaRevisionDtoBySchema(1)).Returns(new SchemaRevisionDto() { RevisionId = 2 });
            schemaService.Setup(x => x.GetBaseFieldDtoBySchemaRevision(2)).Returns(new List<BaseFieldDto>()
            {
                GetField<VarcharFieldDto>("string_field", false, 1)
            });
            schemaService.Setup(x => x.CreateAndSaveSchemaRevision(1, It.IsAny<List<BaseFieldDto>>(), $"GoogleBigQuery_{DateTime.Today:yyyyMMdd}", null))
                .Callback<int, List<BaseFieldDto>, string, string>((id, fields, name, json) =>
                {
                    Assert.AreEqual(7, fields.Count);

                    BaseFieldDto field = fields.First();
                    Assert.AreEqual("string_field", field.Name);
                    Assert.AreEqual(1, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, field.FieldType);
                    Assert.IsFalse(field.IsArray);

                    field = fields[1];
                    Assert.AreEqual("integer_field", field.Name);
                    Assert.AreEqual(2, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.BIGINT, field.FieldType);
                    Assert.IsFalse(field.IsArray);

                    field = fields[2];
                    Assert.AreEqual("date_field", field.Name);
                    Assert.AreEqual(3, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.DATE, field.FieldType);
                    Assert.IsFalse(field.IsArray);

                    field = fields[3];
                    Assert.AreEqual("decimal_field", field.Name);
                    Assert.AreEqual(4, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.DECIMAL, field.FieldType);
                    Assert.IsFalse(field.IsArray);
                    Assert.AreEqual(5, field.Precision);
                    Assert.AreEqual(2, field.Scale);

                    field = fields[4];
                    Assert.AreEqual("struct_field", field.Name);
                    Assert.AreEqual(5, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.STRUCT, field.FieldType);
                    Assert.IsTrue(field.IsArray);
                    Assert.IsTrue(field.HasChildren);
                    Assert.AreEqual(2, field.ChildFields.Count);

                    BaseFieldDto childField = field.ChildFields.First();
                    Assert.AreEqual("child_string_field", childField.Name);
                    Assert.AreEqual(6, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);

                    childField = field.ChildFields.Last();
                    Assert.AreEqual("child_struct_field", childField.Name);
                    Assert.AreEqual(7, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.STRUCT, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);
                    Assert.IsTrue(childField.HasChildren);
                    Assert.AreEqual(2, childField.ChildFields.Count);

                    BaseFieldDto grandChildField = childField.ChildFields.First();
                    Assert.AreEqual("grandchild_timestamp_field", grandChildField.Name);
                    Assert.AreEqual(8, grandChildField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.TIMESTAMP, grandChildField.FieldType);
                    Assert.IsFalse(grandChildField.IsArray);

                    grandChildField = childField.ChildFields.Last();
                    Assert.AreEqual("grandchild_float_field", grandChildField.Name);
                    Assert.AreEqual(9, grandChildField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, grandChildField.FieldType);
                    Assert.IsFalse(grandChildField.IsArray);

                    field = fields[5];
                    Assert.AreEqual("key_value_struct", field.Name);
                    Assert.AreEqual(10, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.STRUCT, field.FieldType);
                    Assert.IsTrue(field.IsArray);
                    Assert.IsTrue(field.HasChildren);
                    Assert.AreEqual(2, field.ChildFields.Count);

                    childField = field.ChildFields.First();
                    Assert.AreEqual("key", childField.Name);
                    Assert.AreEqual(11, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);

                    childField = field.ChildFields.Last();
                    Assert.AreEqual("value", childField.Name);
                    Assert.AreEqual(12, childField.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, childField.FieldType);
                    Assert.IsFalse(childField.IsArray);

                    field = fields[6];
                    Assert.AreEqual("string_field_2", field.Name);
                    Assert.AreEqual(13, field.OrdinalPosition);
                    Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, field.FieldType);
                    Assert.IsFalse(field.IsArray);
                }).Returns(1);

            GoogleBigQueryService service = new GoogleBigQueryService(schemaService.Object);

            service.UpdateSchemaFields(1, (JArray)rawFields.SelectToken("fields"));

            schemaService.Verify();
        }

        [TestMethod]
        public void UpdateSchemaFields_NoUpdate()
        {
            JObject rawFields = GetData("GoogleBigQuery_BasicSchema.json");

            Mock<ISchemaService> schemaService = new Mock<ISchemaService>(MockBehavior.Strict);
            schemaService.Setup(x => x.GetLatestSchemaRevisionDtoBySchema(1)).Returns(new SchemaRevisionDto() { RevisionId = 2 });

            DecimalFieldDto decimalField = GetField<DecimalFieldDto>("decimal_field", false, 4);
            decimalField.Precision = 5;
            decimalField.Scale = 2;

            StructFieldDto childStructField = GetField<StructFieldDto>("child_struct_field", false, 7);
            childStructField.HasChildren = true;
            childStructField.ChildFields = new List<BaseFieldDto>()
            {
                GetField<TimestampFieldDto>("grandchild_timestamp_field", false, 8),
                GetField<VarcharFieldDto>("grandchild_float_field", false, 9)
            };

            StructFieldDto structField = GetField<StructFieldDto>("struct_field", true, 5);
            structField.HasChildren = true;
            structField.ChildFields = new List<BaseFieldDto>
            {
                GetField<VarcharFieldDto>("child_string_field", false, 6),
                childStructField
            };

            StructFieldDto keyValueField = GetField<StructFieldDto>("key_value_struct", true, 10);
            keyValueField.HasChildren = true;
            keyValueField.ChildFields = new List<BaseFieldDto>
            {
                GetField<VarcharFieldDto>("key", false, 11),
                GetField<VarcharFieldDto>("value", false, 12)
            };

            schemaService.Setup(x => x.GetBaseFieldDtoBySchemaRevision(2)).Returns(new List<BaseFieldDto>()
            {
                GetField<VarcharFieldDto>("string_field", false, 1),
                GetField<BigIntFieldDto>("integer_field", false, 2),
                GetField<DateFieldDto>("date_field", false, 3),
                decimalField,
                structField,
                keyValueField,
                GetField<VarcharFieldDto>("string_field_2", false, 13)
            });

            GoogleBigQueryService service = new GoogleBigQueryService(schemaService.Object);

            service.UpdateSchemaFields(1, (JArray)rawFields.SelectToken("fields"));

            schemaService.Verify();
        }

        [TestMethod]
        public void UpdateSchemaFields_CreateNewGoogleBigQueryRevision()
        {
            JObject rawFields = GetData("GoogleBigQuery_EventsSchema.json");

            Mock<ISchemaService> schemaService = new Mock<ISchemaService>(MockBehavior.Strict);
            schemaService.Setup(x => x.GetLatestSchemaRevisionDtoBySchema(1)).Returns<SchemaRevisionDto>(null);
            schemaService.Setup(x => x.CreateAndSaveSchemaRevision(1, It.IsAny<List<BaseFieldDto>>(), $"GoogleBigQuery_{DateTime.Today:yyyyMMdd}", null))
                .Callback<int, List<BaseFieldDto>, string, string>((id, fields, name, json) => Assert.AreEqual(23, fields.Count)).Returns(1);

            GoogleBigQueryService service = new GoogleBigQueryService(schemaService.Object);

            service.UpdateSchemaFields(1, (JArray)rawFields.SelectToken("schema.fields"));

            schemaService.Verify();
        }

        #region Helpers
        private T GetField<T>(string name, bool isArray, int position) where T : BaseFieldDto
        {
            T fieldDto = Activator.CreateInstance<T>();
            fieldDto.Name = name;
            fieldDto.IsArray = isArray;
            fieldDto.OrdinalPosition = position;

            return fieldDto;
        }
        #endregion
    }
}
