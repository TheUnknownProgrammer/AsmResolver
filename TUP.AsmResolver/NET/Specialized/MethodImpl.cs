﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized
{
    public class MethodImplementation : MetaDataMember 
    {
        TypeDefinition @class;
        MetaDataMember methodBody;
        MetaDataMember methodDeclaration;

        public TypeDefinition Class
        {
            get
            {
                if (@class == null)
                {
                    int index = Convert.ToInt32(metadatarow.parts[0]);
                    MetaDataTable table = netheader.TablesHeap.GetTable(MetaDataTableType.TypeDef);
                    if (index > 0 && index <= table.members.Count)
                        @class = table.members[index - 1] as TypeDefinition;
                }
                return @class;
            }
        }
        public MethodReference MethodBody
        {
            get
            {
                if (methodBody == null)
                    netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(metadatarow.parts[1]), out methodBody);
                return methodBody as MethodReference;
            }
        }
        public MethodReference MethodDeclaration
        {
            get
            {
                if (methodBody == null)
                    netheader.TablesHeap.MethodDefOrRef.TryGetMember(Convert.ToInt32(metadatarow.parts[2]), out methodDeclaration);
                return methodDeclaration as MethodReference;
            }
        }
        public override void ClearCache()
        {
            @class = null;
            methodBody = null;
            methodDeclaration = null;
        }
    }
}
