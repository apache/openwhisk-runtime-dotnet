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
using System.Text.Json.Nodes;

namespace Apache.OpenWhisk.Tests.Dotnet
{
    public class Init
    {
        public static string SOME_VAR = System.Environment.GetEnvironmentVariable("SOME_VAR");
        public static string ANOTHER_VAR = System.Environment.GetEnvironmentVariable("ANOTHER_VAR");

        public JsonObject Main(JsonObject args)
        {
            JsonObject message = new JsonObject();
            // an empty env variable is null, convert it to empty string to conform to test invariant
            message.Add("SOME_VAR", SOME_VAR != null ? SOME_VAR : "");
            message.Add("ANOTHER_VAR", ANOTHER_VAR != null ? ANOTHER_VAR : "");
            return (message);
        }
    }
}
