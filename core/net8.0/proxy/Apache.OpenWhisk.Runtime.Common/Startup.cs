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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Apache.OpenWhisk.Runtime.Common
{
    public class Startup
    {
        public static void WriteLogMarkers()
        {
            Console.WriteLine("XXX_THE_END_OF_A_WHISK_ACTIVATION_XXX");
            Console.Error.WriteLine("XXX_THE_END_OF_A_WHISK_ACTIVATION_XXX");
        }

        public void Configure(IApplicationBuilder app)
        {
            PathString initPath = new PathString("/init");
            PathString runPath = new PathString("/run");
            Init init = new Init();
            Run run = null;
            app.Run(async (httpContext) =>
                {
                    if (httpContext.Request.Path.Equals(initPath))
                    {
                        run = await init.HandleRequest(httpContext);

                        if (run != null)
                            await httpContext.Response.WriteResponse(200, "OK");

                        return;
                    }

                    if (httpContext.Request.Path.Equals(runPath))
                    {
                        if (!init.Initialized)
                        {
                            await httpContext.Response.WriteError("Cannot invoke an uninitialized action.");
                            return;
                        }

                        if (run == null)
                        {
                            await httpContext.Response.WriteError("Cannot invoke an uninitialized action.");
                            return;
                        }

                        await run.HandleRequest(httpContext);
                    }
                }
            );
        }
    }
}
