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
namespace Apache.Openwhisk.Runtime.Minimal
{
    /// <summary>
    /// For more information, see https://github.com/apache/openwhisk/blob/master/docs/actions-new.md#initialization
    /// </summary>
    public class InitPostBody
    {
        public Value value { get; set; }
    }

    public class Value
    {
        public string name { get; set; }
        public string main { get; set; }
        public string code { get; set; }
        public bool binary { get; set; }
        public Env env { get; set; }
    }

    public class Env
    {
        public string __OW_API_KEY { get; set; }
        public string __OW_NAMESPACE { get; set; }
        public string __OW_ACTION_NAME { get; set; }
        public string __OW_ACTION_VERSION { get; set; }
        public string __OW_ACTIVATION_ID { get; set; }
        public long __OW_DEADLINE { get; set; }
    }

}
