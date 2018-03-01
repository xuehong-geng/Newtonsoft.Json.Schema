﻿#region License
// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema.Infrastructure.Validation
{
    internal class IfThenElseScope : ConditionalScope
    {
        public JSchema If;
        public JSchema Then;
        public JSchema Else;

        public override void Initialize(ContextBase context, SchemaScope parent, int initialDepth, ScopeType type)
        {
            base.Initialize(context, parent, initialDepth, type);

            If = null;
            Then = null;
            Else = null;
        }

        protected override bool EvaluateTokenCore(JsonToken token, object value, int depth)
        {
            SchemaScope ifScope = GetSchemaScopeBySchema(If);

            if (ifScope.IsValid)
            {
                if (Then != null)
                {
                    SchemaScope thenScope = GetSchemaScopeBySchema(Then);
                    if (!thenScope.IsValid)
                    {
                        ConditionalContext context = (ConditionalContext)thenScope.Context;
                        RaiseError($"JSON does not match schema from 'then'.", ErrorType.Then, Then, null, GetSchemaValidationErrors(context.Errors, Then));
                    }
                }
            }
            else
            {
                if (Else != null)
                {
                    SchemaScope elseScope = GetSchemaScopeBySchema(Else);
                    if (!elseScope.IsValid)
                    {
                        ConditionalContext context = (ConditionalContext)elseScope.Context;
                        RaiseError($"JSON does not match schema from 'else'.", ErrorType.Else, Else, null, GetSchemaValidationErrors(context.Errors, Else));
                    }
                }
            }

            return true;
        }

        private List<ValidationError> GetSchemaValidationErrors(List<ValidationError> errors, JSchema schema)
        {
            List<ValidationError> schemaValidationErrors = new List<ValidationError>();
            foreach (ValidationError validationError in errors)
            {
                if (validationError.Schema == schema)
                {
                    schemaValidationErrors.Add(validationError);
                }
            }

            return schemaValidationErrors;
        }

        internal override bool? IsValid()
        {
            return null;
        }

        public List<JSchema> GetSchemas()
        {
            List<JSchema> schemas = new List<JSchema> { If };
            if (Then != null)
            {
                schemas.Add(Then);
            }
            if (Else != null)
            {
                schemas.Add(Else);
            }

            return schemas;
        }
    }
}