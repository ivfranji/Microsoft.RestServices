﻿namespace Exchange.RestServices
{
    /// <summary>
    /// Int filter formatter.
    /// </summary>
    internal sealed class IntFilterFormatter : BaseFilterFormatter
    {
        /// <inheritdoc cref="BaseFilterFormatter.Type"/>
        public override string Type
        {
            get { return typeof(int).FullName; }
        }

        /// <inheritdoc cref="BaseFilterFormatter.FormatInternal"/>
        protected override string FormatInternal(object obj, FilterOperator filterOperator, PropertyDefinition propertyDefinition)
        {
            return this.FormatString(
                obj.ToString(),
                filterOperator,
                propertyDefinition.Name);
        }
    }
}