using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reflection.Services.Common;

namespace Reflection.Services
{
    public class JobPerform
    {
        public object Perform(Job job)
        {
            JobActivator _activator = new JobActivator();

            using (var scope = _activator.BeginScope())
            {
                object instance = null;

                if (job == null)
                {
                    throw new InvalidOperationException("Can't perform a background job with a null job.");
                }

                if (!job.Method.IsStatic)
                {
                    instance = scope.Resolve(job.Type);

                    if (instance == null)
                    {
                        throw new InvalidOperationException(
                            $"JobActivator returned NULL instance of the '{job.Type}' type.");
                    }
                }

                var arguments = SubstituteArguments(job);
                var result = InvokeMethod(job, instance, arguments);

                return result;
            }
        }

        private static object InvokeMethod(Job job, object instance, object[] arguments)
        {
            try
            {
                var methodInfo = job.Method;
                var result = methodInfo.Invoke(instance, arguments);

                var task = result as Task;

                if (task != null)
                {
                    task.Wait();

                    if (methodInfo.ReturnType.GetTypeInfo().IsGenericType)
                    {
                        var resultProperty = methodInfo.ReturnType.GetRuntimeProperty("Result");

                        result = resultProperty.GetValue(task);
                    }
                    else
                    {
                        result = null;
                    }
                }

                return result;
            }
            catch (ArgumentException ex)
            {
                throw;
            }
            catch (AggregateException ex)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                throw;
            }
        }

        private static object[] SubstituteArguments(Job job)
        {
            if (job == null)
            {
                return null;
            }

            var parameters = job.Method.GetParameters();
            var result = new List<object>(job.Args.Count);

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var argument = job.Args[i];

                var value = argument;

                result.Add(value);
            }

            return result.ToArray();
        }
    }
}
