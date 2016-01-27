using System;

namespace OrigoDB.Core.Modeling.Relational
{
    /// <summary>
    /// Entities must implement this interface
    /// </summary>
    public interface IEntity
    {
        Guid Id { get; set; }
        int Version { get; set; }
    }
}