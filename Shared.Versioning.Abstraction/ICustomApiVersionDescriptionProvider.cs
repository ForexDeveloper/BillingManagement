namespace Shared.Versioning.Abstraction
{
    public interface ICustomApiVersionDescriptionProvider
    {
        IEnumerable<CustomApiVersionDescriptions> GetDescription();
    }
}
