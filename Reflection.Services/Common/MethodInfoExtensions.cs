// This file is part of Reflection.Services.
// Copyright © 2017 Sergey Odinokov.
// 
// Hangfire is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as 
// published by the Free Software Foundation, either version 3 
// of the License, or any later version.
// 
// Hangfire is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public 
// License along with Reflection.Services. If not, see <http://www.gnu.org/licenses/>.

using System.Linq;
using System.Reflection;

namespace Reflection.Services.Common
{
    internal static class MethodInfoExtensions
    {
        public static string GetNormalizedName(this MethodInfo methodInfo)
        {
            // Method names containing '.' are considered explicitly implemented interface methods
            // https://stackoverflow.com/a/17854048/1398672
            return methodInfo.Name.Contains(".") && methodInfo.IsFinal && methodInfo.IsPrivate
                ? methodInfo.Name.Split('.').Last()
                : methodInfo.Name;
        }
    }
}