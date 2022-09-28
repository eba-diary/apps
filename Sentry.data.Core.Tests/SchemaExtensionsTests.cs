using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sentry.data.Core.DTO.Schema.Fields;
using System;
using System.Collections.Generic;

namespace Sentry.data.Core.Tests
{
    [TestClass]
    public class SchemaExtensionsTests
    {
        [TestMethod]
        public void AreEqualTo_OneDifferentField_False()
        {
            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar2", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            Assert.IsFalse(fields1.AreEqualTo(fields2));
        }

        [TestMethod]
        public void AreEqualTo_AllDifferentFields_False()
        {
            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<DateFieldDto>("Date", true, 1),
                GetField<VarcharFieldDto>("Varchar2", false, 2),
                GetField<DecimalFieldDto>("Decimal", false, 3)
            };

            Assert.IsFalse(fields1.AreEqualTo(fields2));
        }

        [TestMethod]
        public void AreEqualTo_DifferentOrder_False()
        {
            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<VarcharFieldDto>("Varchar", true, 1),
                GetField<TimestampFieldDto>("Timestamp", false, 2),
                GetField<IntegerFieldDto>("Integer", false, 3)
            };

            Assert.IsFalse(fields1.AreEqualTo(fields2));
        }

        [TestMethod]
        public void AreEqualTo_DifferentCount_False()
        {
            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2)
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            Assert.IsFalse(fields1.AreEqualTo(fields2));
        }

        [TestMethod]
        public void AreEqualTo_AllSameFields_True()
        {
            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3)
            };

            Assert.IsTrue(fields1.AreEqualTo(fields2));
        }

        [TestMethod]
        public void AreEqualTo_ComplexFields_True()
        {
            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3),
                GetStructField()
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3),
                GetStructField()
            };

            Assert.IsTrue(fields1.AreEqualTo(fields2));
        }

        [TestMethod]
        public void AreEqualTo_ComplexFields_ChildDifferent_False()
        {
            StructFieldDto childStructField = GetField<StructFieldDto>("child", false, 7);
            childStructField.ChildFields = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("ChildInteger", false, 8),
                GetField<VarcharFieldDto>("ChildVarchar", false, 9),
            };

            StructFieldDto structField = GetField<StructFieldDto>("root", true, 4);
            structField.ChildFields = new List<BaseFieldDto>()
            {
                GetField<TimestampFieldDto>("RootTimestamp", false, 5),
                GetField<DecimalFieldDto>("Decimal", false, 6),
                childStructField
            };

            List<BaseFieldDto> fields1 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3),
                structField
            };

            List<BaseFieldDto> fields2 = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("Integer", false, 1),
                GetField<VarcharFieldDto>("Varchar", true, 2),
                GetField<TimestampFieldDto>("Timestamp", false, 3),
                GetStructField()
            };

            Assert.IsFalse(fields1.AreEqualTo(fields2));
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

        private StructFieldDto GetStructField()
        {
            StructFieldDto childStructField = GetField<StructFieldDto>("child", false, 7);
            childStructField.ChildFields = new List<BaseFieldDto>()
            {
                GetField<IntegerFieldDto>("ChildInteger", false, 8),
                GetField<VarcharFieldDto>("ChildVarchar", false, 9),
            };

            StructFieldDto structField = GetField<StructFieldDto>("root", true, 4);
            structField.ChildFields = new List<BaseFieldDto>()
            {
                GetField<DateFieldDto>("Date", false, 5),
                GetField<DecimalFieldDto>("Decimal", false, 6),
                childStructField
            };

            return structField;
        }
        #endregion
    }
}
