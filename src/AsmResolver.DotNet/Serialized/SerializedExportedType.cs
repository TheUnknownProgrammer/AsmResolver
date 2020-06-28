﻿using System;
using System.Collections.Generic;
using AsmResolver.Collections;
using AsmResolver.DotNet.Collections;
using AsmResolver.PE.DotNet.Metadata.Strings;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Represents a lazily initialized implementation of <see cref="ExportedType"/>  that is read from a
    /// .NET metadata image. 
    /// </summary>
    public class SerializedExportedType : ExportedType
    {
        private readonly SerializedModuleDefinition _parentModule;
        private readonly ExportedTypeRow _row;

        /// <summary>
        /// Creates a exported type from a exported type metadata row.
        /// </summary>
        /// <param name="parentModule">The module that contains the exported type.</param>
        /// <param name="token">The token to initialize the exported type for.</param>
        /// <param name="row">The metadata table row to base the exported type on.</param>
        public SerializedExportedType(SerializedModuleDefinition parentModule, MetadataToken token, ExportedTypeRow row)
            : base(token)
        {
            _parentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            _row = row;

            Attributes = row.Attributes;
            ((IOwnedCollectionElement<ModuleDefinition>) this).Owner = parentModule;
        }

        /// <inheritdoc />
        protected override string GetName() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Name);

        /// <inheritdoc />
        protected override string GetNamespace() => _parentModule.DotNetDirectory.Metadata
            .GetStream<StringsStream>()
            .GetStringByIndex(_row.Namespace);

        /// <inheritdoc />
        protected override IImplementation GetImplementation()
        {
            var encoder = _parentModule.DotNetDirectory.Metadata
                .GetStream<TablesStream>()
                .GetIndexEncoder(CodedIndex.Implementation);

            var token = encoder.DecodeIndex(_row.Implementation);
            return _parentModule.TryLookupMember(token, out var member)
                ? member as IImplementation
                : null;
        }

        /// <inheritdoc />
        protected override IList<CustomAttribute> GetCustomAttributes() =>
            _parentModule.GetCustomAttributeCollection(this);
    }
}