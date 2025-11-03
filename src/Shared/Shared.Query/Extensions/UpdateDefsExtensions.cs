using MongoDB.Driver;
using Shared.Query.Interfaces;

namespace Shared.Query.Extensions;
public static class UpdateDefsExtensions
{
    /// <summary>
    /// Combines a SetOnInsert for the Id property with additional update definitions for upsert operations.
    /// </summary>
    ///
    /// <typeparam name="TQueryModel">The query model type which must implement <see cref="IQueryModel"/>.</typeparam>

    /// <param name="updateBuilder">The update definition builder used to construct update definitions.</param>

    /// <param name="id">The Id value to set on insert.</param>

    /// <param name="additionalUpdates">Additional update definitions to apply.</param>

    /// <returns>
    /// An <see cref="UpdateDefinition{TQueryModel}"/> that sets the Id on insert and combines any additional update definitions.
    /// </returns>

    public static UpdateDefinition<TQueryModel> UpsertWithId<TQueryModel>(
        this UpdateDefinitionBuilder<TQueryModel> updateBuilder,
        Guid id,
        params UpdateDefinition<TQueryModel>[] additionalUpdates
    ) where TQueryModel : IQueryModel
    {
        var updateDefinitions = new List<UpdateDefinition<TQueryModel>> { updateBuilder.SetOnInsert(x => x.Id, id) };
        if (additionalUpdates is { Length: > 0 }) updateDefinitions.AddRange(additionalUpdates);
        return updateBuilder.Combine(updateDefinitions);
    }
}
