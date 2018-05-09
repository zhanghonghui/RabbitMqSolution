// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Reflection.Services.Common.ExpressionUtil
{
    internal delegate TValue Hoisted<in TModel, out TValue>(TModel model, List<object> capturedConstants);
}
