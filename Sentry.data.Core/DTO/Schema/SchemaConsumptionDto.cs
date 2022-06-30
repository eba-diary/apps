namespace Sentry.data.Core
{
    /// <summary>
    /// Represents how a Schema can be consumed
    /// </summary>
    public abstract class SchemaConsumptionDto
    {
        public int SchemaConsumptionId { get; set; }

        /// <summary>
        /// Method to accept a visitor to this class hierarchy
        /// </summary>
        /// <typeparam name="T">The superclass of the output of each Visit method</typeparam>
        /// <param name="v">The visitor class itself</param>
        /// <returns>An object of <see cref="T"/></returns>
        public abstract T Accept<T>(ISchemaConsumptionDtoVisitor<T> v);

    }
}
