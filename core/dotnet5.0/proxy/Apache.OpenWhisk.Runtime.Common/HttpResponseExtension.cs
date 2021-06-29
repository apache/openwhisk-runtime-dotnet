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

using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apache.OpenWhisk.Runtime.Common
{
    public static class HttpResponseExtension
    {
        public static async Task WriteResponse(this HttpResponse response, int code, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            response.ContentLength = bytes.Length;
            response.StatusCode = code;
            await response.WriteAsync(content);
        }

        public static async Task WriteError(this HttpResponse response, string errorMessage)
        {
            JObject message = new JObject {{"error", new JValue(errorMessage)}};
            await WriteResponse(response, 502, JsonConvert.SerializeObject(message));
        }

    }
}
