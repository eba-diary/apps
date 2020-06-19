using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using Sentry.data.Core.DTO.Schema.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{

    [TestClass]
    public class SchemaServiceTests
    {
        #region DecimalFieldDto_JsonConstructor Tests

        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_False_IsArray_From_BaseFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_True_IsArray_From_BaseFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, true);

            Assert.IsNotNull(dto);
            Assert.AreEqual(true, dto.IsArray);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Precision_From_DecimalFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            Assert.IsNotNull(dto);
            Assert.AreEqual(6, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Scale_From_DecimalFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            Assert.IsNotNull(dto);
            Assert.AreEqual(2, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Null_SourceFormat_From_DecimalFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            Assert.IsNotNull(dto);
            Assert.AreEqual(null, dto.SourceFormat);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_0_Length_From_DecimalFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Length);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_0_OrdinalPosition_From_DecimalFieldDto_Json_Constructor()
        {

            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.OrdinalPosition);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Default_Percision_From_DecimalFieldDto_Json_Constructor()
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_Default_Scale_From_DecimalFieldDto_Json_Constructor()
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_dscscale_Value_IsNull_To_DecimalFieldDto_Json_Constructor()
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_dscscale_DoesNotExist_To_DecimalFieldDto_Json_Constructor()
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Scale);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_dscprecision_DoesNotExist_To_DecimalFieldDto_Json_Constructor()
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Precision);
        }
        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_dscprecision_Value_IsNull_To_DecimalFieldDto_Json_Constructor()
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual(0, dto.Precision);
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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("salary", dto.Name);
            Assert.AreEqual("Decimal field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDTM);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDTM);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDTM);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDTM);
            Assert.AreEqual(0, dto.FieldId);
            Assert.AreEqual(Guid.Empty, dto.FieldGuid);
            Assert.AreEqual(false, dto.DeleteInd);
            Assert.AreEqual(false, dto.IsArray);
        }
        #endregion

        #region DecimalFieldDto SchemaRow Constructor Tests

        [TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        public void Get_False_IsArray_From_BaseFieldDto_SchemaRow_Constructor()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

            //Action
            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

            //Assersion
            Assert.IsNotNull(dto);
            Assert.AreEqual(false, dto.IsArray);
        }
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_True_IsArray_From_BaseFieldDto_Json_Constructor()
        //{

        //    JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, true);

        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(true, dto.IsArray);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_Precision_From_DecimalFieldDto_Json_Constructor()
        //{

        //    JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(6, dto.Precision);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_Scale_From_DecimalFieldDto_Json_Constructor()
        //{

        //    JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(2, dto.Scale);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_Null_SourceFormat_From_DecimalFieldDto_Json_Constructor()
        //{

        //    JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(null, dto.SourceFormat);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_0_Length_From_DecimalFieldDto_Json_Constructor()
        //{

        //    JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(0, dto.Length);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_0_OrdinalPosition_From_DecimalFieldDto_Json_Constructor()
        //{

        //    JsonSchema schema = BuildMockJsonSchemaWithDecimalField();

        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(0, dto.OrdinalPosition);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_Default_Percision_From_DecimalFieldDto_Json_Constructor()
        //{
        //    //Setup
        //    string jsonSchema = @"{
        //        ""type"": ""object"",
        //        ""properties"": { 
        //            ""salary"":{
        //                ""type"": ""number"",
        //                ""dsc-scale"": 2,
        //                ""description"": ""Decimal field""
        //            }
        //        }
        //    }";
        //    JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();

        //    //Action
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    //Assertion
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(1, dto.Precision);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_Default_Scale_From_DecimalFieldDto_Json_Constructor()
        //{
        //    //Setup
        //    string jsonSchema = @"{
        //        ""type"": ""object"",
        //        ""properties"": { 
        //            ""salary"":{
        //                ""type"": ""number"",
        //                ""dsc-scale"": null,
        //                ""description"": ""Decimal field""
        //            }
        //        }
        //    }";
        //    JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();

        //    //Action
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    //Assertion
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(1, dto.Scale);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_dscscale_Value_IsNull_To_DecimalFieldDto_Json_Constructor()
        //{
        //    //Setup
        //    string jsonSchema = @"{
        //        ""type"": ""object"",
        //        ""properties"": { 
        //            ""salary"":{
        //                ""type"": ""number"",
        //                ""dsc-scale"": null,
        //                ""description"": ""Decimal field""
        //            }
        //        }
        //    }";
        //    JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();

        //    //Action
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    //Assertion
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(1, dto.Scale);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_dscscale_DoesNotExist_To_DecimalFieldDto_Json_Constructor()
        //{
        //    //Setup
        //    string jsonSchema = @"{
        //        ""type"": ""object"",
        //        ""properties"": { 
        //            ""salary"":{
        //                ""type"": ""number"",
        //                ""description"": ""Decimal field""
        //            }
        //        }
        //    }";
        //    JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();

        //    //Action
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    //Assertion
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(1, dto.Scale);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_dscprecision_DoesNotExist_To_DecimalFieldDto_Json_Constructor()
        //{
        //    //Setup
        //    string jsonSchema = @"{
        //        ""type"": ""object"",
        //        ""properties"": { 
        //            ""salary"":{
        //                ""type"": ""number"",
        //                ""dsc-scale"": 2,
        //                ""description"": ""Decimal field""
        //            }
        //        }
        //    }";
        //    JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();

        //    //Action
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    //Assertion
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(1, dto.Precision);
        //}
        //[TestMethod, TestCategory("DecimalFieldDto JsonContructor")]
        //public void Get_dscprecision_Value_IsNull_To_DecimalFieldDto_Json_Constructor()
        //{
        //    //Setup
        //    string jsonSchema = @"{
        //        ""type"": ""object"",
        //        ""properties"": { 
        //            ""salary"":{
        //                ""type"": ""number"",
        //                ""dsc-precision"": null,
        //                ""dsc-scale"": 2,
        //                ""description"": ""Decimal field""
        //            }
        //        }
        //    }";
        //    JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
        //    KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();

        //    //Action
        //    DecimalFieldDto dto = new DecimalFieldDto(prop, false);

        //    //Assertion
        //    Assert.IsNotNull(dto);
        //    Assert.AreEqual(1, dto.Precision);
        //}

        #endregion

        #region DecimalFieldDto Field Constructor Tests
        [TestMethod, TestCategory("DecimalFieldDto Field Constructor")]
        public void Get_Precision_from_DecimalFieldDto_DecimalField_Constructor()
        {
            //setup
            FileSchema schema = BuildMockFileSchema("csv", true, false, 0, new string[] { "decimal" });
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.Where(w => w.FieldType == SchemaDatatypes.DECIMAL).First();

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
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.Where(w => w.FieldType == SchemaDatatypes.DECIMAL).First();

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
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.Where(w => w.FieldType == SchemaDatatypes.DECIMAL).First();

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
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.Where(w => w.FieldType == SchemaDatatypes.DECIMAL).First();

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
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.Where(w => w.FieldType == SchemaDatatypes.DECIMAL).First();

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
            DecimalField field = (DecimalField)schema.Revisions[0].Fields.Where(w => w.FieldType == SchemaDatatypes.DECIMAL).First();

            //Action
            DecimalFieldDto dto = new DecimalFieldDto(field);

            //Assertion
            Assert.IsNotNull(dto);
            Assert.AreEqual("decimalField", dto.Name);
            Assert.AreEqual("Decimal Field", dto.Description);
            Assert.IsNotNull(dto.ChildFields);
            Assert.AreEqual(0, dto.ChildFields.Count);
            Assert.AreNotEqual(DateTime.MinValue, dto.CreateDTM);
            Assert.AreNotEqual(DateTime.MaxValue, dto.CreateDTM);
            Assert.AreNotEqual(DateTime.MinValue, dto.LastUpdatedDTM);
            Assert.AreNotEqual(DateTime.MaxValue, dto.LastUpdatedDTM);
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

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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

            KeyValuePair<string, JsonSchemaProperty> prop = schema.Properties.First();
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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
            DecimalFieldDto dto = new DecimalFieldDto(prop, false);

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
                IsInSAS = true,
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
                    ext.Name = "JSON";
                    ext.Id = 2;
                    break;
                case "txt":
                    ext.Name = "TXT";
                    ext.Id = 3;
                    break;
                case "any":
                    ext.Name = "ANY";
                    ext.Id = 4;
                    break;
                case "delimited":
                    ext.Name = "DELIMITED";
                    ext.Id = 5;
                    break;
                case "csv":
                default:
                    ext.Name = "CSV";
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
        private DecimalField BuildMockDecimalField(SchemaRevision parentRevision, string fieldType)
        {
            DecimalField field = new DecimalField()
            {
                Name = "decimalField",
                CreateDTM = DateTime.Parse("2020-06-17 13:08:00"),
                Description = "Decimal Field",
                EndPosition = 0,
                FieldType = SchemaDatatypes.DECIMAL,
                IsArray = false,
                LastUpdateDTM = DateTime.Parse("2020-06-17 13:08:00"),
                NullableIndicator = false,
                OrdinalPosition = parentRevision.Fields.Count + 1,
                ParentField = null,
                Precision = 6,
                Scale = 2,
                StartPosition = 0,
                FieldId = 99,
                ParentSchemaRevision = parentRevision
            };
            return field;
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

    }
}
