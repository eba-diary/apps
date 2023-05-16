namespace Sentry.data.Core
{
    /// <summary>
    /// Implementation of the Visitor Pattern, in order to convert the polymorphic list of 
    /// <see cref="SchemaConsumption"/> entities into strongly-typed <see cref="SchemaConsumptionDto"/>s
    /// </summary>
    public class SchemaConsumptionDtoTransformer : ISchemaConsumptionVisitor<SchemaConsumptionDto>
    {
        public SchemaConsumptionDto Visit(SchemaConsumptionSnowflake consumptionDetails)
        {
            return new SchemaConsumptionSnowflakeDto()
            {
                SchemaConsumptionId = consumptionDetails.SchemaConsumptionId,
                SnowflakeWarehouse = consumptionDetails.SnowflakeWarehouse,
                SnowflakeDatabase = consumptionDetails.SnowflakeDatabase,
                SnowflakeSchema = consumptionDetails.SnowflakeSchema,
                SnowflakeStage = consumptionDetails.SnowflakeStage,
                SnowflakeStatus = consumptionDetails.SnowflakeStatus,
                SnowflakeTable = consumptionDetails.SnowflakeTable,
                SnowflakeType = consumptionDetails.SnowflakeType,
                LastChanged = consumptionDetails.LastChanged
            };
        }
    }
}
