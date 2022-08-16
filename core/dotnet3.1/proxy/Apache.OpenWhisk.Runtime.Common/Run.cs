/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Apache.OpenWhisk.Runtime.Common
{
    public class Run
    {
        private readonly Type _type;
        private readonly MethodInfo _method;
        private readonly ConstructorInfo _constructor;
        private readonly bool _awaitableMethod;

        public Run(Type type, MethodInfo method, ConstructorInfo constructor, bool awaitableMethod)
        {
            _type = type;
            _method = method;
            _constructor = constructor;
            _awaitableMethod = awaitableMethod;
        }

        public async Task HandleRequest(HttpContext httpContext)
        {
            if (_type == null || _method == null || _constructor == null)
            {
                await httpContext.Response.WriteError("Cannot invoke an uninitialized action.");
                return;
            }

            try
            {
                string body = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();

                JObject inputObject = string.IsNullOrEmpty(body) ? null : JObject.Parse(body);

                JObject valObject = null;
                JArray valArray = null;

                if (inputObject != null)
                {
                    valObject = inputObject["value"] as JObject;
                    foreach (JToken token in inputObject.Children())
                    {
                        try
                        {
                            if (token.Path.Equals("value", StringComparison.InvariantCultureIgnoreCase))
                                continue;
                            string envKey = $"__OW_{token.Path.ToUpperInvariant()}";
                            string envVal = token.First.ToString();
                            Environment.SetEnvironmentVariable(envKey, envVal);
                            //Console.WriteLine($"Set environment variable \"{envKey}\" to \"{envVal}\".");
                        }
                        catch (Exception)
                        {
                            await Console.Error.WriteLineAsync(
                                $"Unable to set environment variable for the \"{token.Path}\" token.");
                        }
                    }
                    if (valObject == null) {
                        valArray = inputObject["value"] as JArray;
                    }
                }

                object owObject = _constructor.Invoke(new object[] { });

                try
                {
                    JContainer output;

                    if(_awaitableMethod) {
                        if (valObject != null) {
                            output = (JContainer) await (dynamic) _method.Invoke(owObject, new object[] {valObject});
                        } else {
                            output = (JContainer) await (dynamic) _method.Invoke(owObject, new object[] {valArray});
                        }
                    }
                    else {
                        if (valObject != null) {
                            output = (JContainer) _method.Invoke(owObject, new object[] {valObject});
                        } else {
                            output = (JContainer) _method.Invoke(owObject, new object[] {valArray});
                        }
                    }

                    if (output == null)
                    {
                        await httpContext.Response.WriteError("The action returned null");
                        Console.Error.WriteLine("The action returned null");
                        return;
                    }

                    await httpContext.Response.WriteResponse(200, output.ToString());
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                    await httpContext.Response.WriteError(ex.Message
#if DEBUG
                                                          + ", " + ex.StackTrace
#endif
                    );
                }
            }
            finally
            {
                Startup.WriteLogMarkers();
            }
        }
    }
}
