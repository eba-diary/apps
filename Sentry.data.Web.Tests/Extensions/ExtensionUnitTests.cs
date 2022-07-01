using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using Sentry.data.Core;
using Sentry.data.Core.DTO.Schema.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentry.data.Web.Tests
{
    [TestClass]
    public class ExtensionUnitTests
    {
        [TestMethod, TestCategory("ToDto JsonSchema")]
        public void Can_Find_Object_Reference()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchema_ObjectReference_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.ToDto(dtoList, ref position);

            //Assertion
            Assert.AreEqual(1, dtoList.Count);
            Assert.AreEqual("person", dtoList.First().Name);
            Assert.IsInstanceOfType(dtoList.First(), typeof(StructFieldDto));
        }

        [TestMethod, TestCategory("ToDto JsonSchema")]
        public void Can_Boolean_Within_Object_Reference()
        {
            //SETUP
            JsonSchema schema = BuildMockJsonSchema_ObjectReference_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;
            //ACTION
            schema.ToDto(dtoList, ref position);

            //ASSERT
            Assert.AreEqual(true, dtoList.First().ChildFields.Any(w => w.Name == "haschildren"));
            Assert.IsInstanceOfType(dtoList.First().ChildFields.First(w => w.Name == "haschildren"), typeof(VarcharFieldDto));
        }
        [TestMethod, TestCategory("ToDto JsonSchema")]
        public void Can_Find_Object_Property()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.ToDto(dtoList, ref position);

            //Assertion
            Assert.AreEqual(1, dtoList.Count);
            Assert.AreEqual("person", dtoList.First().Name);
            Assert.IsInstanceOfType(dtoList.First(), typeof(StructFieldDto));
        }
        [TestMethod, TestCategory("ToDto JsonSchema")]
        public void Can_Find_Object_Property_Children()
        {
            //Setup

            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.ToDto(dtoList, ref position);
            List<BaseFieldDto> childrenList = dtoList.First().ChildFields;

            //Assertion
            Assert.AreEqual(4, childrenList.Count);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_String()
        {
            //Setup
            //string jsonSchema = @"{
            //  ""type"": ""object"",
            //  ""properties"": {
            //    ""person"": {
            //      ""type"": ""object"",
            //      ""properties"": {
            //        ""name"": {
            //          ""type"": ""string""
            //        },
            //        ""age"": {
            //          ""type"": ""integer""
            //        }
            //      }
            //    }
            //  }
            //}";

            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "name").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(VarcharFieldDto));
        }

        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_Integer()
        {
            //Setup
            //string jsonSchema = @"{
            //  ""type"": ""object"",
            //  ""properties"": {
            //    ""person"": {
            //      ""type"": ""object"",
            //      ""properties"": {
            //        ""name"": {
            //          ""type"": ""string""
            //        },
            //        ""age"": {
            //          ""type"": ""integer""
            //        }
            //      }
            //    }
            //  }
            //}";

            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "age").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(IntegerFieldDto));
        }

        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_Object()
        {
            //Setup            
            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(StructFieldDto));
        }

        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_ArrayofVarchar()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "top3favoritecolors").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(VarcharFieldDto));
            Assert.AreEqual(true, dtoList.First().IsArray);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_ArrayofInteger()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                      ""type"": ""string""
                    },
                    ""age"": {
                      ""type"": ""integer""
                    },
                    ""favoritenumbers"": {
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""integer""
                        },
                        ""description"": ""Array of Integers""
                    }
                  }
                }
              }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "favoritenumbers").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(IntegerFieldDto));
            Assert.AreEqual(true, dtoList.First().IsArray);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_ArrayofDates()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                      ""type"": ""string""
                    },
                    ""age"": {
                      ""type"": ""integer""
                    },
                    ""pastoccurances"": {
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date""
                        },
                        ""description"": ""Array of Dates""
                    }
                  }
                }
              }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "pastoccurances").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(DateFieldDto));
            Assert.AreEqual(true, dtoList.First().IsArray);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_ArrayofTimestamps()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                      ""type"": ""string""
                    },
                    ""age"": {
                      ""type"": ""integer""
                    },
                    ""pastoccurances"": {
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""description"": ""Array of Timestamps""
                    }
                  }
                }
              }
            }";
            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "pastoccurances").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First(), typeof(TimestampFieldDto));
            Assert.AreEqual(true, dtoList.First().IsArray);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Get_Properties_For_ArrayofVarchar()
        {
            //Setup
            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").Value.Properties.First(p => p.Key == "top3favoritecolors").ToDto(dtoList, ref position);
            BaseFieldDto dtoField = dtoList.First();

            //Assertion
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoField, typeof(VarcharFieldDto));
            Assert.AreEqual(true, dtoField.IsArray);
            Assert.AreEqual("top3favoritecolors", dtoField.Name);
            Assert.AreEqual("Array of VARCHAR", dtoField.Description);
            Assert.AreEqual(391, dtoField.Length);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Can_Detect_ArrayofReference()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""$ref"": ""#/definitions/Person""
                }
              },
              ""definitions"": {
                    ""Person"": {
                      ""type"": ""object"",
                      ""properties"": {
                            ""name"": {
                                ""type"": ""string""
                            },
                            ""age"": {
                                ""type"": ""integer""
                            },
                            ""children"": {
                                ""type"": ""array"",
                                ""items"": { ""$ref"": ""#/definitions/Child"" }
                            }
                        }
                    },
                    ""top3favoritenumbers"": {
                        ""type"": ""string""
                    },
                    ""Child"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": {
                                ""type"": ""string""
                            }
                        }
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Definitions.First(w => w.Key == "Person").Value.Properties.First(p => p.Key == "children").ToDto(dtoList, ref position);
            BaseFieldDto dtoField = dtoList.First();

            //Assertion
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoField, typeof(StructFieldDto));
            Assert.AreEqual(true, dtoField.IsArray);
            Assert.AreEqual("children", dtoField.Name);

            List<BaseFieldDto> childFields = dtoList.First(w => w.Name == "children").ChildFields;

            Assert.AreEqual(1, childFields.Count);
            Assert.AreEqual("name", childFields.First().Name);
            Assert.IsInstanceOfType(childFields.First(), typeof(VarcharFieldDto));
            Assert.AreEqual(1000, childFields.First().Length);
            Assert.IsNull(childFields.First().Description);
        }
        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Get_Default_ArrayOfVarchar_Missing_Array_Ref()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""$ref"": ""#/definitions/Person""
                }
              },
              ""definitions"": {
                ""Person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                        ""type"": ""string""
                    },
                    ""age"": {
                        ""type"": ""integer""
                    },
                    ""top3favoritenumbers"": {
                        ""type"": ""array"",
                        ""description"": ""top favorite numbers""
                    }
                  }
                },
                ""top3favoritenumbers"": {
                  ""type"": ""integer""
                }
              }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Definitions.First(w => w.Key == "Person").Value.Properties.First(p => p.Key == "top3favoritenumbers").ToDto(dtoList, ref position);
            BaseFieldDto dtoField = dtoList.First();

            //Assertion
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoField, typeof(VarcharFieldDto));
            Assert.AreEqual(true, dtoField.IsArray);
            Assert.AreEqual("top3favoritenumbers", dtoField.Name);
            Assert.AreEqual("top favorite numbers", dtoField.Description);
        }

        [TestMethod, TestCategory("ToDto JsonSchemaProperty")]
        public void Get_Default_Varchar_For_Boolean()
        {
            //Setup            
            JsonSchema schema = BuildMockJsonSchema_ObjectProperty_Based();
            List<BaseFieldDto> dtoList = new List<BaseFieldDto>();
            int position = 1;

            //Action
            schema.Properties.First(w => w.Key == "person").ToDto(dtoList, ref position);

            //Assertion            
            Assert.AreEqual(1, dtoList.Count);
            Assert.IsInstanceOfType(dtoList.First().ChildFields.First(w => w.Name == "haschildren"), typeof(VarcharFieldDto));
            Assert.AreEqual(false, dtoList.First().ChildFields.First(w => w.Name == "haschildren").IsArray);
        }

        [TestMethod, TestCategory("FindArraySchema JsonSchemaProperty")]
        public void Get_String_JsonSchema__Mulitple_Array_Items()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""$ref"": ""#/definitions/Person""
                }
              },
              ""definitions"": {
                ""Person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                        ""type"": ""string""
                    },
                    ""age"": {
                        ""type"": ""integer""
                    },
                    ""top3favoritenumbers"": {
                        ""type"": ""array"",
                        ""items"": [
                            {
                                ""type"": ""string""
                            },
                            {
                                ""type"": ""integer""
                            }
                        ]
                    }
                  }
                }
              }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();

            //Action
            JsonSchema outSchema = schema.Definitions.First(w => w.Key == "Person").Value.Properties.First(p => p.Key == "top3favoritenumbers").FindArraySchema();
            JsonSchema expectSchema = schema.Definitions.First(w => w.Key == "Person").Value.Properties.First(p => p.Key == "top3favoritenumbers").Value.Items.First();

            //Assertion
            Assert.AreEqual(outSchema, expectSchema);
            Assert.AreEqual(JsonObjectType.String, outSchema.Type);
        }


        [TestMethod, TestCategory("FindArraySchema JsonSchemaProperty")]
        public void Get_Reference_JsonSchema__Array_Item_Reference()
        {
            //Setup
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""$ref"": ""#/definitions/Person""
                }
              },
              ""definitions"": {
                    ""Person"": {
                      ""type"": ""object"",
                      ""properties"": {
                            ""name"": {
                                ""type"": ""string""
                            },
                            ""age"": {
                                ""type"": ""integer""
                            },
                            ""children"": {
                                ""type"": ""array"",
                                ""items"": { ""$ref"": ""#/definitions/Child"" }
                            }
                        }
                    },
                    ""top3favoritenumbers"": {
                        ""type"": ""string""
                    },
                    ""Child"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": {
                                ""type"": ""string""
                            }
                        }
                    }
                }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();

            //Action
            JsonSchema outSchema = schema.Definitions.First(w => w.Key == "Person").Value.Properties.First(p => p.Key == "children").FindArraySchema();
            JsonSchema expectSchema = schema.Definitions.First(w => w.Key == "Child").Value;

            //Assertion
            Assert.AreEqual(outSchema, expectSchema);
            Assert.AreEqual(JsonObjectType.Object, outSchema.Type);
        }

        private JsonSchema BuildMockJsonSchema_ObjectProperty_Based()
        {
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                      ""type"": ""string""
                    },
                    ""age"": {
                      ""type"": ""integer""
                    },
                    ""top3favoritecolors"": {
                        ""type"": ""array"",
                        ""items"": {
                            ""type"": ""string"",
                            ""maxlength"": 391
                        },
                        ""description"": ""Array of VARCHAR""
                    },
                    ""haschildren"": {
                        ""type"": ""boolean"",
                        ""description"": ""Indicator whether person has children""
                    }                    
                  }
                }
              }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }

        private JsonSchema BuildMockJsonSchema_ObjectReference_Based()
        {
            string jsonSchema = @"{
              ""type"": ""object"",
              ""properties"": {
                ""person"": {
                  ""$ref"": ""#/definitions/Person""
                }
              },
              ""definitions"": {
                ""Person"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""name"": {
                      ""type"": ""string""
                    },
                    ""age"": {
                      ""type"": ""integer""
                    },
                    ""top3favoritecolors"": {
                        ""type"": ""array"",
                        ""$ref"": ""#/definitions/top3favoritecolors""
                    },
                    ""haschildren"": {
                        ""type"": ""boolean"",
                        ""description"": ""Indicator whether person has children""
                    }
                  }
                },
                ""top3favoritecolors"": {
                  ""type"": ""string""
                }
              }
            }";

            JsonSchema schema = JsonSchema.FromJsonAsync(jsonSchema).GetAwaiter().GetResult();
            return schema;
        }
    }
}
