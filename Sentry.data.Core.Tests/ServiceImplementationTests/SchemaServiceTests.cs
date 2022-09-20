using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Sentry.Core;
using Sentry.data.Core.DTO.Schema.Fields;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Factories.Fields;
using Sentry.data.Core.GlobalEnums;
using Sentry.FeatureFlags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{

    [TestClass]
    public class SchemaServiceTests : BaseCoreUnitTest
    {
        #region DecimalFieldDto_JsonConstructor Tests
        #region DecimalFieldDto Precision Tests
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Precision_From_DecimalFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(6, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Precision_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(6, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_Null_dscprecision_From_DecimalFieldDto_Json_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""salary"":{
                        ""type"": ""number"",
                        ""dsc-precision"": null,
                        ""dsc-scale"": 2,
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_Null_dscprecision_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //SETUP
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""decimalarray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""number"",
                            ""dsc-precision"": null,
                            ""dsc-scale"": 2
                            },
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscprecision_From_DecimalFieldDto_Json_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""salary"":{
                        ""type"": ""number"",
                        ""dsc-scale"": 2,
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscprecision_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""decimalarray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""number"",
                            ""dsc-scale"": 2
                            },
                        ""description"": ""Decimal field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_PRECISION_DEFAULT, dto.Precision);
        }
        #endregion
        #region DecimalFieldDto Scale Tests
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Scale_From_DecimalFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(2, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Scale_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            int position = 1;
            
            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(2, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_Null_dscscale_From_DecimalFieldDto_Json_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""salary"":{
                        ""type"": ""number"",
                        ""dsc-scale"": null,
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_Null_dscscale_From_DecimalFieldDto_Json_Constructor__Array() 
        {
            //SETUP
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""decimalarray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""number"",
                            ""dsc-precision"": 6,
                            ""dsc-scale"": null
                            },
                        ""description"": ""Decimal field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ACTION
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscscale_From_DecimalFieldDto_Json_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""salary"":{
                        ""type"": ""number"",
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscscale_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""decimalarray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""number"",
                            ""dsc-precision"": 6
                            },
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DECIMAL_SCALE_DEFAULT, dto.Scale);
        }
        #endregion
        #region DecimalFieldDto SourceFormat Tests
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Null_SourceFormat_From_DecimalFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(null, dto.SourceFormat);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Null_SourceFormat_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(null, dto.SourceFormat);
        }
        #endregion
        #region DecimalFieldDto Length Tests
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_0_Length_From_DecimalFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Length);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_0_Length_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Length);
        }
        #endregion
        #region DecimalFieldDto OridinalPosition Tests
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_0_OrdinalPosition_From_DecimalFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(1, dto.OrdinalPosition);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_0_OrdinalPosition_From_DecimalFieldDto_Json_Constructor__Array()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            int position = 1;
            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(1, dto.OrdinalPosition);
        }
        #endregion

        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_False_IsArray_From_BaseFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);
            
            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_True_IsArray_From_BaseFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_FieldType_From_DecimalFieldDto_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""salary"":{
                        ""type"": ""number"",
                        ""dsc-precision"": 6,
                        ""dsc-scale"": 2,
                        ""description"": ""Decimal field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.DECIMAL, dto.FieldType);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_DecimalFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;
            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("salary", dto.Name);
            Assert.AreEqual("Decimal field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }

        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_DecimalFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;
            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("decimalarray", dto.Name);
            Assert.AreEqual("Decimal field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(true, dto.IsArray);
        }
        #endregion

        #region DecimalFieldDto SchemaRow Constructor Tests

        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_False_IsArray_From_BaseFieldDto_SchemaRow_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            //Action
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Assersion
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        #endregion

        #region DecimalFieldDto Field Constructor Tests
        [TestMethod, TestCategory("DecimalFieldDto Field Constructor")]
        public void Get_Precision_from_DecimalFieldDto_DecimalField_Constructor()
        {
            //setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.First(w => w.FieldType == SchemaDatatypes.DECIMAL);

            DecimalFieldDto dto = new DecimalFieldDto(field);

            //assersion
            Assert.IsNotNull(dto);
            Assert.AreEqual(6, dto.Precision);
        }

        [TestMethod, TestCategory("DecimalFieldDto Field Constructor")]
        public void Get_Scale_from_DecimalFieldDto_DecimalField_Constructor()
        {
            //setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.First(w => w.FieldType == SchemaDatatypes.DECIMAL);

            DecimalFieldDto dto = new DecimalFieldDto(field);

            //assersion
            Assert.IsNotNull(dto);
            Assert.AreEqual(2, dto.Scale);
        }

        [TestMethod, TestCategory("DecimalFieldDto Field Constructor")]
        public void Get_Null_SourceFormat_from_DecimalFieldDto_DecimalField_Constructor()
        {
            //setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.First(w => w.FieldType == SchemaDatatypes.DECIMAL);

            DecimalFieldDto dto = new DecimalFieldDto(field);

            //assersion
            Assert.IsNotNull(dto);
            Assert.IsNull(dto.SourceFormat);
        }

        [TestMethod, TestCategory("DecimalFieldDto Field Constructor")]
        public void Get_0_length_from_DecimalFieldDto_DecimalField_Constructor()
        {
            //setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.First(w => w.FieldType == SchemaDatatypes.DECIMAL);

            DecimalFieldDto dto = new DecimalFieldDto(field);

            //assersion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Length);
        }

        [TestMethod, TestCategory("DecimalFieldDto Field Constructor")]
        public void Get_True_Nullable_from_DecimalFieldDto_DecimalField_Constructor()
        {
            //setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.First(w => w.FieldType == SchemaDatatypes.DECIMAL);

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(field);

            //assersion
            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.Nullable);
        }

        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_DecimalField_Constructor()
        {
            //Setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.First(w => w.FieldType == SchemaDatatypes.DECIMAL);

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(field);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("decimalField", dto.Name);
            Assert.AreEqual("Decimal Field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(99, dto.FieldId);
            Assert.AreEqual("c5023db5-7125-4faf-acca-22b3f1e8bc79", dto.FieldGuid.ToString());
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }
        #endregion

        #region DecimalFieldDto ToEntity Tests

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void Get_DecimalField_Entity_from_DecimalFieldDto()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Action
            BaseField entity = dto.ToEntity(null, null);

            //Assersion
            Assert.IsTrue(entity is DecimalField);
        }

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void DecimalFieldDto_ToEntity_Returns_DecimalField()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Action
            BaseField entity = dto.ToEntity(null, null);

            //Assersion
            Assert.IsTrue(entity is DecimalField);
        }

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void DecimalFieldDto_ToEntity_Returns_BaseFieldProperties()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();
            int position = 1;

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Action
            BaseField entity = dto.ToEntity(null, null);

            //Assersion
            Assert.IsNotNull(entity);
            Assert.AreEqual("salary", entity.Name);
            Assert.AreEqual("Decimal field", entity.Description);
            Assert.IsNotNull(entity.ChildFields);
            Assert.AreEqual(0, entity.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, entity.CreateDTM);
            Assert.AreNotEqual(DateTime.MaxValue, entity.CreateDTM);
            Assert.AreNotEqual(DateTime.MinValue, entity.LastUpdateDTM);
            Assert.AreNotEqual(DateTime.MaxValue, entity.LastUpdateDTM);
            Assert.AreEqual(0, entity.FieldId);
            Assert.AreNotEqual(Guid.Empty, entity.FieldGuid);
            Assert.AreEqual(false, entity.IsArray);
            Assert.AreEqual(null, entity.ParentField);
            Assert.AreEqual(null, entity.ParentSchemaRevision);
        }

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void DecimalFieldDto_ToEntity_Returns_ParentSchemaRevision()
        {
            //Setup
            //Build Dto object
            JsonSchema jschema = BuildMockJsonSchemaWithDecimalField();
            KeyValuePair<string, JsonSchemaProperty> prop = jschema.Properties.First();
            int position = 1;
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Build ParentRevision and ParentField objects
            FileSchema fschema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            SchemaRevision parentRevision = fschema.Revisions.First();
            DecimalField parentField = (DecimalField)parentRevision.Fields.FirstOrDefault(w => w.FieldType == SchemaDatatypes.DECIMAL);

            //Action
            BaseField entity = dto.ToEntity(parentField, parentRevision);

            //Assersion
            Assert.IsNotNull(entity);
            Assert.IsNotNull(entity.ParentSchemaRevision);
            Assert.AreEqual(parentRevision, entity.ParentSchemaRevision);
        }

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void DecimalFieldDto_ToEntity_Returns_ParentField()
        {
            //Setup
            //Build Dto object
            JsonSchema jschema = BuildMockJsonSchemaWithDecimalField();
            KeyValuePair<string, JsonSchemaProperty> prop = jschema.Properties.First();
            int position = 1;
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            //Build ParentRevision and ParentField objects
            FileSchema fschema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            SchemaRevision parentRevision = fschema.Revisions.First();
            DecimalField parentField = (DecimalField)parentRevision.Fields.FirstOrDefault(w => w.FieldType == SchemaDatatypes.DECIMAL);

            //Action
            BaseField entity = dto.ToEntity(parentField, parentRevision);

            //Assersion
            Assert.IsNotNull(entity);
            Assert.IsNotNull(entity.ParentField);
            Assert.AreEqual(parentField, entity.ParentField);
        }

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void DecimalFieldDto_ToEntity_Returns_New_FieldGuid()
        {
            //Setup
            //Build Dto object
            JsonSchema jschema = BuildMockJsonSchemaWithDecimalField();
            KeyValuePair<string, JsonSchemaProperty> prop = jschema.Properties.First();
            int position = 1;
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            dto.FieldGuid = Guid.Empty;

            //Build ParentRevision and ParentField objects
            FileSchema fschema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            SchemaRevision parentRevision = fschema.Revisions.First();
            DecimalField parentField = (DecimalField)parentRevision.Fields.FirstOrDefault(w => w.FieldType == SchemaDatatypes.DECIMAL);

            //Action
            BaseField entity = dto.ToEntity(parentField, parentRevision);

            //Assersion
            Assert.IsNotNull(entity);
            Assert.AreNotEqual(Guid.Empty, entity.FieldGuid);
        }

        [TestMethod, TestCategory("DecimalFieldDto ToEntity")]
        public void DecimalFieldDto_ToEntity_Returns_Existing_FieldGuid()
        {
            //Setup
            //Build Dto object
            JsonSchema jschema = BuildMockJsonSchemaWithDecimalField();
            KeyValuePair<string, JsonSchemaProperty> prop = jschema.Properties.First();
            int position = 1;
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, false);

            Guid g = Guid.NewGuid();
            dto.FieldGuid = g;

            //Build ParentRevision and ParentField objects
            FileSchema fschema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            SchemaRevision parentRevision = fschema.Revisions.First();
            DecimalField parentField = (DecimalField)parentRevision.Fields.FirstOrDefault(w => w.FieldType == SchemaDatatypes.DECIMAL);

            //Action
            BaseField entity = dto.ToEntity(parentField, parentRevision);

            //Assersion
            Assert.IsNotNull(entity);
            Assert.AreEqual(g, entity.FieldGuid);
        }
        #endregion

        #region VarcharFieldDto JsonConstructor Tests
        #region __VarcharFieldDto Length Tests
        /*********Start __VarcharFieldDto Length Tests************/
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_Length_From_VarcharFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithVarcharField();
            int position = 1;
            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(99, dto.Length);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_Length_From_VarcharFieldDto_Json_Constructor__Array()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithVarcharFieldArray();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(15, dto.Length);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Can_Default_Null_maxlength_From_VarcharFieldDto_Json_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""firstname"":{
                        ""type"": ""string"",
                        ""maxlength"": null,
                        ""description"": ""Varchar field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT, dto.Length);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Can_Default_Null_maxlength_From_VarcharFieldDto_Json_Constructor_Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""varchararray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""maxlength"": null
                            },
                        ""description"": ""Varchar field array""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT, dto.Length);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Can_Default_NonExistent_maxlength_From_VarcharFieldDto_Json_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""firstname"":{
                        ""type"": ""string"",
                        ""description"": ""Varchar field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

             //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT, dto.Length);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Can_Default_NonExistent_maxlength_From_VarcharFieldDto_Json_Constructor_Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""varchararray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string""
                            },
                        ""description"": ""Varchar field array""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.VARCHAR_LENGTH_DEFAULT, dto.Length);
        }

        /*********End __VarcharFieldDto Length Tests************/
        #endregion

        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_IsArray_False_From_VarcharFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithVarcharField();
            int position = 1;
            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_IsArray_True_From_VarcharFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithVarcharFieldArray();
            int position = 1;
            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, true);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_VarcharFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithVarcharField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("firstname", dto.Name);
            Assert.AreEqual("Varchar field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_VarcharFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithVarcharFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("varchararray", dto.Name);
            Assert.AreEqual("Varchar field array", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_FieldType_From_VarcharFieldDto_Constructor()
        {
            //Setup

            JsonSchema schema = BuildMockJsonSchemaWithVarcharField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, dto.FieldType);
        }
        [TestMethod, TestCategory("VarcharFieldDto JsonContructor")]
        public void Get_FieldType_From_VarcharFieldDto_Constructor__Array()
        {
            //Setup

            JsonSchema schema = BuildMockJsonSchemaWithVarcharFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            VarcharFieldDto dto = new VarcharFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.VARCHAR, dto.FieldType);
        }
        #endregion

        #region DateFieldDto JsonConstructor Tests
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Get_IsArray_False_From_DateFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithDateField();
            int position = 1;


            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Get_IsArray_True_From_DateFieldDto_Json_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateFieldArray();
            int position = 1;

            //Action
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DateFieldDto dto = new DateFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_DateFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("DOB", dto.Name);
            Assert.AreEqual("Date field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_DateFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("datearray", dto.Name);
            Assert.AreEqual("Date field array", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Get_FieldType_From_DateFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.DATE, dto.FieldType);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Get_FieldType_From_DateFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.DATE, dto.FieldType);
        }

        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Can_Default_Null_dscformat_From_DateFieldDto_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""DOB"":{
                        ""type"": ""string"",
                        ""format"": ""date"",
                        ""dsc-format"": null,
                        ""description"": ""Date field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DATE_DEFAULT, dto.SourceFormat);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Can_Default_Null_dscformat_From_DateFieldDto_Constructor__Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""datearray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date"",
                            ""dsc-format"": null,
                            ""description"": ""Date field""
                            },
                        ""description"": ""Date field array""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;


            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DATE_DEFAULT, dto.SourceFormat);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscformat_From_DateFieldDto_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""DOB"":{
                        ""type"": ""string"",
                        ""format"": ""date"",
                        ""description"": ""Date field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DATE_DEFAULT, dto.SourceFormat);
        }
        [TestMethod, TestCategory("DateFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscformat_From_DateFieldDto_Constructor__Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""datearray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date"",
                            ""description"": ""Date field""
                            },
                        ""description"": ""Date field array""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            DateFieldDto dto = new DateFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.DATE_DEFAULT, dto.SourceFormat);
        }
        #endregion

        #region TimestampFieldDto JsonConstructor Tests
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Get_IsArray_False_From_TimestampFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithTimestampField();
            int position = 1;

            //ACTION
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Get_IsArray_True_From_TimestampFieldDto_Json_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithTimestampFieldArray();
            int position = 1;

            //Action
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_TimestampFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithTimestampField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("createdtm", dto.Name);
            Assert.AreEqual("Timestamp field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_TimestampFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithTimestampFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("timestamparray", dto.Name);
            Assert.AreEqual("Timestamp field array", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Get_FieldType_From_TimestampFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.TIMESTAMP, dto.FieldType);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Get_FieldType_From_TimestampFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDateFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.TIMESTAMP, dto.FieldType);
        }

        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Can_Default_Null_dscformat_From_TimestampFieldDto_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""createdtm"":{
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""dsc-format"": null,
                        ""description"": ""Timestamp field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT, dto.SourceFormat);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Can_Default_Null_dscformat_From_TimestampFieldDto_Constructor__Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""timestamparray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date-time"",
                            ""dsc-format"": null,
                            ""description"": ""Date field""
                            },
                        ""description"": ""Timestamp field array""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT, dto.SourceFormat);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscformat_From_TimestampFieldDto_Constructor()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""createdtm"":{
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""description"": ""Timestamp field""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT, dto.SourceFormat);
        }
        [TestMethod, TestCategory("TimestampFieldDto JsonContructor")]
        public void Can_Default_NonExistent_dscformat_From_TimestampFieldDto_Constructor__Array()
        {
            //Setup
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""timestamparray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date-time"",
                            ""description"": ""Date field""
                            },
                        ""description"": ""Timestamp field array""
                    }
                }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            TimestampFieldDto dto = new TimestampFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT, dto.SourceFormat);
        }
        #endregion

        #region IntegerFieldDto JsonConstrcutor Tests

        [TestMethod, TestCategory("IntegerFieldDto JsonContructor")]
        public void Get_IsArray_False_From_IntegerFieldDto_Json_Constructor()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchemaWithIntegerField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //ACTION
            IntegerFieldDto dto = new IntegerFieldDto(prop, position, false);

            //ASSERT
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("IntegerFieldDto JsonContructor")]
        public void Get_IsArray_True_From_IntegerFieldDto_Json_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithIntegerFieldArray();
            int position = 1;
            //Action
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            IntegerFieldDto dto = new IntegerFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("IntegerFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_IntegerFieldDto_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithIntegerField();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            IntegerFieldDto dto = new IntegerFieldDto(prop, position, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("age", dto.Name);
            Assert.AreEqual("Integer field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("IntegerFieldDto JsonContructor")]
        public void Get_BaseFieldDto_Properties_From_IntegerFieldDto_Constructor__Array()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithIntegerFieldArray();
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            int position = 1;

            //Action
            IntegerFieldDto dto = new IntegerFieldDto(prop, position, true);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("timestamparray", dto.Name);
            Assert.AreEqual("Timestamp field array", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDtm);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDtm);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDtm);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(true, dto.IsArray);
        }
        #endregion

        #region FieldFactory Tests
        [TestMethod]
        public void FieldFactory_GetField_Returns_DeleteInd_True()
        {
            //Setup
            SchemaRow row = new SchemaRow()
            {
                ArrayType = null,
                DeleteInd = true,
                FieldGuid = Guid.NewGuid(),
                Name = "VARCHAR Field"
            };
            FieldDtoFactory dateFieldFactory;
            dateFieldFactory = new DateFieldDtoFactory(row);

            FieldDtoFactory decimalFieldFactory;
            decimalFieldFactory = new DecimalFieldDtoFactory(row);

            FieldDtoFactory integerFieldFactory;
            integerFieldFactory = new IntegerFieldDtoFactory(row);

            FieldDtoFactory structFieldFactory;
            structFieldFactory = new StructFieldDtoFactory(row);

            FieldDtoFactory timestampFieldFactory;
            timestampFieldFactory = new TimestampFieldDtoFactory(row);

            FieldDtoFactory varcharFieldFactory;
            varcharFieldFactory = new VarcharFieldDtoFactory(row);

            //Action
            BaseFieldDto dateFieldDto = dateFieldFactory.GetField();
            BaseFieldDto decimalFieldDto = decimalFieldFactory.GetField();
            BaseFieldDto integerFieldDto = integerFieldFactory.GetField();
            BaseFieldDto structFieldDto = structFieldFactory.GetField();
            BaseFieldDto timestampFieldDto = timestampFieldFactory.GetField();
            BaseFieldDto varcharFieldDto = varcharFieldFactory.GetField();

            //Assert
            Assert.IsTrue(dateFieldDto.DeleteInd, "DateFieldDtoFactory failed DeleteInd Check");
            Assert.IsTrue(decimalFieldDto.DeleteInd, "DecimalFieldDtoFactory failed DeleteInd Check");
            Assert.IsTrue(integerFieldDto.DeleteInd, "IntegerFieldDtoFactory failed DeleteInd Check");
            Assert.IsTrue(structFieldDto.DeleteInd, "StructFieldDtoFactory failed DeleteInd Check");
            Assert.IsTrue(timestampFieldDto.DeleteInd, "TimestampFieldDtoFactory failed DeleteInd Check");
            Assert.IsTrue(varcharFieldDto.DeleteInd, "VarcharFieldDtoFactory failed DeleteInd Check");
        }
        [TestMethod]
        public void FieldFactory_GetField_Returns_DeleteInd_False()
        {
            //Setup
            SchemaRow row = new SchemaRow()
            {
                ArrayType = null,
                DeleteInd = false,
                FieldGuid = Guid.NewGuid(),
                Name = "VARCHAR Field"
            };
            FieldDtoFactory dateFieldFactory;
            dateFieldFactory = new DateFieldDtoFactory(row);

            FieldDtoFactory decimalFieldFactory;
            decimalFieldFactory = new DecimalFieldDtoFactory(row);

            FieldDtoFactory integerFieldFactory;
            integerFieldFactory = new IntegerFieldDtoFactory(row);

            FieldDtoFactory structFieldFactory;
            structFieldFactory = new StructFieldDtoFactory(row);

            FieldDtoFactory timestampFieldFactory;
            timestampFieldFactory = new TimestampFieldDtoFactory(row);

            FieldDtoFactory varcharFieldFactory;
            varcharFieldFactory = new VarcharFieldDtoFactory(row);

            //Action
            BaseFieldDto dateFieldDto = dateFieldFactory.GetField();
            BaseFieldDto decimalFieldDto = decimalFieldFactory.GetField();
            BaseFieldDto integerFieldDto = integerFieldFactory.GetField();
            BaseFieldDto structFieldDto = structFieldFactory.GetField();
            BaseFieldDto timestampFieldDto = timestampFieldFactory.GetField();
            BaseFieldDto varcharFieldDto = varcharFieldFactory.GetField();

            //Assert
            Assert.IsFalse(dateFieldDto.DeleteInd, "DateFieldDtoFactory failed DeleteInd Check");
            Assert.IsFalse(decimalFieldDto.DeleteInd, "DecimalFieldDtoFactory failed DeleteInd Check");
            Assert.IsFalse(integerFieldDto.DeleteInd, "IntegerFieldDtoFactory failed DeleteInd Check");
            Assert.IsFalse(structFieldDto.DeleteInd, "StructFieldDtoFactory failed DeleteInd Check");
            Assert.IsFalse(timestampFieldDto.DeleteInd, "TimestampFieldDtoFactory failed DeleteInd Check");
            Assert.IsFalse(varcharFieldDto.DeleteInd, "VarcharFieldDtoFactory failed DeleteInd Check");
        }
        #endregion

        #region GenerateParquetStorageBucket Tests
        [TestMethod]
        public void SchemaService_GenerateParquetStorageBucket_HRBucket()
        {
            // Arrange
            var schemaService = new SchemaService(null, null, null, null, null, null, null, null, null,null, null, null);

            // Act
            var result = schemaService.GenerateParquetStorageBucket(true, "DATA", "DEV");

            // Assert
            Assert.AreEqual("sentry-data-dev-hrdataset-ae2", result, false);
        }

        [TestMethod]
        public void SchemaService_GenerateParquetStorageBucket_BaseBucket()
        {
            // Arrange
            var schemaService = new SchemaService(null, null, null, null, null, null, null, null, null, null, null, null);

            // Act
            var result = schemaService.GenerateParquetStorageBucket(false, "DATA", "DEV");

            // Assert
            Assert.AreEqual("sentry-data-dev-dataset-ae2", result, false);
        }

        #endregion

        #region GenerateParquetStoragePrefix Tests
        [TestMethod]
        public void SchemaService_GenerateParquetStoragePrefix_ConsolidatedDataFlow_True()
        {
            // Arrange
            var dataFeatures = new MockDataFeatures();

            var schemaService = new SchemaService(null, null, null, null, null, null, dataFeatures, null, null, null, null, null);


            // Act
            var result = schemaService.GenerateParquetStoragePrefix("DATA", "DEV", "123456");

            // Assert
            Assert.AreEqual($"{GlobalConstants.ConvertedFileStoragePrefix.PARQUET_STORAGE_PREFIX}/DATA/DEV/123456", result, false);
        }

        [TestMethod]
        public void SchemaService_GenerateParquetStoragePrefix_Null_SAID_Keycode()
        {
            // Arrange
            var dataFeatures = new MockDataFeatures();

            var schemaService = new SchemaService(null, null, null, null, null, null, dataFeatures, null, null, null, null, null);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(() => schemaService.GenerateParquetStoragePrefix(null, "DEV", "123456"));
        }

        [TestMethod]
        public void SchemaService_GenerateParquetStoragePrefix_Null_Storagecode()
        {
            // Arrange
            var schemaService = new SchemaService(null, null, null, null, null, null, null, null, null, null, null, null);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(() => schemaService.GenerateParquetStoragePrefix("DATA", "DEV", null));
        }

        [TestMethod]
        public void SchemaService_GenerateParquetStoragePrefix_Null_NamedEnviornment()
        {
            // Arrange
            var dataFeatures = new MockDataFeatures();

            var schemaService = new SchemaService(null, null, null, null, null, null, dataFeatures, null, null, null, null, null);

            // Assert
            Assert.ThrowsException<ArgumentNullException>(() => schemaService.GenerateParquetStoragePrefix("DATA", null, "123456"));
        }

        #endregion

        #region Generate Snowflake Metadata Tests

        [TestMethod]
        public void SchemaService_GenerateConsumptionLayers_AuthOff()
        {
            //Arrange
            Mock<IDataFeatures> dataFeatures = new Mock<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");
            dataFeatures.Setup(s => s.CLA3718_Authorization.GetValue()).Returns(false);
            dataFeatures.Setup(s => s.CLA4410_StopCategoryBasedConsumptionLayerCreation.GetValue()).Returns(false);


            Dataset dataset = MockClasses.MockDataset();
            dataset.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset.NamedEnvironment = "QUAL";

            FileSchemaDto fileSchemaDto = new FileSchemaDto() { Name = "Schema YYYY" };
            FileSchema schema = new FileSchema() { SchemaId = 1 };


            Mock<SchemaService> schemaService = new Mock<SchemaService>(null, null, null, null, null, null, dataFeatures.Object, null, null, null, null) { CallBase = true };
            schemaService.Setup(s => s.GetSnowflakeDatabaseName(It.IsAny<bool>())).Returns("DB_Name");
            schemaService.Setup(s => s.GetSnowflakeSchemaName(It.IsAny<Dataset>(), It.IsAny<SnowflakeConsumptionType>())).Returns("YYYY");
            schemaService.Setup(s => s.GetSnowflakeDatabaseName(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<SnowflakeConsumptionType>())).Returns("DB_Name");


            var dbName_TESTNP = schemaService.Object.GenerateConsumptionLayers(fileSchemaDto, schema, dataset);
            var snowflakeConsumptionLayers = dbName_TESTNP.OfType<SchemaConsumptionSnowflake>().ToList();

            //Assert
            Assert.AreEqual(1, snowflakeConsumptionLayers.Count(w => w.SnowflakeType == SnowflakeConsumptionType.CategorySchemaParquet));
            Assert.AreEqual(1, snowflakeConsumptionLayers.Count(w => w.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaRaw));
            Assert.AreEqual(1, snowflakeConsumptionLayers.Count(w => w.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaRawQuery));
            Assert.AreEqual(1, snowflakeConsumptionLayers.Count(w => w.SnowflakeType == SnowflakeConsumptionType.DatasetSchemaParquet));
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SchemaService_GenerateConsumptionLayers_StopCategoryBasedGeneration(bool stopCategoryGeneration)
        {
            //Arrange
            Mock<IDataFeatures> dataFeatures = new Mock<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");
            dataFeatures.Setup(s => s.CLA3718_Authorization.GetValue()).Returns(true);
            dataFeatures.Setup(s => s.CLA4410_StopCategoryBasedConsumptionLayerCreation.GetValue()).Returns(stopCategoryGeneration);
            dataFeatures.Setup(s => s.CLA440_CategoryConsumptionLayerCreateLineInSand.GetValue()).Returns("2022-08-15");

            Dataset dataset = MockClasses.MockDataset();
            dataset.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset.NamedEnvironment = "QUAL";
            dataset.DatasetDtm = DateTime.Parse("2022-07-15");

            FileSchemaDto fileSchemaDto = new FileSchemaDto() { Name = "Schema YYYY" };
            FileSchema schema = new FileSchema() { SchemaId = 1 };

            Mock<SchemaService> schemaService = new Mock<SchemaService>(null, null, null, null, null, null, dataFeatures.Object, null, null, null, null) { CallBase = true };
            schemaService.Setup(s => s.GetSnowflakeDatabaseName(It.IsAny<bool>())).Returns("DB_Name");
            schemaService.Setup(s => s.GetSnowflakeSchemaName(It.IsAny<Dataset>(), It.IsAny<SnowflakeConsumptionType>())).Returns("YYYY");
            schemaService.Setup(s => s.GetSnowflakeDatabaseName(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<SnowflakeConsumptionType>())).Returns("DB_Name");

            var dbName_TESTNP = schemaService.Object.GenerateConsumptionLayers(fileSchemaDto, schema, dataset);
            var snowflakeConsumptionLayers = dbName_TESTNP.OfType<SchemaConsumptionSnowflake>().ToList();

            //Assert
            Assert.AreEqual(!stopCategoryGeneration, snowflakeConsumptionLayers.Any(w => w.SnowflakeType == SnowflakeConsumptionType.CategorySchemaParquet));
            Assert.AreEqual(3, snowflakeConsumptionLayers.Count(w => w.SnowflakeType != SnowflakeConsumptionType.CategorySchemaParquet));
        }

        [TestMethod]
        [DataRow("2022-08-20")]
        [DataRow("2022-07-20")]
        public void SchemaService_GenerateConsumptionLayers_StopCategoryBasedDatasetDate(string datasetCreateDTM)
        {
            //Arrange
            Mock<IDataFeatures> dataFeatures = new Mock<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("");
            dataFeatures.Setup(s => s.CLA3718_Authorization.GetValue()).Returns(true);
            dataFeatures.Setup(s => s.CLA4410_StopCategoryBasedConsumptionLayerCreation.GetValue()).Returns(false);
            dataFeatures.Setup(s => s.CLA440_CategoryConsumptionLayerCreateLineInSand.GetValue()).Returns(datasetCreateDTM);

            Dataset dataset = MockClasses.MockDataset();
            dataset.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset.NamedEnvironment = "QUAL";
            dataset.DatasetDtm = DateTime.Parse("2022-08-01");

            FileSchemaDto fileSchemaDto = new FileSchemaDto() { Name = "Schema YYYY" };
            FileSchema schema = new FileSchema() { SchemaId = 1 };

            Mock<SchemaService> schemaService = new Mock<SchemaService>(null, null, null, null, null, null, dataFeatures.Object, null, null, null, null) { CallBase = true };
            schemaService.Setup(s => s.GetSnowflakeDatabaseName(It.IsAny<bool>())).Returns("DB_Name");
            schemaService.Setup(s => s.GetSnowflakeSchemaName(It.IsAny<Dataset>(), It.IsAny<SnowflakeConsumptionType>())).Returns("YYYY");
            schemaService.Setup(s => s.GetSnowflakeDatabaseName(It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<SnowflakeConsumptionType>())).Returns("DB_Name");

            var dbName_TESTNP = schemaService.Object.GenerateConsumptionLayers(fileSchemaDto, schema, dataset);
            var snowflakeConsumptionLayers = dbName_TESTNP.OfType<SchemaConsumptionSnowflake>().ToList();

            //Assert
            Assert.AreEqual(datasetCreateDTM == "2022-08-20", snowflakeConsumptionLayers.Any(w => w.SnowflakeType == SnowflakeConsumptionType.CategorySchemaParquet));
            Assert.AreEqual(3, snowflakeConsumptionLayers.Count(w => w.SnowflakeType != SnowflakeConsumptionType.CategorySchemaParquet));
        }


        [TestMethod]
        public void SchemaService_GenerateSnowflakeDatabaseName_Include_Prefix()
        {
            //Arrange
            Mock<IDataFeatures> dataFeatures = new Mock<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns("All");

            var schemaService = new SchemaService(null, null, null, null, null, null, dataFeatures.Object, null, null, null, null);

            //Act
            string dbName_TESTNP = schemaService.GenerateSnowflakeDatabaseName(false, "TEST", NamedEnvironmentType.NonProd.ToString(), "RAWQUERY_");

            //Assert
            Assert.AreEqual("DATA_RAWQUERY_TEST", dbName_TESTNP);
        }

        [TestMethod]
        [DataRow(" ")]
        [DataRow("ABC")]
        public void SchemaService_GenerateSnowflakeDatabaseName(string allowableEnvironments)
        {
            // Arrange
            Mock<IDataFeatures> dataFeatures = new Mock<IDataFeatures>();
            dataFeatures.Setup(s => s.CLA4260_QuartermasterNamedEnvironmentTypeFilter.GetValue()).Returns(allowableEnvironments);

            var schemaService = new SchemaService(null, null, null, null, null, null, dataFeatures.Object, null, null, null, null);

            // Act
            string dbName_TESTNP = schemaService.GenerateSnowflakeDatabaseName(false, "TEST", NamedEnvironmentType.NonProd.ToString(), null);
            string dbName_TESTPROD = schemaService.GenerateSnowflakeDatabaseName(false, "TEST", NamedEnvironmentType.Prod.ToString(), null);
            string dbName_QUALNP = schemaService.GenerateSnowflakeDatabaseName(false, "QUAL", NamedEnvironmentType.NonProd.ToString(), null);
            string dbName_QUALPROD = schemaService.GenerateSnowflakeDatabaseName(false, "QUAL", NamedEnvironmentType.Prod.ToString(), null);
            string dbName_PRODNP = schemaService.GenerateSnowflakeDatabaseName(false, "PROD", NamedEnvironmentType.NonProd.ToString(), null);
            string dbName_PRODPROD = schemaService.GenerateSnowflakeDatabaseName(false, "PROD", NamedEnvironmentType.Prod.ToString(), null);

            string dbName_TESTNP_HR = schemaService.GenerateSnowflakeDatabaseName(true, "TEST", NamedEnvironmentType.NonProd.ToString(), null);
            string dbName_TESTPROD_HR = schemaService.GenerateSnowflakeDatabaseName(true, "TEST", NamedEnvironmentType.Prod.ToString(), null);
            string dbName_QUALNP_HR = schemaService.GenerateSnowflakeDatabaseName(true, "QUAL", NamedEnvironmentType.NonProd.ToString(), null);
            string dbName_QUALPROD_HR = schemaService.GenerateSnowflakeDatabaseName(true, "QUAL", NamedEnvironmentType.Prod.ToString(), null);
            string dbName_PRODNP_HR = schemaService.GenerateSnowflakeDatabaseName(true, "PROD", NamedEnvironmentType.NonProd.ToString(), null);
            string dbName_PRODPROD_HR = schemaService.GenerateSnowflakeDatabaseName(true, "PROD", NamedEnvironmentType.Prod.ToString(), null);

            // Assert
            if (allowableEnvironments == " ")
            {
                Assert.AreEqual("DATA_TEST", dbName_TESTNP);
                Assert.AreEqual("DATA_TEST", dbName_TESTPROD);
                Assert.AreEqual("WDAY_TEST", dbName_TESTNP_HR);
                Assert.AreEqual("WDAY_TEST", dbName_TESTPROD_HR);

                Assert.AreEqual("DATA_QUALNP", dbName_QUALNP);
                Assert.AreEqual("DATA_QUAL", dbName_QUALPROD);
                Assert.AreEqual("DATA_NONPROD", dbName_PRODNP);
                Assert.AreEqual("DATA_PROD", dbName_PRODPROD);

                Assert.AreEqual("WDAY_QUALNP", dbName_QUALNP_HR);
                Assert.AreEqual("WDAY_QUAL", dbName_QUALPROD_HR);
                Assert.AreEqual("WDAY_NONPROD", dbName_PRODNP_HR);
                Assert.AreEqual("WDAY_PROD", dbName_PRODPROD_HR);
            }
            else
            {

                Assert.AreEqual("DATA_TEST", dbName_TESTNP);
                Assert.AreEqual("DATA_TEST", dbName_TESTPROD);
                Assert.AreEqual("WDAY_TEST", dbName_TESTNP_HR);
                Assert.AreEqual("WDAY_TEST", dbName_TESTPROD_HR);

                Assert.AreEqual("DATA_QUAL", dbName_QUALNP);
                Assert.AreEqual("DATA_QUAL", dbName_QUALPROD);
                Assert.AreEqual("DATA_PROD", dbName_PRODNP);
                Assert.AreEqual("DATA_PROD", dbName_PRODPROD);

                Assert.AreEqual("WDAY_QUAL", dbName_QUALNP_HR);
                Assert.AreEqual("WDAY_QUAL", dbName_QUALPROD_HR);
                Assert.AreEqual("WDAY_PROD", dbName_PRODNP_HR);
                Assert.AreEqual("WDAY_PROD", dbName_PRODPROD_HR);
            }

        }

        [TestMethod]
        public void SchemaService_GenerateSnowflakeSchemaName()
        {
            //Arrange
            Dataset dataset = MockClasses.MockDataset();
            dataset.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset.NamedEnvironment = "QUAL";
            dataset.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            Dataset datasetHR = MockClasses.MockDataset();
            datasetHR.DatasetCategories = new List<Category>() { new Category() { Name = "Human Resources", AbbreviatedName = "HR" } };
            datasetHR.NamedEnvironment = "QUAL";
            datasetHR.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            var schemaService = new SchemaService(null, null, null, null, null, null, null, null, null, null, null);

            //Act
            var schemaName = schemaService.GenerateCategoryBasedSnowflakeSchemaName(dataset);
            var schemaName_HR = schemaService.GenerateCategoryBasedSnowflakeSchemaName(datasetHR);

            //Assert
            Assert.AreEqual("CLAIM", schemaName);
            Assert.AreEqual("HR", schemaName_HR);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SchemaService_GenerateDatasetBasedSnowflakeSchemaName(bool alwaysSuffixSchemaNames)
        {
            //Arrange
            Dataset dataset_Prod = MockClasses.MockDataset();
            dataset_Prod.DatasetName = "DS With Long Name";
            dataset_Prod.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset_Prod.NamedEnvironment = "PROD";
            dataset_Prod.NamedEnvironmentType = NamedEnvironmentType.Prod;

            Dataset dataset_Prod2 = MockClasses.MockDataset();
            dataset_Prod2.DatasetName = "DS With Long Name";
            dataset_Prod2.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset_Prod2.NamedEnvironment = "PROD2";
            dataset_Prod2.NamedEnvironmentType = NamedEnvironmentType.Prod;

            Dataset dataset_Qual = MockClasses.MockDataset();
            dataset_Qual.DatasetName = "DS With Long Name";
            dataset_Qual.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset_Qual.NamedEnvironment = "QUAL";
            dataset_Qual.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            Dataset dataset_Qual2 = MockClasses.MockDataset();
            dataset_Qual2.DatasetName = "DS With Long Name";
            dataset_Qual2.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset_Qual2.NamedEnvironment = "QUAL2";
            dataset_Qual2.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            Dataset dataset_Test = MockClasses.MockDataset();
            dataset_Test.DatasetName = "DS With Long Name";
            dataset_Test.DatasetCategories = new List<Category>() { new Category() { Name = "CLAIM" } };
            dataset_Test.NamedEnvironment = "TEST";
            dataset_Test.NamedEnvironmentType = NamedEnvironmentType.NonProd;

            var schemaService = new SchemaService(null, null, null, null, null, null, null, null, null, null, null);

            //Act
            string schemaName_Prod = schemaService.GenerateDatasetBasedSnowflakeSchemaName(dataset_Prod, alwaysSuffixSchemaNames);
            string schemaName_Prod2 = schemaService.GenerateDatasetBasedSnowflakeSchemaName(dataset_Prod2, alwaysSuffixSchemaNames);
            string schemaName_Qual = schemaService.GenerateDatasetBasedSnowflakeSchemaName(dataset_Qual, alwaysSuffixSchemaNames);
            string schemaName_Qual2 = schemaService.GenerateDatasetBasedSnowflakeSchemaName(dataset_Qual2, alwaysSuffixSchemaNames);
            string schemaName_Test = schemaService.GenerateDatasetBasedSnowflakeSchemaName(dataset_Test, alwaysSuffixSchemaNames);

            //Assert
            string expected_DS_Name = "DSWITHLONGNAME";
            string expected_Prod_SchemaName;
            string expected_Prod2_SchemaName;
            string expected_Qual_SchemaName;
            string expected_Qual2_SchemaName;
            string expected_Test_SchemaName;

            if (alwaysSuffixSchemaNames)
            {
                expected_Prod_SchemaName = expected_DS_Name + "_PROD";
                expected_Prod2_SchemaName = expected_DS_Name + "_PROD2";
                expected_Qual_SchemaName = expected_DS_Name + "_QUAL";
                expected_Qual2_SchemaName = expected_DS_Name + "_QUAL2";
                expected_Test_SchemaName = expected_DS_Name + "_TEST";
            }
            else
            {
                expected_Prod_SchemaName = expected_DS_Name;
                expected_Prod2_SchemaName = expected_DS_Name;
                expected_Qual_SchemaName = expected_DS_Name;
                expected_Qual2_SchemaName = expected_DS_Name + "_QUAL2";
                expected_Test_SchemaName = expected_DS_Name + "_TEST";
            }

            Assert.AreEqual(expected_Prod_SchemaName, schemaName_Prod);
            Assert.AreEqual(expected_Prod2_SchemaName, schemaName_Prod2);
            Assert.AreEqual(expected_Qual_SchemaName, schemaName_Qual);
            Assert.AreEqual(expected_Qual2_SchemaName, schemaName_Qual2);
            Assert.AreEqual(expected_Test_SchemaName, schemaName_Test);

        }

        #endregion

        [TestMethod]
        public void UpdateAndSaveSchema_UnknownSchemaId_ThrowDatasetNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet((ctx) => ctx.Datasets).Returns(Enumerable.Empty<Dataset>().AsQueryable());

            SchemaService schemaService = new SchemaService(datasetContext.Object, null, null, null, null, null, null, null, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto();

            Assert.ThrowsException<DatasetNotFoundException>(() => schemaService.UpdateAndSaveSchema(dto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveSchema_UnknownSchemaId_ThrowSchemaNotFound()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Dataset ds = new Dataset() 
            { 
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema() { SchemaId = 1 },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet((ctx) => ctx.Datasets).Returns(datasets.AsQueryable());

            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, null, null, null,null, null, null);

            FileSchemaDto dto = new FileSchemaDto() { SchemaId = 5, ParentDatasetId = 2 };

            Assert.ThrowsException<SchemaNotFoundException>(() => schemaService.UpdateAndSaveSchema(dto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveSchema_BadUser_ThrowUnauthorized()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Dataset ds = new Dataset() 
            { 
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema() { SchemaId = 1 },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(ctx => ctx.Datasets).Returns(datasets.AsQueryable()).Verifiable();

            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            UserSecurity security = new UserSecurity() { CanManageSchema = false };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, null, null, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto() { SchemaId = 1, ParentDatasetId = 2 };

            Assert.ThrowsException<SchemaUnauthorizedAccessException>(() => schemaService.UpdateAndSaveSchema(dto));

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveSchema_CurrentView_TrueAndCreateEvent()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset()
            {
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>()
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema() 
                { 
                    SchemaId = 1, 
                    CreateCurrentView = false, 
                    Extension = new FileExtension() { Id = 4 } 
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(ds.DatasetFileConfigs.AsQueryable()).Verifiable();
            datasetContext.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(ds);

            datasetContext.Setup(x => x.SaveChanges(true)).Verifiable();

            SchemaRevision revision = new SchemaRevision() 
            { 
                ParentSchema = fileConfig.Schema,
                SchemaRevision_Id = 3 
            };
            List<SchemaRevision> revisions = new List<SchemaRevision>() { revision };
            datasetContext.SetupGet(x => x.SchemaRevision).Returns(revisions.AsQueryable()).Verifiable();

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mr.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> features = mr.Create<IDataFeatures>();
            features.SetupGet(x => x.CLA3605_AllowSchemaParquetUpdate).Returns(feature.Object);

            //mock publisher
            HiveTableCreateModel hiveCreate = new HiveTableCreateModel()
            {
                SchemaID = 1,
                RevisionID = 3,
                DatasetID = 2,
                HiveStatus = null,
                InitiatorID = "000000",
                ChangeIND = "{\"createcurrentview\":\"true\"}"
            };

            SnowTableCreateModel snowCreate = new SnowTableCreateModel()
            {
                SchemaID = 1,
                RevisionID = 3,
                DatasetID = 2,
                InitiatorID = "000000",
                ChangeIND = "{\"createcurrentview\":\"true\"}"
            };

            Mock<IMessagePublisher> publisher = mr.Create<IMessagePublisher>();
            publisher.Setup(x => x.PublishDSCEvent("1", JsonConvert.SerializeObject(hiveCreate), null)).Verifiable();
            publisher.Setup(x => x.PublishDSCEvent("1", JsonConvert.SerializeObject(snowCreate), null)).Verifiable();

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, features.Object, publisher.Object, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto() 
            { 
                SchemaId = 1, 
                ParentDatasetId = 2,
                CreateCurrentView = true, 
                FileExtensionId = 4 
            };

            Assert.IsTrue(schemaService.UpdateAndSaveSchema(dto));

            //verify change was detected
            Assert.AreEqual("000000", fileConfig.Schema.UpdatedBy);

            //verify current view updated
            Assert.IsTrue(fileConfig.Schema.CreateCurrentView);

            //verify extension didn't change
            Assert.AreEqual(4, fileConfig.Schema.Extension.Id);

            mr.VerifyAll();
        }


        [TestMethod]
        public void UpdateAndSaveSchema_ControlMTriggerName_CreatedCorrectly()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset()
            {
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>(),
                NamedEnvironment = "NAMEDENVIRONMENT",
                ShortName = "SHORTNAME"
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema()
                {
                    SchemaId = 1,
                    CreateCurrentView = false,
                    Extension = new FileExtension() { Id = 4 },
                    Name = "SCHEMANAME"
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(ds.DatasetFileConfigs.AsQueryable()).Verifiable();
            datasetContext.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(ds);

            datasetContext.Setup(x => x.SaveChanges(true)).Verifiable();

            SchemaRevision revision = new SchemaRevision()
            {
                ParentSchema = fileConfig.Schema,
                SchemaRevision_Id = 3
            };
            List<SchemaRevision> revisions = new List<SchemaRevision>() { revision };
            datasetContext.SetupGet(x => x.SchemaRevision).Returns(revisions.AsQueryable()).Verifiable();

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mr.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> features = mr.Create<IDataFeatures>();
            features.SetupGet(x => x.CLA3605_AllowSchemaParquetUpdate).Returns(feature.Object);

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, features.Object, null, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto()
            {
                SchemaId = 1,
                ParentDatasetId = 2,
                CreateCurrentView = true,
                FileExtensionId = 4,
                Name = "SCHEMANAME"
            };

            Assert.IsTrue(schemaService.UpdateAndSaveSchema(dto));

            //verify ControlMTriggerName was created correctly
            Assert.AreEqual("DATA_NAMEDENVIRONMENT_SHORTNAME_SCHEMANAME_COMPLETED", fileConfig.Schema.ControlMTriggerName);

            mr.VerifyAll();
        }


        [TestMethod]
        public void UpdateAndSaveSchema_ControlMTriggerName_CleansedCorrectly()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset()
            {
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>(),
                NamedEnvironment = "NamedEnvironment%$$#_",
                ShortName = "short9839)(!@"
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema()
                {
                    SchemaId = 1,
                    CreateCurrentView = false,
                    Extension = new FileExtension() { Id = 4 },
                    Name = "schema&^Name"
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(ds.DatasetFileConfigs.AsQueryable()).Verifiable();
            datasetContext.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(ds);

            datasetContext.Setup(x => x.SaveChanges(true)).Verifiable();

            SchemaRevision revision = new SchemaRevision()
            {
                ParentSchema = fileConfig.Schema,
                SchemaRevision_Id = 3
            };
            List<SchemaRevision> revisions = new List<SchemaRevision>() { revision };
            datasetContext.SetupGet(x => x.SchemaRevision).Returns(revisions.AsQueryable()).Verifiable();

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mr.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> features = mr.Create<IDataFeatures>();
            features.SetupGet(x => x.CLA3605_AllowSchemaParquetUpdate).Returns(feature.Object);

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, features.Object, null, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto()
            {
                SchemaId = 1,
                ParentDatasetId = 2,
                CreateCurrentView = true,
                FileExtensionId = 4,
                Name = "schema&^Name"
            };

            Assert.IsTrue(schemaService.UpdateAndSaveSchema(dto));

            //verify ControlMTriggerName was created correctly
            Assert.AreEqual("DATA_NAMEDENVIRONMENT_SHORT9839_SCHEMANAME_COMPLETED", fileConfig.Schema.ControlMTriggerName);

            mr.VerifyAll();
        }


        [TestMethod]
        public void UpdateAndSaveSchema_ParquetStorage_TrueAndCreateEvent()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset()
            {
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>()
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema()
                {
                    SchemaId = 1,
                    ParquetStorageBucket = "Bucket",
                    ParquetStoragePrefix = "Prefix",
                    Extension = new FileExtension() { Id = 4 }
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();
            datasetContext.SetupGet(x => x.DatasetFileConfigs).Returns(ds.DatasetFileConfigs.AsQueryable()).Verifiable();
            datasetContext.Setup(x => x.SaveChanges(true)).Verifiable();
            datasetContext.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(ds);

            SchemaRevision revision = new SchemaRevision()
            {
                ParentSchema = fileConfig.Schema,
                SchemaRevision_Id = 3
            };
            List<SchemaRevision> revisions = new List<SchemaRevision>() { revision };
            datasetContext.SetupGet(x => x.SchemaRevision).Returns(revisions.AsQueryable()).Verifiable();

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mr.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> features = mr.Create<IDataFeatures>();
            features.SetupGet(x => x.CLA3605_AllowSchemaParquetUpdate).Returns(feature.Object);

            //mock publisher
            HiveTableCreateModel hiveCreate = new HiveTableCreateModel()
            {
                SchemaID = 1,
                RevisionID = 3,
                DatasetID = 2,
                HiveStatus = null,
                InitiatorID = "000000",
                ChangeIND = "{\"parquetstoragebucket\":\"newbucket\",\"parquetstorageprefix\":\"newprefix\"}"
            };

            SnowTableCreateModel snowCreate = new SnowTableCreateModel()
            {
                SchemaID = 1,
                RevisionID = 3,
                DatasetID = 2,
                InitiatorID = "000000",
                ChangeIND = "{\"parquetstoragebucket\":\"newbucket\",\"parquetstorageprefix\":\"newprefix\"}"
            };

            Mock<IMessagePublisher> publisher = mr.Create<IMessagePublisher>();
            publisher.Setup(x => x.PublishDSCEvent("1", JsonConvert.SerializeObject(hiveCreate), null)).Verifiable();
            publisher.Setup(x => x.PublishDSCEvent("1", JsonConvert.SerializeObject(snowCreate), null)).Verifiable();

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, features.Object, publisher.Object, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto()
            {
                SchemaId = 1,
                ParquetStorageBucket = "NewBucket",
                ParquetStoragePrefix = "NewPrefix",
                FileExtensionId = 4,
                ParentDatasetId = 2
            };

            Assert.IsTrue(schemaService.UpdateAndSaveSchema(dto));

            //verify change was detected
            Assert.AreEqual("000000", fileConfig.Schema.UpdatedBy);

            //verify parquet properties updated
            Assert.AreEqual("NewBucket", fileConfig.Schema.ParquetStorageBucket);
            Assert.AreEqual("NewPrefix", fileConfig.Schema.ParquetStoragePrefix);

            //verify extension didn't change
            Assert.AreEqual(4, fileConfig.Schema.Extension.Id);

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveSchema_FileExtension_True()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset()
            {
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>()
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema()
                {
                    SchemaId = 1,
                    Extension = new FileExtension() { Id = 4,  }
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();
            datasetContext.Setup(x => x.SaveChanges(true)).Verifiable();
            datasetContext.Setup(x => x.GetById<FileExtension>(5)).Returns(new FileExtension() { Id = 5 }).Verifiable();
            datasetContext.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(ds);

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            appUser.SetupGet(x => x.AssociateId).Returns("000000");
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mr.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> features = mr.Create<IDataFeatures>();
            features.SetupGet(x => x.CLA3605_AllowSchemaParquetUpdate).Returns(feature.Object);

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, features.Object, null, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto()
            {
                SchemaId = 1,
                FileExtensionId = 5,
                ParentDatasetId = 2
            };

            Assert.IsTrue(schemaService.UpdateAndSaveSchema(dto));
            //verify change was detected
            Assert.AreEqual("000000", fileConfig.Schema.UpdatedBy);

            //verify extension
            Assert.AreEqual(5, fileConfig.Schema.Extension.Id);

            mr.VerifyAll();
        }

        [TestMethod]
        public void UpdateAndSaveSchema_NoUpdate_True()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset()
            {
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>()
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema()
                {
                    SchemaId = 1,
                    Name = "Name",
                    Description = "Description",
                    UpdatedBy = "000001",
                    Extension = new FileExtension() { Id = 4, }
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();
            datasetContext.Setup(x => x.SaveChanges(true)).Verifiable();
            datasetContext.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(ds);

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();
            userService.Setup(x => x.GetCurrentUser().AssociateId).Returns(fileConfig.Schema.UpdatedBy);

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            //mock features
            Mock<IFeatureFlag<bool>> feature = mr.Create<IFeatureFlag<bool>>();
            feature.Setup(x => x.GetValue()).Returns(true);
            Mock<IDataFeatures> features = mr.Create<IDataFeatures>();
            features.SetupGet(x => x.CLA3605_AllowSchemaParquetUpdate).Returns(feature.Object);

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, features.Object, null, null, null, null, null);

            FileSchemaDto dto = new FileSchemaDto()
            {
                SchemaId = 1,
                Name = "Name",
                Description = "Description",
                FileExtensionId = 4,
                ParentDatasetId = 2
            };

            Assert.IsTrue(schemaService.UpdateAndSaveSchema(dto));
            //verify properties have not changed
            Assert.AreEqual("000001", fileConfig.Schema.UpdatedBy);
            Assert.AreEqual("Name", fileConfig.Schema.Name);
            Assert.AreEqual("Description", fileConfig.Schema.Description);

            mr.VerifyAll();
        }

        [TestMethod]
        public void GetLatestSchemaRevisionJsonStructureBySchemaId_1_SchemaRevisionJsonStructureDto()
        {
            MockRepository mr = new MockRepository(MockBehavior.Strict);

            //mock context
            Dataset ds = new Dataset() 
            { 
                DatasetId = 2,
                ObjectStatus = ObjectStatusEnum.Active,
                DatasetFileConfigs = new List<DatasetFileConfig>()
            };

            DatasetFileConfig fileConfig = new DatasetFileConfig()
            {
                Schema = new FileSchema()
                {
                    SchemaId = 1,
                    Revisions = new List<SchemaRevision>() { new SchemaRevision()
                    {
                        SchemaRevision_Name = "StructureTest",
                        Revision_NBR = 1,
                        Fields = new List<BaseField>() { new IntegerField() { Name = "fieldname", Description = "Field Description" } }
                    } }
                },
                ParentDataset = ds
            };

            ds.DatasetFileConfigs = new List<DatasetFileConfig>() { fileConfig };
            List<Dataset> datasets = new List<Dataset>() { ds };

            Mock<IDatasetContext> datasetContext = mr.Create<IDatasetContext>();
            datasetContext.SetupGet(x => x.Datasets).Returns(datasets.AsQueryable()).Verifiable();

            //mock user service
            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            //mock security service
            UserSecurity security = new UserSecurity() { CanManageSchema = true };
            Mock<ISecurityService> securityService = mr.Create<ISecurityService>();
            securityService.Setup(x => x.GetUserSecurity(ds, appUser.Object)).Returns(security).Verifiable();

            SchemaService schemaService = new SchemaService(datasetContext.Object, userService.Object, null, null, null, securityService.Object, null, null, null,null, null, null);

            SchemaRevisionJsonStructureDto dto = schemaService.GetLatestSchemaRevisionJsonStructureBySchemaId(2, 1);

            Assert.IsNotNull(dto.Revision);
            Assert.AreEqual(1, dto.Revision.RevisionNumber);
            Assert.AreEqual("StructureTest", dto.Revision.SchemaRevisionName);

            Assert.IsNotNull(dto.JsonStructure);
            Assert.IsTrue(JToken.DeepEquals(GetData("BasicSchema_Integer.json"), dto.JsonStructure));

            mr.VerifyAll();
        }

        [TestMethod]
        public void TestDotNamePathBuild()
        {
            List<BaseField> fields = BuildMockNestedSchema();
            SchemaService schemaService = new SchemaService(null, null, null, null, null, null, null, null, null, null, null, null);
            schemaService.SetHierarchyProperties(fields);
            Assert.AreEqual("Root", fields[0].DotNamePath);
            Assert.AreEqual("Root.Middle", fields[1].DotNamePath);
            Assert.AreEqual("Root.Middle.Child", fields[2].DotNamePath);
        }

        [TestMethod]
        public void SetHierarchyProperties_Fields_StructurePositions()
        {
            List<BaseField> fields = new List<BaseField>()
            {
                new VarcharField() { Name = "Varchar" },
                new StructField() { Name = "RootStruct" },
                new IntegerField() { Name = "Integer" },
                new StructField() { Name = "RootStruct2"}
            };

            VarcharField midVarchar = new VarcharField()
            {
                Name = "MidVarchar",
                ParentField = fields[1]
            };

            StructField midStruct = new StructField()
            {
                Name = "MidStruct",
                ParentField = fields[1]
            };

            DateField lowDate = new DateField()
            {
                Name = "LowDate",
                ParentField = midStruct
            };

            BigIntField lowBigInt = new BigIntField()
            {
                Name = "LowBigInt",
                ParentField = midStruct
            };

            DecimalField midDecimal = new DecimalField()
            {
                Name = "MidDecimal",
                ParentField = fields[3]
            };

            VarcharField midVarchar2 = new VarcharField()
            {
                Name = "MidVarchar2",
                ParentField = fields[3]
            };

            SchemaService service = new SchemaService(null, null, null, null, null, null, null, null, null, null, null);
            service.SetHierarchyProperties(fields);

            Assert.AreEqual("1", fields.First().StructurePosition);
            Assert.AreEqual("2", fields[1].StructurePosition);

            IList<BaseField> childFields = fields[1].ChildFields;
            Assert.AreEqual("2.1", childFields.First().StructurePosition);
            Assert.AreEqual("2.2", childFields.Last().StructurePosition);

            childFields = childFields.Last().ChildFields;
            Assert.AreEqual("2.2.1", childFields.First().StructurePosition);
            Assert.AreEqual("2.2.2", childFields.Last().StructurePosition);

            Assert.AreEqual("3", fields[2].StructurePosition);
            Assert.AreEqual("4", fields.Last().StructurePosition);

            childFields = fields.Last().ChildFields;
            Assert.AreEqual("4.1", childFields.First().StructurePosition);
            Assert.AreEqual("4.2", childFields.Last().StructurePosition);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void UpdateSchema_SnowflakeStage(bool AllowUpdateFlag)
        {
            FileSchemaDto dto = new FileSchemaDto()
            {
                Name = "Name-NewValue",
                ConsumptionDetails = new List<SchemaConsumptionDto>() { new SchemaConsumptionSnowflakeDto() { SchemaConsumptionId = 1, SnowflakeStage = "SnowflakeStage-NewValue" } },
                FileExtensionId = 1
            };

            FileSchema schema = new FileSchema()
            {
                ConsumptionDetails = new List<SchemaConsumption>() { new SchemaConsumptionSnowflake() { SchemaConsumptionId = 1, SnowflakeStage = "SnowflakeStage-OriginalValue" } },
                Extension = new FileExtension() { Id = 2 }
            };

            MockRepository mr = new MockRepository(MockBehavior.Strict);

            Mock<IApplicationUser> appUser = mr.Create<IApplicationUser>();
            appUser.Setup(x => x.AssociateId).Returns("123456");

            Mock<IUserService> userService = mr.Create<IUserService>();
            userService.Setup(x => x.GetCurrentUser()).Returns(appUser.Object).Verifiable();

            Mock<IDataFeatures> flags = mr.Create<IDataFeatures>();
            flags.Setup(x => x.CLA3605_AllowSchemaParquetUpdate.GetValue()).Returns(AllowUpdateFlag);

            Mock<IDatasetContext> context = mr.Create<IDatasetContext>();
            context.Setup(x => x.GetById<FileExtension>(It.IsAny<int>())).Returns(new FileExtension() { Id = 1 });
            context.Setup(x => x.GetById<Dataset>(It.IsAny<int>())).Returns(new Dataset() { DatasetId = 1 });

            SchemaService schemaService = new SchemaService(context.Object, userService.Object, null, null, null, null, flags.Object, null, null, null, null, null);

            //ACT
            schemaService.UpdateSchema(dto, schema);

            var snowstage = dto.ConsumptionDetails.OfType<SchemaConsumptionSnowflakeDto>().First().SnowflakeStage == schema.ConsumptionDetails.OfType<SchemaConsumptionSnowflake>().First().SnowflakeStage;

            //ASSERT
            Assert.AreEqual(snowstage, AllowUpdateFlag);
        }

        [TestMethod]
        public void ValidateCleanedFields_1_BaseFieldDtos_Success()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 1,
                Extension = new FileExtension() { Name = GlobalConstants.ExtensionNames.JSON }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(1)).Returns(fileSchema);

            SchemaService service = new SchemaService(datasetContext.Object, null, null, null, null, null, null, null, null, null, null, null);

            List<BaseFieldDto> dtos = new List<BaseFieldDto>()
            {
                new VarcharFieldDto(new SchemaRow()
                {
                    Name = "VarcharField",
                    Position = 1,
                    Length = "10"
                }),
                new StructFieldDto(new SchemaRow()
                {
                    Name = "StructField",
                    Position = 2
                }) 
                { 
                    HasChildren = true, 
                    ChildFields = new List<BaseFieldDto>() 
                    { 
                        new VarcharFieldDto(new SchemaRow()
                        {
                            Name = "ChildVarcharField",
                            Position = 1,
                            Length = "10"
                        }) 
                    } 
                },
                new IntegerFieldDto(new SchemaRow()
                {
                    Name = "IntegerField",
                    Position = 1,
                    Length = "10"
                })
            };
            
            //verifying that exception does not get thrown
            service.ValidateCleanedFields(1, dtos);

            datasetContext.VerifyAll();

            //verify integer was cleaned
            Assert.AreEqual(0, dtos.Last().Length);
        }

        [TestMethod]
        public void ValidateCleanedFields_1_BaseFieldDtos_Duplicates_ValidationResults()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 1,
                Extension = new FileExtension() { Name = GlobalConstants.ExtensionNames.JSON }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(1)).Returns(fileSchema);

            SchemaService service = new SchemaService(datasetContext.Object, null, null, null, null, null, null, null, null, null, null, null);

            List<BaseFieldDto> dtos = new List<BaseFieldDto>()
            {
                new VarcharFieldDto(new SchemaRow()
                {
                    Name = "VarcharField",
                    Position = 1,
                    Length = "10"
                }),
                new VarcharFieldDto(new SchemaRow()
                {
                    Name = "VarcharField",
                    Position = 2,
                    Length = "10"
                })
            };

            //verifying that exception does not get thrown
            ValidationException exception = Assert.ThrowsException<ValidationException>(() => service.ValidateCleanedFields(1, dtos));

            Assert.AreEqual("Validation errors occurred: (VarcharField) cannot be duplicated. , (VarcharField) cannot be duplicated. ", exception.Message);
            Assert.AreEqual(2, exception.ValidationResults.GetAll().Count);

            datasetContext.VerifyAll();
        }

        [TestMethod]
        public void ValidateCleanedFields_1_BaseFieldDtos_DtoValidations_ValidationResults()
        {
            Mock<IDatasetContext> datasetContext = new Mock<IDatasetContext>(MockBehavior.Strict);
            FileSchema fileSchema = new FileSchema()
            {
                SchemaId = 1,
                Extension = new FileExtension() { Name = GlobalConstants.ExtensionNames.JSON }
            };

            datasetContext.Setup(x => x.GetById<FileSchema>(1)).Returns(fileSchema);

            SchemaService service = new SchemaService(datasetContext.Object, null, null, null, null, null, null, null, null, null, null, null);

            List<BaseFieldDto> dtos = new List<BaseFieldDto>()
            {
                new VarcharFieldDto(new SchemaRow()
                {
                    Name = "1Varchar Field",
                    Position = 1,
                    Length = "10"
                }),
                new StructFieldDto(new SchemaRow()
                {
                    Name = "StructField",
                    Position = 2
                })
                {
                    HasChildren = true,
                    ChildFields = new List<BaseFieldDto>()
                    {
                        new VarcharFieldDto(new SchemaRow()
                        {
                            Name = "Child Varchar Field",
                            Position = 1,
                            Length = "10"
                        })
                    }
                }
            };

            //verifying that exception does not get thrown
            ValidationException exception = Assert.ThrowsException<ValidationException>(() => service.ValidateCleanedFields(1, dtos));

            Assert.AreEqual("Validation errors occurred: Field name (1Varchar Field) must start with a letter or underscore, Field name (1Varchar Field) can only contain letters, underscores, digits (0-9), and dollar signs (\"$\"), Field name (Child Varchar Field) can only contain letters, underscores, digits (0-9), and dollar signs (\"$\")", exception.Message);
            Assert.AreEqual(3, exception.ValidationResults.GetAll().Count);

            datasetContext.VerifyAll();
        }

        #region Private Methods
        private JsonSchema BuildMockJsonSchemaWithDecimalField()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""salary"":{
                        ""type"": ""number"",
                        ""dsc-precision"": 6,
                        ""dsc-scale"": 2,
                        ""description"": ""Decimal field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
        private JsonSchema BuildMockJsonSchemaWithDecimalFieldArray()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""decimalarray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""number"",
                            ""dsc-precision"": 6,
                            ""dsc-scale"": 2
                            },
                        ""description"": ""Decimal field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }

        private JsonSchema BuildMockJsonSchemaWithVarcharField()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""firstname"":{
                        ""type"": ""string"",
                        ""maxlength"": 99,
                        ""description"": ""Varchar field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
        private JsonSchema BuildMockJsonSchemaWithVarcharFieldArray()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""varchararray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""maxlength"": 15
                            },
                        ""description"": ""Varchar field array""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }

        private JsonSchema BuildMockJsonSchemaWithDateField()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""DOB"":{
                        ""type"": ""string"",
                        ""format"": ""date"",
                        ""dsc-format"": ""yy-mm-dd"",
                        ""description"": ""Date field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
        private JsonSchema BuildMockJsonSchemaWithDateFieldArray()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""datearray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date"",
                            ""dsc-format"": ""yy-mm-dd"",
                            ""description"": ""Date field""
                            },
                        ""description"": ""Date field array""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }

        private JsonSchema BuildMockJsonSchemaWithTimestampField()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""createdtm"":{
                        ""type"": ""string"",
                        ""format"": ""date-time"",
                        ""dsc-format"": ""yy-mm-dd hh:mm:ss"",
                        ""description"": ""Timestamp field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
        private JsonSchema BuildMockJsonSchemaWithTimestampFieldArray()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""timestamparray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date-time"",
                            ""dsc-format"": ""yy-mm-dd hh:mm:ss"",
                            ""description"": ""Date field""
                            },
                        ""description"": ""Timestamp field array""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }


        private JsonSchema BuildMockJsonSchemaWithIntegerField()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""age"":{
                        ""type"": ""integer"",
                        ""description"": ""Integer field""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
        private JsonSchema BuildMockJsonSchemaWithIntegerFieldArray()
        {
            string jsonSchema = @"{
                ""type"": ""object"",
                ""properties"": { 
                    ""timestamparray"":{
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date-time"",
                            ""dsc-format"": ""yy-mm-dd hh:mm:ss"",
                            ""description"": ""Date field""
                            },
                        ""description"": ""Timestamp field array""
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
        private FileSchema BuildMockFileSchema(string extensionType, bool addRevision, bool deleted, int deletedDaysAgo, string[] fieldTypes)
        {
            FileSchema s = new FileSchema()
            {
                CreateCurrentView = true,
                CreatedBy = "072984",
                CreatedDTM = DateTime.Parse("2020-06-17 13:08:00"),
                Delimiter = ",",
                Description = "Mock Schema Description",
                Extension = BuildMockFileExtension(extensionType),
                HasHeader = true,
                HiveDatabase = "dscqual_sentry",
                HiveLocation = "data/1000002",
                HiveTable = "mockschema",
                HiveTableStatus = "Available",
                LastUpdatedDTM = DateTime.Parse("2020-06-17 13:08:10"),
                Name = "Mock Schema",
                SasLibrary = "DSCQUAL_sentry",
                SchemaEntity_NME = "Mock Schema",
                SchemaId = 88,
                StorageCode = "1000002",
                UpdatedBy = "072984",
                CLA1396_NewEtlColumns = false,
                CLA1580_StructureHive = false,
                Revisions = new List<SchemaRevision>()
            };

            if (addRevision)
            {
                s.Revisions.Add(BuildMockSchemaRevision(s, fieldTypes));
            }

            if (deleted)
            {
                s.DeleteInd = true;
                s.DeleteIssueDTM = DateTime.Now.AddDays((deletedDaysAgo * -1));
                s.DeleteIssuer = "072984";
            }
            else
            {
                s.DeleteInd = false;
                s.DeleteIssueDTM = DateTime.MinValue;
                s.DeleteIssuer = null;
            }

            return s;
        }
        private FileExtension BuildMockFileExtension(string extension)
        {
            FileExtension ext = new FileExtension()
            {
                Created = DateTime.Parse("2020-06-16 13:08:10"),
                CreatedUser = "072984"
            };

            switch (extension.ToLower())
            {

                case "json":
                    ext.Name = GlobalConstants.ExtensionNames.JSON;
                    ext.Id = 2;
                    break;
                case "txt":
                    ext.Name = GlobalConstants.ExtensionNames.TXT;
                    ext.Id = 3;
                    break;
                case "any":
                    ext.Name = GlobalConstants.ExtensionNames.ANY;
                    ext.Id = 4;
                    break;
                case "delimited":
                    ext.Name = GlobalConstants.ExtensionNames.DELIMITED;
                    ext.Id = 5;
                    break;
                case "csv":
                default:
                    ext.Name = GlobalConstants.ExtensionNames.CSV;
                    ext.Id = 1;
                    break;
            }

            return ext;
        }
        private SchemaRevision BuildMockSchemaRevision(FileSchema parentSchema, string[] fieldTypes)
        {
            SchemaRevision revision = new SchemaRevision()
            {
                CreatedBy = "072984",
                CreatedDTM = DateTime.Parse("2020-06-17 13:08:11"),
                Fields = new List<BaseField>(),
                JsonSchemaObject = null,
                LastUpdatedDTM = DateTime.Parse("2020-06-17 13:08:12"),
                ParentSchema = parentSchema,
                Revision_NBR = 1,
                SchemaRevision_Id = 77,
                SchemaRevision_Name = "Revision_1"
            };

            foreach (string type in fieldTypes)
            {
                switch (type.ToUpper())
                {
                    case "DECIMAL":
                        revision.Fields.Add(BuildMockDecimalField(revision));
                        break;
                    default:
                        break;
                }
            }

            return revision;
        }
       
        private DecimalField BuildMockDecimalField(SchemaRevision parentRevision)
        {
            DecimalField field = new DecimalField()
            {
                Name = "decimalField",
                CreateDTM = DateTime.Parse("2020-06-17 13:08:00"),
                Description = "Decimal Field",
                EndPosition = 0,
                IsArray = false,
                LastUpdateDTM = DateTime.Parse("2020-06-17 13:08:00"),
                NullableIndicator = true,
                OrdinalPosition = parentRevision.Fields.Count + 1,
                ParentField = null,
                Precision = 6,
                Scale = 2,
                StartPosition = 0,
                FieldId = 99,
                ParentSchemaRevision = parentRevision,
                FieldGuid = Guid.Parse("c5023db5-7125-4faf-acca-22b3f1e8bc79")
            };

            return field;
        }

        private List<BaseField> BuildMockNestedSchema()
        {
            List<BaseField> schemaList = new List<BaseField>();
            StructField root = new StructField();
            root.Name = "Root";
            StructField middle = new StructField();
            middle.ParentField = root;
            middle.Name = "Middle";
            StructField child = new StructField();
            child.ParentField = middle;
            child.Name = "Child";
            schemaList.Add(root);
            schemaList.Add(middle);
            schemaList.Add(child);
            return schemaList;
        }
        #endregion
    }
}
