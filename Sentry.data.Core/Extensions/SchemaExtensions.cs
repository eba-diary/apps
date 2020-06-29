﻿using NJsonSchema;
using Sentry.Common.Logging;
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

        public static DataElementDto ToDataElementDto(this DatasetDto dsDto)
        {
            return new DataElementDto()
            {
                DataElementName = dsDto.ConfigFileName,
                SchemaName = dsDto.ConfigFileName,
                SchemaDescription = dsDto.ConfigFileDesc,
                Delimiter = dsDto.Delimiter,
                HasHeader = dsDto.HasHeader,
                CreateCurrentView = true,
                IsInSAS = dsDto.IsInSAS,
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

        public static void ToDto(this KeyValuePair<string, JsonSchemaProperty> prop, IList<BaseFieldDto> dtoList, BaseFieldDto parentRow = null)
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
                            fieldFactory = new StructFieldDtoFactory(prop, false);
                            BaseFieldDto noneStructField = fieldFactory.GetField();

                            if (parentRow == null)
                            {
                                dtoList.Add(noneStructField);
                            }
                            else
                            {
                                parentRow.ChildFields.Add(noneStructField);
                            }

                            currentProperty.Reference.ToDto(dtoList.ToList(), noneStructField);
                            
                        }
                        else
                        {
                            Logger.Warn($"No ref object detected");
                            Logger.Warn($"{prop.Key} will be defined as STRUCT");
                            fieldFactory = new VarcharFieldDtoFactory(prop, false);

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
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        Logger.Debug($"Detected ref object: property will be defined as STRUCT");
                        fieldFactory = new StructFieldDtoFactory(prop, false);
                        BaseFieldDto objectStructfield = fieldFactory.GetField();

                        if (parentRow == null)
                        {
                            dtoList.Add(objectStructfield);
                        }
                        else
                        {
                            parentRow.ChildFields.Add(objectStructfield);
                        }

                        foreach (KeyValuePair<string, JsonSchemaProperty> nestedProp in currentProperty.Properties)
                        {
                            nestedProp.ToDto(dtoList, objectStructfield);
                        }

                        break;
                    case JsonObjectType.Array:
                        Logger.Debug($"Detected type of {currentProperty.Type}");

                        JsonSchema nestedSchema = null;
                        //While JSON Schema alows an arrays of multiple types, DSC only allows single type.

                        nestedSchema = prop.FindArraySchema();

                        //Determine what this is an array of
                        if (nestedSchema == null)
                        {
                            Logger.Warn($"Schema could not be detected for {prop.Key} property");
                            Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
                            fieldFactory = new VarcharFieldDtoFactory(prop, true);
                        }
                        else if (nestedSchema.IsObject)
                        {
                            Logger.Debug($"Detected nested schema as Object");
                            Logger.Debug($"{prop.Key} will be defined as array of STRUCT");
                            fieldFactory = new StructFieldDtoFactory(prop, true);
                        }
                        else
                        {
                            switch (nestedSchema.Type)
                            {
                                case JsonObjectType.Object:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    Logger.Debug($"{prop.Key} will be defined as array of STRUCT");
                                    fieldFactory = new StructFieldDtoFactory(prop, true);
                                    break;
                                case JsonObjectType.Integer:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    Logger.Debug($"{prop.Key} will be defined as array of INTEGER");
                                    fieldFactory = new IntegerFieldDtoFactory(prop, true);
                                    break;
                                case JsonObjectType.String:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    switch (nestedSchema.Format)
                                    {
                                        case "date-time":
                                            Logger.Debug($"Detected string format of {nestedSchema.Format}");
                                            Logger.Debug($"{prop.Key} will be defined as array of TIMESTAMP");
                                            fieldFactory = new TimestampFieldDtoFactory(prop, true);
                                            break;
                                        case "date":
                                            Logger.Debug($"Detected string format of {nestedSchema.Format}");
                                            Logger.Debug($"{prop.Key} will be defined as array of DATE");
                                            fieldFactory = new DateFieldDtoFactory(prop, true);
                                            break;
                                        default:
                                            Logger.Debug($"No string format detected");
                                            Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
                                            fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                            break;
                                    }
                                    break;
                                case JsonObjectType.Number:
                                    Logger.Debug($"Detected nested schema as {nestedSchema.Type}");
                                    Logger.Debug($"{prop.Key} will be defined as array of DECIMAL");
                                    fieldFactory = new DecimalFieldDtoFactory(prop, true);
                                    break;
                                case JsonObjectType.None:
                                    if (nestedSchema.IsAnyType)
                                    {
                                        Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} and marked as IsAnyType");
                                        Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
                                        fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                    }
                                    else
                                    {
                                        Logger.Debug($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()}");
                                        Logger.Debug($"{prop.Key} will be defined as array of VARCHAR");
                                        fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                    }
                                    break;
                                default:
                                    Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
                                    Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
                                    fieldFactory = new VarcharFieldDtoFactory(prop, true);
                                    break;
                            }
                        }

                        BaseFieldDto field = fieldFactory.GetField();

                        if (nestedSchema != null)
                        {
                            nestedSchema.ToDto(dtoList.ToList(), field);
                        }

                        if (parentRow == null)
                        {
                            dtoList.Add(field);
                        }
                        else
                        {
                            parentRow.ChildFields.Add(field);
                        }
                        break;
                    case JsonObjectType.String:
                        Logger.Debug($"Detected type of {currentProperty.Type}");

                        if (!String.IsNullOrWhiteSpace(currentProperty.Format))
                        {
                            switch (currentProperty.Format)
                            {
                                case "date-time":
                                    Logger.Debug($"Detected string format of {currentProperty.Format}");
                                    Logger.Debug($"{prop.Key} will be defined as TIMESTAMP");
                                    fieldFactory = new TimestampFieldDtoFactory(prop, false);
                                    break;
                                case "date":
                                    Logger.Debug($"Detected string format of {currentProperty.Format}");
                                    Logger.Debug($"{prop.Key} will be defined as DATE");
                                    fieldFactory = new DateFieldDtoFactory(prop, false);
                                    break;
                                default:
                                    Logger.Warn($"Detected string format of {currentProperty.Format} which is not handled by DSC");
                                    Logger.Warn($"{prop.Key} will be defined as DATE");
                                    fieldFactory = new VarcharFieldDtoFactory(prop, false);
                                    break;
                            }
                        }
                        else
                        {
                            Logger.Debug($"No string format detected");
                            Logger.Debug($"{prop.Key} will be defined as VARCHAR");
                            fieldFactory = new VarcharFieldDtoFactory(prop, false);
                        }

                        if (parentRow == null)
                        {
                            dtoList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                    case JsonObjectType.Integer:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        Logger.Debug($"{prop.Key} will be defined as INTEGER");

                        fieldFactory = new IntegerFieldDtoFactory(prop, false);

                        if (parentRow == null)
                        {
                            dtoList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                    case JsonObjectType.Number:
                        Logger.Debug($"Detected type of {currentProperty.Type}");
                        Logger.Debug($"{prop.Key} will be defined as DECIMAL");
                        fieldFactory = new DecimalFieldDtoFactory(prop, false);

                        if (parentRow == null)
                        {
                            dtoList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                    default:
                        Logger.Warn($"The {prop.Key} property is defined as {JsonObjectType.None.ToString()} which is not handled by DSC");
                        Logger.Warn($"{prop.Key} will be defined as array of VARCHAR");
                        fieldFactory = new VarcharFieldDtoFactory(prop, true);

                        if (parentRow == null)
                        {
                            dtoList.Add(fieldFactory.GetField());
                        }
                        else
                        {
                            parentRow.ChildFields.Add(fieldFactory.GetField());
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ToSchemaRow Error", ex);
                throw;
            }
        }

        public static void ToDto(this JsonSchema schema, List<BaseFieldDto> schemaRowList, BaseFieldDto parentSchemaRow = null)
        {
            try
            {
                switch (schema.Type)
                {
                    case JsonObjectType.Object:
                        foreach (KeyValuePair<string, JsonSchemaProperty> prop in schema.Properties.ToList())
                        {
                            prop.ToDto(schemaRowList, parentSchemaRow);
                        }
                        break;
                    case JsonObjectType.None:
                        if (schema.HasReference)
                        {
                            schema.Reference.ToDto(schemaRowList, parentSchemaRow);
                        }
                        else
                        {
                            if (parentSchemaRow == null)
                            {
                                Logger.Warn("Unhandled Scenario");
                            }
                            else
                            {
                                parentSchemaRow.Description = "MOCKED OUT";
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
    }
}
