namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides members for resolving references to members defined in external .NET assemblies.  
    /// </summary>
    public interface IMetadataResolver
    {
        /// <summary>
        /// Gets the object responsible for the resolution of external assemblies.
        /// </summary>
        IAssemblyResolver AssemblyResolver
        {
            get;
        }
        
        /// <summary>
        /// Resolves a reference to a type.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>The type definition, or <c>null</c> if the type could not be resolved.</returns>
        TypeDefinition ResolveType(ITypeDefOrRef type);
    }
}