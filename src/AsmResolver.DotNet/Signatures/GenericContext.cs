using System;

namespace AsmResolver.DotNet.Signatures
{
    /// <summary>
    /// Provides a context within a generic instantiation, including the type arguments of the enclosing type and method. 
    /// </summary>
    public readonly struct GenericContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="GenericContext"/> class.
        /// </summary>
        /// <param name="type">The type providing type arguments.</param>
        /// <param name="method">The method providing type arguments.</param>
        public GenericContext(IGenericArgumentsProvider type, IGenericArgumentsProvider method)
        {
            Type = type;
            Method = method;
        }
        
        /// <summary>
        /// Gets the object responsible for providing type arguments defined by the current generic type instantiation. 
        /// </summary>
        public IGenericArgumentsProvider Type
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for providing type arguments defined by the current generic method instantiation. 
        /// </summary>
        public IGenericArgumentsProvider Method
        {
            get;
        }

        /// <summary>
        /// Enters a new generic context with a new type providing type arguments. 
        /// </summary>
        /// <param name="type">The new type providing the type arguments.</param>
        /// <returns>The new generic context.</returns>
        public GenericContext WithType(IGenericArgumentsProvider type) => new GenericContext(type, Method);
        
        /// <summary>
        /// Enters a new generic context with a new method providing type arguments. 
        /// </summary>
        /// <param name="method">The new method providing the type arguments.</param>
        /// <returns>The new generic context.</returns>
        public GenericContext WithMethod(IGenericArgumentsProvider method) => new GenericContext(Type, method);

        /// <summary>
        /// Resolves a type parameter to a type argument, based on the current generic context.
        /// </summary>
        /// <param name="parameter">The parameter to get the argument value for.</param>
        /// <returns>The argument type.</returns>
        public TypeSignature GetTypeArgument(GenericParameterSignature parameter)
        {
            var argumentProvider = parameter.ParameterType switch
            {
                GenericParameterType.Type => Type,
                GenericParameterType.Method => Method,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            if (argumentProvider is null)
                throw new ArgumentOutOfRangeException();

            if (parameter.Index >= 0 && parameter.Index < argumentProvider.TypeArguments.Count)
                return argumentProvider.TypeArguments[parameter.Index];

            throw new ArgumentOutOfRangeException();
        }
    }
}