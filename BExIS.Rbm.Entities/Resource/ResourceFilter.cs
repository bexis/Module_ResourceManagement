using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;
using BExIS.Rbm.Entities.ResourceStructure;

namespace BExIS.Rbm.Entities.Resource
{
    /// <summary>
    /// Join operator to combine filter.
    /// </summary>
    /// <remarks></remarks> 
    public enum Operator
    {
        AND, OR, XOR
    }

    /// <summary>
    /// Comparison Operator to combine filter expression left with filter expression right.
    /// </summary>
    /// <remarks></remarks> 
    public enum ComparisonOperator
    {
        equalTo, notEqualTo, graterThen, lessThen
    }

    public abstract class ResourceFilter : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Name of the filter.
        /// </summary>
        public virtual string Name { get; set; }

        #endregion

    }

    
    public class ResourceFilterExpression : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Left filter expression with represent a <see cref="ResourceStructureAttribute"/>.
        /// </summary>
        public virtual ResourceStructureAttribute FilterExpressionLeft { get; set; }

        /// <summary>
        /// Right filter expression with represent a <see cref="ResourceAttributeValue"/>.
        /// </summary>
        public virtual ResourceAttributeValue FilterExpressionRight { get; set; }

        /// <summary>
        /// The comparison operator between the filter expressions.
        /// </summary>
        public virtual ComparisonOperator ComparisonOperator { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    //example Filter: ResourceFilterExpression = (ResourceStructureAttribute equalTo ResourceAttributeValue) AND  ResourceFilterExpression = (ResourceStructureAttribute equalTo ResourceAttributeValue)
    public class ResourceFilterOperator : BaseEntity
    {
        #region Attributes

        /// <summary>
        /// Left resource filter expression with represent a <see cref="ResourceFilterExpression"/>.
        /// </summary>
        public virtual ResourceFilterExpression ResourceFilterExpressionLeft { get; set; }

        /// <summary>
        /// Right resource filter expression with represent a <see cref="ResourceFilterExpression"/>.
        /// </summary>
        public virtual ResourceFilterExpression ResourceFilterExpressionRight { get; set; }

        /// <summary>
        /// The comparison operator between the resource filter expressions.
        /// </summary>
        public virtual Operator Operator { get; set; }

        #endregion
    }
}
