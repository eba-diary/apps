﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.Core;
using Sentry.data.Core.DTO.Schema.Fields;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class FieldDtoTests
    {
        [TestMethod]
        public void Validate_VarcharFieldDto_Success()
        {
            SchemaRow row = new SchemaRow()
            {
                Length = "10"
            };

            SetSchemaRow(row);
            
            VarcharFieldDto dto = new VarcharFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        [TestMethod]
        public void Validate_VarcharFieldDto_Fail()
        {
            SchemaRow row = new SchemaRow() { Length = "0" };
            SetSchemaRow(row);
            row.Name = "123 Test";

            VarcharFieldDto dto = new VarcharFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(3, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Field name (123 Test) must start with a letter or underscore", result.Description);

            result = resultList[1];
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Field name (123 Test) can only contain letters, underscores, digits (0-9), and dollar signs (\"$\")", result.Description);

            result = resultList.Last();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(123 Test) VARCHAR length (0) is required to be between 1 and 16000000", result.Description);
        }

        [TestMethod]
        public void Validate_TimestampFieldDto_Success()
        {
            SchemaRow row = new SchemaRow() { Format = GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT };
            SetSchemaRow(row);

            TimestampFieldDto dto = new TimestampFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        //[TestMethod]
        //public void Validate_TimestampFieldDto_FIXEDWIDTH_Success()
        //{
        //    SchemaRow row = new SchemaRow() { Format = GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT };
        //    SetSchemaRow(row);

        //    TimestampFieldDto dto = new TimestampFieldDto(row);

        //    ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

        //    Assert.IsTrue(results.IsValid());
        //}

        [TestMethod]
        public void Validate_TimestampFieldDto_FIXEDWIDTH_SourceFormat_Fail()
        {
            SchemaRow row = new SchemaRow() { Format = GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT };
            SetSchemaRow(row);

            TimestampFieldDto dto = new TimestampFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(2, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be greater than zero for FIXEDWIDTH schema", result.Description);

            result = resultList.Last();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be equal or greater than specified format for FIXEDWIDTH schema", result.Description);
        }

        [TestMethod]
        public void Validate_TimestampFieldDto_FIXEDWIDTH_NoSourceFormat_Fail()
        {
            SchemaRow row = new SchemaRow();
            SetSchemaRow(row);

            TimestampFieldDto dto = new TimestampFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(2, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be greater than zero for FIXEDWIDTH schema", result.Description);

            result = resultList.Last();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be equal or greater than default format length (19) for FIXEDWIDTH schema", result.Description);
        }

        [TestMethod]
        public void Validate_StuctFieldDto_Success()
        {
            SchemaRow row = new SchemaRow();

            SetSchemaRow(row);

            StructFieldDto dto = new StructFieldDto(row);
            dto.HasChildren = true;

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        [TestMethod]
        public void Validate_StuctFieldDto_Fail()
        {
            SchemaRow row = new SchemaRow();

            SetSchemaRow(row);

            StructFieldDto dto = new StructFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(1, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) STRUCTs are required to have children", result.Description);
        }

        [TestMethod]
        public void Validate_DecimalFieldDto_Success()
        {
            SchemaRow row = new SchemaRow()
            {
                Precision = "6",
                Scale = "2"
            };

            SetSchemaRow(row);

            DecimalFieldDto dto = new DecimalFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        //[TestMethod]
        //public void Validate_DecimalFieldDto_FIXEDWIDTH_Success()
        //{
        //    SchemaRow row = new SchemaRow()
        //    {
        //        Precision = "6"
        //    };

        //    SetSchemaRow(row);

        //    DecimalFieldDto dto = new DecimalFieldDto(row);

        //    ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

        //    Assert.IsTrue(results.IsValid());
        //}

        [TestMethod]
        public void Validate_DecimalFieldDto_Fail()
        {
            SchemaRow row = new SchemaRow()
            {
                Precision = "0",
                Scale = "40"
            };

            SetSchemaRow(row);

            DecimalFieldDto dto = new DecimalFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(3, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Precision (0) is required to be between 1 and 38", result.Description);

            result = resultList[1];
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Scale (40) is required to be between 0 and 38", result.Description);

            result = resultList.Last();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Scale (40) needs to be less than or equal to Precision (0)", result.Description);
        }

        //[TestMethod]
        //public void Validate_DecimalFieldDto_FIXEDWIDTH_Fail()
        //{
        //    SchemaRow row = new SchemaRow()
        //    {
        //        Length = "5",
        //        Precision = "6",
        //        Scale = "2"
        //    };

        //    SetSchemaRow(row);

        //    DecimalFieldDto dto = new DecimalFieldDto(row);

        //    ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

        //    Assert.IsFalse(results.IsValid());
        //    Assert.AreEqual(1, results.GetAll().Count);

        //    List<ValidationResult> resultList = results.GetAll();

        //    ValidationResult result = resultList.First();
        //    Assert.AreEqual("1", result.Id);
        //    Assert.AreEqual("(TestField) Length (5) needs to be equal or greater than specified precision (6) for FIXEDWIDTH schema", result.Description);
        //}

        [TestMethod]
        public void Validate_DateFieldDto_Success()
        {
            SchemaRow row = new SchemaRow() { Format = GlobalConstants.Datatypes.Defaults.DATE_DEFAULT };
            SetSchemaRow(row);

            DateFieldDto dto = new DateFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        //[TestMethod]
        //public void Validate_DateFieldDto_FIXEDWIDTH_Success()
        //{
        //    SchemaRow row = new SchemaRow() { Format = GlobalConstants.Datatypes.Defaults.TIMESTAMP_DEFAULT };
        //    SetSchemaRow(row);

        //    DateFieldDto dto = new DateFieldDto(row);

        //    ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

        //    Assert.IsTrue(results.IsValid());
        //}

        [TestMethod]
        public void Validate_DateFieldDto_FIXEDWIDTH_SourceFormat_Fail()
        {
            SchemaRow row = new SchemaRow() { Format = GlobalConstants.Datatypes.Defaults.DATE_DEFAULT };
            SetSchemaRow(row);

            DateFieldDto dto = new DateFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(2, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be greater than zero for FIXEDWIDTH schema", result.Description);

            result = resultList.Last();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be equal or greater than specified format for FIXEDWIDTH schema", result.Description);
        }

        [TestMethod]
        public void Validate_DateFieldDto_FIXEDWIDTH_NoSourceFormat_Fail()
        {
            SchemaRow row = new SchemaRow();
            SetSchemaRow(row);

            DateFieldDto dto = new DateFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(2, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be greater than zero for FIXEDWIDTH schema", result.Description);

            result = resultList.Last();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("(TestField) Length (0) needs to be equal or greater than default format length (10) for FIXEDWIDTH schema", result.Description);
        }

        [TestMethod]
        public void Validate_IntegerFieldDto_Success()
        {
            SchemaRow row = new SchemaRow();

            SetSchemaRow(row);

            IntegerFieldDto dto = new IntegerFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        //[TestMethod]
        //public void Validate_IntegerFieldDto_FIXEDWIDTH_Success()
        //{
        //    SchemaRow row = new SchemaRow()
        //    {
        //        Length = "2"
        //    };

        //    SetSchemaRow(row);

        //    IntegerFieldDto dto = new IntegerFieldDto(row);

        //    ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

        //    Assert.IsTrue(results.IsValid());
        //}

        [TestMethod]
        public void Validate_IntegerFieldDto_Fail()
        {
            SchemaRow row = new SchemaRow();

            SetSchemaRow(row);
            row.Name = "123 Test";

            IntegerFieldDto dto = new IntegerFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(2, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Field name (123 Test) must start with a letter or underscore", result.Description);

            result = resultList[1];
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Field name (123 Test) can only contain letters, underscores, digits (0-9), and dollar signs (\"$\")", result.Description);
        }

        [TestMethod]
        public void Validate_BigIntFieldDto_Success()
        {
            SchemaRow row = new SchemaRow();

            SetSchemaRow(row);

            BigIntFieldDto dto = new BigIntFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsTrue(results.IsValid());
        }

        //[TestMethod]
        //public void Validate_BigIntFieldDto_FIXEDWIDTH_Success()
        //{
        //    SchemaRow row = new SchemaRow()
        //    {
        //        Length = "2"
        //    };

        //    SetSchemaRow(row);

        //    BigIntFieldDto dto = new BigIntFieldDto(row);

        //    ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.FIXEDWIDTH);

        //    Assert.IsTrue(results.IsValid());
        //}

        [TestMethod]
        public void Validate_BigIntFieldDto_Fail()
        {
            SchemaRow row = new SchemaRow();

            SetSchemaRow(row);
            row.Name = "123 Test";

            BigIntFieldDto dto = new BigIntFieldDto(row);

            ValidationResults results = dto.Validate(GlobalConstants.ExtensionNames.CSV);

            Assert.IsFalse(results.IsValid());
            Assert.AreEqual(2, results.GetAll().Count);

            List<ValidationResult> resultList = results.GetAll();

            ValidationResult result = resultList.First();
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Field name (123 Test) must start with a letter or underscore", result.Description);

            result = resultList[1];
            Assert.AreEqual("1", result.Id);
            Assert.AreEqual("Field name (123 Test) can only contain letters, underscores, digits (0-9), and dollar signs (\"$\")", result.Description);
        }

        #region Methods
        private void SetSchemaRow(SchemaRow schemaRow)
        {
            schemaRow.Name = "TestField";
            schemaRow.Position = 1;
        }
        #endregion
    }
}
