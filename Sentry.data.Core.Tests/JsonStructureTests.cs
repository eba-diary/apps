using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class JsonStructureTests : BaseCoreUnitTest
    {
        [TestMethod]
        public void ToJsonStructure_VarcharField_BasicStringSchema()
        {
            VarcharField field = new VarcharField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1,
                FieldLength = 1000
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_String.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_DateField_BasicDateSchema()
        {
            DateField field = new DateField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1,
                SourceFormat = "yyyy-MM-dd"
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_Date.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_TimestampField_BasicDateTimeSchema()
        {
            TimestampField field = new TimestampField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1,
                SourceFormat = "yyyy-MM-dd HH:mm:ss"
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_DateTime.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_IntegerField_BasicIntegerSchema()
        {
            IntegerField field = new IntegerField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_Integer.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_BigIntField_BasicBigIntegerSchema()
        {
            BigIntField field = new BigIntField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_BigInteger.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_DecimalField_BasicNumberSchema()
        {
            DecimalField field = new DecimalField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1,
                Precision = 9,
                Scale = 2
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_Number.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_StructField_BasicObjectSchema()
        {
            StructField field = new StructField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1
            };

            VarcharField childField = new VarcharField()
            {
                Name = "childfieldname",
                Description = "Child Field Description",
                OrdinalPosition = 2,
                FieldLength = 10,
                ParentField = field
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_Object.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_VarcharFieldArray_BasicStringArraySchema()
        {
            VarcharField field = new VarcharField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1,
                FieldLength = 1000,
                IsArray = true
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_StringArray.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_StructFieldArray_BasicObjectArraySchema()
        {
            StructField field = new StructField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1,
                IsArray = true
            };

            VarcharField childField = new VarcharField()
            {
                Name = "childfieldname",
                Description = "Child Field Description",
                OrdinalPosition = 2,
                FieldLength = 10,
                ParentField = field
            };

            IntegerField childField2 = new IntegerField()
            {
                Name = "childfieldname2",
                Description = "Child Field 2 Description",
                OrdinalPosition = 3,
                ParentField = field
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("BasicSchema_ObjectArray.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_NestedStructField_NestedObjectSchema()
        {
            StructField field = new StructField()
            {
                Name = "fieldname",
                Description = "Field Description",
                OrdinalPosition = 1
            };

            VarcharField childField = new VarcharField()
            {
                Name = "childfieldname",
                Description = "Child Field Description",
                OrdinalPosition = 2,
                FieldLength = 10,
                ParentField = field
            };

            StructField childField2 = new StructField()
            {
                Name = "structchildfieldname",
                Description = "Struct Child Field Description",
                OrdinalPosition = 3,
                ParentField = field
            }; 
            
            DateField innerChildField = new DateField()
            {
                Name = "innerchildfieldname",
                Description = "Inner Child Field Description",
                OrdinalPosition = 4,
                SourceFormat = "yyyy-MM-dd",
                ParentField = childField2
            };

            IntegerField innerChildField2 = new IntegerField()
            {
                Name = "innerchildfieldname2",
                Description = "Inner Child Field 2 Description",
                OrdinalPosition = 5,
                ParentField = childField2
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { field }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("Schema_NestedObject.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ToJsonStructure_ComplexFields_ComplexSchema()
        {
            VarcharField baseName = new VarcharField()
            {
                Name = "baseName",
                Description = "baseName Description",
                OrdinalPosition = 1,
                FieldLength = 1000
            };

            DateField baseDate = new DateField()
            {
                Name = "baseDate",
                Description = "baseDate Description",
                OrdinalPosition = 2,
                SourceFormat = "yyyy-MM-dd"
            };

            StructField baseObjectArray = new StructField()
            {
                Name = "baseObjectArray",
                Description = "baseObjectArray Description",
                OrdinalPosition = 3,
                IsArray = true
            };

            VarcharField objectArrayName = new VarcharField()
            {
                Name = "objectArrayName",
                Description = "objectArrayName Description",
                OrdinalPosition = 1,
                FieldLength = 1000,
                ParentField = baseObjectArray
            };

            IntegerField objectArrayNumberArray = new IntegerField()
            {
                Name = "objectArrayNumberArray",
                Description = "objectArrayNumberArray Description",
                OrdinalPosition = 2,
                IsArray = true,
                ParentField = baseObjectArray
            };

            StructField objectArrayObject = new StructField()
            {
                Name = "objectArrayObject",
                Description = "objectArrayObject Description",
                OrdinalPosition = 3,
                ParentField = baseObjectArray
            };

            VarcharField objectArrayObjectName = new VarcharField()
            {
                Name = "objectArrayObjectName",
                Description = "objectArrayObjectName Description",
                OrdinalPosition = 1,
                FieldLength = 1000,
                ParentField = objectArrayObject
            };

            TimestampField objectArrayObjectDateTime = new TimestampField()
            {
                Name = "objectArrayObjectDateTime",
                Description = "objectArrayObjectDateTime Description",
                OrdinalPosition = 2,
                SourceFormat = "yyyy-MM-dd HH:mm:ss",
                ParentField = objectArrayObject
            };

            StructField objectArrayObjectArray = new StructField()
            {
                Name = "objectArrayObjectArray",
                Description = "objectArrayObjectArray Description",
                OrdinalPosition = 3,
                IsArray = true,
                ParentField = objectArrayObject
            };

            VarcharField objectArrayObjectArrayName = new VarcharField()
            {
                Name = "objectArrayObjectArrayName",
                Description = "objectArrayObjectArrayName Description",
                OrdinalPosition = 1,
                FieldLength = 1000,
                ParentField = objectArrayObjectArray
            };

            DecimalField objectArrayObjectArrayNumber = new DecimalField()
            {
                Name = "objectArrayObjectArrayNumber",
                Description = "objectArrayObjectArrayNumber Description",
                OrdinalPosition = 2,
                Precision = 11,
                Scale = 2,
                ParentField = objectArrayObjectArray
            };

            StructField baseObject = new StructField()
            {
                Name = "baseObject",
                Description = "baseObject Description",
                OrdinalPosition = 4
            };

            IntegerField objectNumber = new IntegerField()
            {
                Name = "objectNumber",
                Description = "objectNumber Description",
                OrdinalPosition = 1,
                ParentField = baseObject
            };

            DateField objectDateArray = new DateField()
            {
                Name = "objectDateArray",
                Description = "objectDateArray Description",
                OrdinalPosition = 2,
                IsArray = true,
                SourceFormat = "yyyy-MM-dd",
                ParentField = baseObject
            };

            StructField objectObject = new StructField()
            {
                Name = "objectObject",
                Description = "objectObject Description",
                OrdinalPosition = 3,
                ParentField = baseObject
            };

            DecimalField objectObjectNumber = new DecimalField()
            {
                Name = "objectObjectNumber",
                Description = "objectObjectNumber Description",
                OrdinalPosition = 1,
                Precision = 5,
                Scale = 2,
                ParentField = objectObject
            };

            StructField objectObjectObject = new StructField()
            {
                Name = "objectObjectObject",
                Description = "objectObjectObject Description",
                OrdinalPosition = 2,
                ParentField = objectObject
            };

            VarcharField objectObjectObjectName = new VarcharField()
            {
                Name = "objectObjectObjectName",
                Description = "objectObjectObjectName Description",
                OrdinalPosition = 1,
                FieldLength = 1000,
                ParentField = objectObjectObject
            };

            TimestampField objectObjectObjectDateTime = new TimestampField()
            {
                Name = "objectObjectObjectDateTime",
                Description = "objectObjectObjectDateTime Description",
                OrdinalPosition = 2,
                SourceFormat = "yyyy-MM-dd hh:mm:ss",
                ParentField = objectObjectObject
            };

            StructField objectObjectObjectObjectArray = new StructField()
            {
                Name = "objectObjectObjectObjectArray",
                Description = "objectObjectObjectObjectArray Description",
                OrdinalPosition = 3,
                IsArray = true,
                ParentField = objectObjectObject
            };

            IntegerField objectObjectObjectObjectArrayNumber = new IntegerField()
            {
                Name = "objectObjectObjectObjectArrayNumber",
                Description = "objectObjectObjectObjectArrayNumber Description",
                OrdinalPosition = 1,
                ParentField = objectObjectObjectObjectArray
            };

            VarcharField objectObjectObjectObjectArrayArray = new VarcharField()
            {
                Name = "objectObjectObjectObjectArrayArray",
                Description = "objectObjectObjectObjectArrayArray Description",
                OrdinalPosition = 2,
                FieldLength = 1000,
                IsArray = true,
                ParentField = objectObjectObjectObjectArray
            };

            SchemaRevision revision = new SchemaRevision()
            {
                SchemaRevision_Name = "StructureTest",
                Fields = new List<BaseField>() { baseDate, baseObject, baseObjectArray, baseName }
            };

            JObject result = revision.ToJsonStructure();

            JObject expected = GetData("Schema_Complex.json");

            Assert.IsTrue(JToken.DeepEquals(expected, result));

            /**This is the JSON format being tested
              {
                "baseName": "Name",
                "baseDate": "2021-12-15",
                "baseObjectArray": [
                    {
                        "objectArrayName": "Name",
                        "objectArrayNumberArray": [
                            1, 2, 3
                        ],
                        "objectArrayObject": {
                            "objectArrayObjectName": "Name",
                            "objectArrayObjectDateTime": "2021-12-15 13:34:00",
                            "objectArrayObjectArray": [
                                {
                                    "objectArrayObjectArrayName": "Name",
                                    "objectArrayObjectArrayNumber": 1.2
                                }
                            ]
                        }
                    }
                ],
                "baseObject": {
                    "objectNumber": 5,
                    "objectDateArray": [
                        "2021-12-13",
                        "2021-12-14"
                    ],
                    "objectObject": {
                        "objectObjectNumber": 500.65,
                        "objectObjectObject": {
                            "objectObjectObjectName": "Name",
                            "objectObjectObjectDateTime": "2021-12-15 01:34:00.000",
                            "objectObjectObjectObjectArray": [
                                {
                                    "objectObjectObjectObjectArrayNumber": 1,
                                    "objectObjectObjectObjectArrayArray": ["Name1", "Name2"]
                                }
                            ]
                        }
                    }
                }
            }*/
        }
    }
}
