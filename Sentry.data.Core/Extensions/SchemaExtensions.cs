using NJsonSchema;
using Sentry.Common.Logging;
using Sentry.data.Core.Exceptions;
using Sentry.data.Core.Factories.Fields;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sentry.data.Core
{
    public static class SchemaExtensions
    {
        public static SchemaRevisionDto ToDto(this SchemaRevision revision)
        {
            return new SchemaRevisionDto()
            {
                RevisionId = revision.SchemaRevision_Id,
                RevisionNumber = revision.Revision_NBR,
                SchemaRevisionName = revision.SchemaRevision_Name,
                CreatedBy = revision.CreatedBy,
                CreatedDTM = revision.CreatedDTM,
                LastUpdatedDTM = revision.LastUpdatedDTM,
                JsonSchemaObject = revision.JsonSchemaObject
            };
        }

        public static BaseFieldDto ToDto(this BaseField field)
        {
            FieldDtoFactory factory;
            BaseFieldDto dto = null;

            switch (field)
            {                 
                case DecimalField dm:
                    factory = new DecimalFieldDtoFactory(dm);
                    break;
                case DateField dt:
                    factory = new DateFieldDtoFactory(dt);
                    break;
                case TimestampField tm:
                    factory = new TimestampFieldDtoFactory(tm);
                    break;
                case VarcharField v:
                    factory = new VarcharFieldDtoFactory(v);
                    break;
                case IntegerField i:
                    factory = new IntegerFieldDtoFactory(i);
                    break;
                case StructField s:
                    factory = new StructFieldDtoFactory(s);
                    break;
                case BigIntField bi:
                    factory = new BigIntFieldDtoFactory(bi);
                    break;
                default:
                    factory = null;
                    break;
            }

            if (factory != null)
            {
                dto = factory.GetField();

                List<BaseFieldDto> childList = new List<BaseFieldDto>();
                foreach (BaseField childField in field.ChildFields)
                {
                    childList.Add(childField.ToDto());
                }
                dto.ChildFields = childList;
            }

            return dto;
        }

        public static List<BaseFieldDto> ToDto(this List<BaseField> fields)
        {
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            foreach (BaseField field in fields)
            {
                dtoList.Add(field.ToDto());
            }
            return dtoList;
        }

        public static DataElementDto ToDataElementDto(this DatasetSchemaDto dsDto)
        {
            return new DataElementDto()
            {
                DataElementName = dsDto.ConfigFileName,
                SchemaName = dsDto.ConfigFileName,
                SchemaDescription = dsDto.ConfigFileDesc,
                Delimiter = dsDto.Delimiter,
                HasHeader = dsDto.HasHeader,
                CreateCurrentView = true,
                FileExtensionId = dsDto.FileExtensionId,
                ParentDatasetId = dsDto.DatasetId
            };
        }
        public static Object TryConvertTo<T>(Object input)
        {
            Object result = null;
            try
            {
                result = Convert.ChangeType(input, typeof(T));
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        /// <summary>
        /// Will return JsonSchema associated with array JsonSchemaProperty.  Null will be return if not found
        /// </summary>
        /// <param name="prop"></param>
        /// <exception cref="ArgumentException">If array does not contain Items or Definition reference</exception>
        /// <returns></returns>
        public static JsonSchema FindArraySchema(this KeyValuePair<string, JsonSchemaProperty> prop)
        {
            JsonSchemaProperty currentProp = prop.Value;
            JsonSchema outSchema = null;

            //if items does not exist or there is not a reference, pass back a null
            if (currentProp.Items.Count == 0 && currentProp.Item == null && !currentProp.HasReference)
            {
                Logger.Warn($"Array ({prop.Key}) not defined properly - did not contain items or reference");
                return outSchema;
            }
            //if jsonschemaproperty has reference, pass back reference
            else if (currentProp.HasReference)
            {
                outSchema = currentProp.Reference;
            }
            //if Item is populated, pass back Item jsonschema
            else if (currentProp.Items.Count == 0 && currentProp.Item != null)
            {
                outSchema = currentProp.Item;
            }
            //DSC only supports single type arrays
            //Pass back first jsonschema in Items and log warning
            else
            {
                if (currentProp.Items.Count > 1)
                {
                    Logger.Warn($"Schema contains multiple items within array ({prop.Key}) - taking first Item");
                }
                outSchema = currentProp.Items.First();
            }

            if (outSchema.HasReference)
            {
                outSchema = outSchema.Reference;
            }

            return outSchema;

        }
        public static void ToDto(this JsonSchema schema, IList<BaseFieldDto> dtoList, ref int rowPosition, BaseFieldDto parentRow = null)
        {
            try
            {
                switch (schema.Type)
                {
                    case JsonObjectType.Object:
                        foreach (KeyValuePair<string, JsonSchemaProperty> prop in schema.Properties.ToList())
                        {
                            prop.ToDto(dtoList, ref rowPosition, parentRow);
                        }
                        break;
                    case JsonObjectType.None:
                        if (schema.HasReference)
                        {
                            schema.Reference.ToDto(dtoList, ref rowPosition, parentRow);
                        }
                        else
                        {
                            if (parentRow == null)
                            {
                                Logger.Warn($"Unhandled Scenario:::jsonobjecttype:{JsonObjectType.None.ToString()}:::parentrownull");
                                throw new SchemaConversionException("Unhandled Schema Scenario");
                            }
                            else
                            {
                                parentRow.Description = "MOCKED OUT";
                            }
                        }
                        break;
                    default:
                        Logger.Warn($"Unhandled Scenario for schema object type of {schema.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ToSchemaRows Error", ex);
                throw;
            }
        }

        public static void ToDto(this KeyValuePair<string, JsonSchemaProperty> prop, IList<BaseFieldDto> dtoList, ref int rowPosition, BaseFieldDto parentRow = null)
        {
            try
            {
                FieldDtoFactory fieldFactory = null;

                JsonSchemaProperty currentProperty = prop.Value;
                Logger.Debug($"Found property:{prop.Key}");

                switch (currentProperty.Type)
                {
                    case JsonObjectType.None:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        if (currentProperty.HasReference)
                        {
                            Logger.Debug($"Detected ref object: property will be defined as STRUCT");
                            fieldFactory = BuildStructFactory(prop, JsonObjectType.Object, ++rowPosition);
                            BaseFieldDto noneStructField = fieldFactory.GetField();

                            AddToFieldList(dtoList, parentRow, noneStructField);                            

                            currentProperty.Reference.ToDto(dtoList.ToList(), ref rowPosition, noneStructField);
                            
                        }
                        else
                        {
                            Logger.Warn($"No ref object detected");
                            Logger.Warn($"{prop.Key} will be defined as STRUCT");
                            fieldFactory = new VarcharFieldDtoFactory(prop, ++rowPosition, false);                            

                            if (parentRow == null)
                            {
                                dtoList.Add(fieldFactory.GetField());
                            }
                            else
                            {
                                parentRow.ChildFields.Add(fieldFactory.GetField());
                            }
                        }
                        break;
                    case JsonObjectType.Object:
                    case JsonObjectType.Null | JsonObjectType.Object:
                        fieldFactory = BuildStructFactory(prop, currentProperty.Type, ++rowPosition);

                        BaseFieldDto objectStructfield = fieldFactory.GetField();

                        AddToFieldList(dtoList, parentRow, objectStructfield);

                        foreach (KeyValuePair<string, JsonSchemaProperty> nestedProp in currentProperty.Properties)
                        {
                            nestedProp.ToDto(dtoList, ref rowPosition, objectStructfield);
                        }
                        break;
                    case JsonObjectType.Array:
                    case JsonObjectType.Null | JsonObjectType.Array:
                        Logger.Debug($"Detected type of {currentProperty.Type}");

                        JsonSchema nestedSchema = null;
                        //While JSON Schema alows an arrays of multiple types, DSC only allows single type.

                        nestedSchema = prop.FindArraySchema();

                        //Determine what this is an array of
                        if (nestedSchema == null)
                        {
                            Logger.Warn($"Schema could not be detected for {prop.Key} property");

                            fieldFactory = BuildStringFactory(prop, JsonObjectType.String, null, ++rowPosition, true);
                        }
                        else if (nestedSchema.IsObject)
                        {
                            Logger.Debug($"Detected nested schema as Object");

                            fieldFactory = BuildStructFactory(prop, JsonObjectType.Object, ++rowPosition, true);
                        }
                        else
                        {
                            switch (nestedSchema.Type)
                            {
                                case JsonObjectType.Object:
                                case JsonObjectType.Null | JsonObjectType.Object:
                                    fieldFactory = BuildStructFactory(prop, nestedSchema.Type, ++rowPosition, true);
                                    break;
                                case JsonObjectType.Integer:
                                case JsonObjectType.Null | JsonObjectType.Integer:
                                    fieldFactory = BuildIntegerFactory(prop, nestedSchema.Type, nestedSchema.Format, ++rowPosition, true);
                                    break;
                                case JsonObjectType.String:
                                case JsonObjectType.Null | JsonObjectType.String:                                    
                                    fieldFactory = BuildStringFactory(prop, nestedSchema.Type, nestedSchema.Format, ++rowPosition, true);
                                    break;
                                case JsonObjectType.Number:
                                case JsonObjectType.Null | JsonObjectType.Number:
                                    fieldFactory = BuildDecimalFactory(prop, nestedSchema.Type, ++rowPosition, true);
                                    break;
                                case JsonObjectType.None:
                                    if (nestedSchema.IsAnyType)
                                    {
                                        Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} and marked as IsAnyType");
                                        fieldFactory = BuildStringFactory(prop, JsonObjectType.String, null, ++rowPosition, true);
                                    }
                                    else
                                    {
                                        Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()}");
                                        fieldFactory = BuildStringFactory(prop, JsonObjectType.String, null, ++rowPosition, true);
                                    }
                                    break;
                                default:
                                    Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
                                    fieldFactory = BuildStringFactory(prop, JsonObjectType.String, null, ++rowPosition, true);
                                    break;
                            }
                        }

                        BaseFieldDto field = fieldFactory.GetField();

                        if (nestedSchema != null)
                        {
                            nestedSchema.ToDto(dtoList.ToList(), ref rowPosition, field);
                        }

                        AddToFieldList(dtoList, parentRow, field);
                        
                        break;
                    case JsonObjectType.String:
                    case JsonObjectType.Null | JsonObjectType.String:
                        fieldFactory = BuildStringFactory(prop, currentProperty.Type, currentProperty.Format, ++rowPosition);

                        AddToFieldList(dtoList, parentRow, fieldFactory.GetField());

                        break;
                    case JsonObjectType.Integer:
                    case JsonObjectType.Null | JsonObjectType.Integer:
                        fieldFactory = BuildIntegerFactory(prop, currentProperty.Type, currentProperty.Format, ++rowPosition);

                        AddToFieldList(dtoList, parentRow, fieldFactory.GetField());

                        break;
                    case JsonObjectType.Number:
                    case JsonObjectType.Null | JsonObjectType.Number:
                        fieldFactory = BuildDecimalFactory(prop, currentProperty.Type, ++rowPosition);

                        AddToFieldList(dtoList, parentRow, fieldFactory.GetField());

                        break;
                    default:
                        Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");

                        fieldFactory = BuildStringFactory(prop, JsonObjectType.String, null, ++rowPosition);

                        AddToFieldList(dtoList, parentRow, fieldFactory.GetField());

                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ToSchemaRow Error", ex);
                throw;
            }
        }

        public static bool AreEqualTo(this List<BaseFieldDto> existingFields, List<BaseFieldDto> newFields)
        {
            if (existingFields.Count == newFields.Count)
            {
                for (int i = 0; i < existingFields.Count; i++)
                {
                    if (!existingFields[i].IsEqualTo(newFields[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static bool IsEqualTo(this BaseFieldDto existingField, BaseFieldDto newField)
        {
            return existingField.FieldType == newField.FieldType &&
                existingField.Name == newField.Name &&
                existingField.IsArray == newField.IsArray &&
                existingField.Scale == newField.Scale &&
                existingField.Precision == newField.Precision &&
                existingField.Length == newField.Length &&
                existingField.ChildFields.AreEqualTo(newField.ChildFields);
        }

        private static FieldDtoFactory BuildDecimalFactory(KeyValuePair<string, JsonSchemaProperty> prop, JsonObjectType objectType, int rowPosition, bool isArray = false)
        {
            FieldDtoFactory fieldFactory;
            Logger.Debug($"Detected type of {objectType}");
            Logger.Debug($"{prop.Key} will be defined as DECIMAL");
            fieldFactory = new DecimalFieldDtoFactory(prop, rowPosition, isArray);
            return fieldFactory;
        }

        private static FieldDtoFactory BuildIntegerFactory(KeyValuePair<string, JsonSchemaProperty> prop, JsonObjectType objectType, string format, int rowPosition, bool isArray = false)
        {
            FieldDtoFactory fieldFactory;
            Logger.Debug($"Detected type of {objectType}");

            //
            if (!String.IsNullOrWhiteSpace(format) && format == "biginteger")
            {
                Logger.Debug($"{prop.Key} will be defined as BIGINT");
                fieldFactory = new BigIntFieldDtoFactory(prop, rowPosition, isArray);
                return fieldFactory;
            }

            Logger.Debug($"{prop.Key} will be defined as INTEGER");
            fieldFactory = new IntegerFieldDtoFactory(prop, rowPosition, isArray);
            return fieldFactory;
            
        }

        private static FieldDtoFactory BuildStringFactory(KeyValuePair<string, JsonSchemaProperty> prop, JsonObjectType objectType, string format, int rowPosition, bool isArray = false)
        {
            FieldDtoFactory fieldFactory;
            Logger.Debug($"Detected type of {objectType}");

            if (!String.IsNullOrWhiteSpace(format))
            {
                switch (format)
                {
                    case "date-time":
                        Logger.Debug($"Detected string format of {format}");
                        Logger.Debug($"{prop.Key} will be defined as TIMESTAMP");
                        fieldFactory = new TimestampFieldDtoFactory(prop, rowPosition, isArray);
                        break;
                    case "date":
                        Logger.Debug($"Detected string format of {format}");
                        Logger.Debug($"{prop.Key} will be defined as DATE");
                        fieldFactory = new DateFieldDtoFactory(prop, rowPosition, isArray);
                        break;
                    default:
                        Logger.Warn($"Detected string format of {format} which is not handled by DSC");
                        Logger.Warn($"{prop.Key} will be defined as DATE");
                        fieldFactory = new VarcharFieldDtoFactory(prop, rowPosition, isArray);
                        break;
                }
            }
            else
            {
                Logger.Debug($"No string format detected");
                Logger.Debug($"{prop.Key} will be defined as VARCHAR");
                fieldFactory = new VarcharFieldDtoFactory(prop, rowPosition, isArray);
            }

            return fieldFactory;
        }

        private static FieldDtoFactory BuildStructFactory(KeyValuePair<string, JsonSchemaProperty> prop, JsonObjectType objecType, int rowPosition, bool isArray = false)
        {
            FieldDtoFactory fieldFactory;
            Logger.Debug($"Detected type of {objecType}");
            Logger.Debug($"Detected ref object: property will be defined as STRUCT");
            fieldFactory = new StructFieldDtoFactory(prop, rowPosition, isArray);

            return fieldFactory;
        }

        private static void AddToFieldList(IList<BaseFieldDto> dtoList, BaseFieldDto parentRow, BaseFieldDto objectStructfield)
        {
            if (parentRow == null)
            {
                dtoList.Add(objectStructfield);
            }
            else
            {
                parentRow.ChildFields.Add(objectStructfield);
                parentRow.HasChildren = true;
            }
        }        
    }
}
